using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Notifications;
using GroupMeClient.Settings;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="ChatsViewModel"/> provides a ViewModel for the Chats page in the GroupMe Desktop Client.
    /// </summary>
    public class ChatsViewModel : ViewModelBase, INotificationSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChatsViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The API client that should be used.</param>
        /// <param name="settingsManager">The application settings manager.</param>
        public ChatsViewModel(GroupMeClientApi.GroupMeClient groupMeClient, SettingsManager settingsManager)
        {
            this.GroupMeClient = groupMeClient;
            this.SettingsManager = settingsManager;

            this.AllGroupsChats = new ObservableCollection<Controls.GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<Controls.GroupContentsControlViewModel>();

            _ = this.Loaded();
        }

        /// <summary>
        /// Gets a collection of all the Groups and Chats displayed in the left sidebar.
        /// </summary>
        public ObservableCollection<Controls.GroupControlViewModel> AllGroupsChats { get; private set; }

        /// <summary>
        /// Gets a collection of all the Groups and Chats currently opened.
        /// </summary>
        public ObservableCollection<Controls.GroupContentsControlViewModel> ActiveGroupsChats { get; private set; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }

        private SettingsManager SettingsManager { get; }

        private PushClient PushClient { get; set; }

        private SemaphoreSlim ReloadGroupsSem { get; } = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container)
        {
            _ = this.LoadGroupsAndChats();

            var groupId = notification.Message.GroupId;
            var groupVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == groupId);

            if (groupVm != null)
            {
                // update the latest viewed message in the persistant state
                var groupState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == container.Id);
                groupState.LastTotalMessageCount = container.TotalMessageCount + 1; // add one for the new message, since the group hasn't been reloaded yet
                groupState.LastReadMessageId = notification.Message.Id;
                this.SettingsManager.SaveSettings();
            }

            await groupVm?.LoadNewMessages();
        }

        /// <inheritdoc/>
        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container)
        {
            _ = this.LoadGroupsAndChats();

            var chatVm = this.ActiveGroupsChats.FirstOrDefault(c => c.Id == container.Id);

            if (chatVm != null)
            {
                // update the latest viewed message in the persistant state
                var chatState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == container.Id);
                chatState.LastTotalMessageCount = container.TotalMessageCount + 1; // add one for the new message, since the group hasn't been reloaded yet
                chatState.LastReadMessageId = notification.Message.Id;
                this.SettingsManager.SaveSettings();
            }

            await chatVm?.LoadNewMessages();
        }

        /// <inheritdoc/>
        Task INotificationSink.MessageUpdated(Message message, string alert, IMessageContainer container)
        {
            var groupChatVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == container.Id);
            groupChatVm?.UpdateMessageLikes(message);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        void INotificationSink.HeartbeatReceived()
        {
        }

        /// <inheritdoc/>
        void INotificationSink.RegisterPushSubscriptions(PushClient pushClient, GroupMeClientApi.GroupMeClient client)
        {
            // Save the PushClient for Subscribing/Unsubscribing from sources later
            this.PushClient = pushClient;
        }

        private async Task Loaded()
        {
            this.AllGroupsChats.Clear();
            await this.LoadGroupsAndChats();
        }

        private async Task LoadGroupsAndChats()
        {
            await this.ReloadGroupsSem.WaitAsync();

            try
            {
                await this.GroupMeClient.GetGroupsAsync();
                await this.GroupMeClient.GetChatsAsync();

                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());

                foreach (var group in groupsAndChats)
                {
                    // check the last-read message status from peristant storage
                    var groupState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == group.Id);
                    if (groupState == null)
                    {
                        groupState = new ChatsSettings.GroupOrChatState()
                        {
                            GroupOrChatId = group.Id,
                            LastReadMessageId = group.LatestMessage.Id,
                            LastTotalMessageCount = group.TotalMessageCount,
                        };
                        this.SettingsManager.ChatsSettings.GroupChatStates.Add(groupState);
                    }

                    // calculate how many new messages have been added since the group/chat was last read
                    var unreadMessages = group.TotalMessageCount - groupState.LastTotalMessageCount;

                    var existingVm = this.AllGroupsChats.FirstOrDefault(g => g.Id == group.Id);
                    if (existingVm == null)
                    {
                        // create a new GroupControl ViewModel for this Group
                        var vm = new Controls.GroupControlViewModel(group)
                        {
                            GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
                            TotalUnreadCount = unreadMessages,
                        };
                        this.AllGroupsChats.Add(vm);
                    }
                    else
                    {
                        // Update the existing Group/Chat VM
                        existingVm.MessageContainer = group;
                        existingVm.TotalUnreadCount = unreadMessages;
                    }
                }

                this.SettingsManager.SaveSettings();
                await this.GroupMeClient.Update();
            }
            finally
            {
                this.ReloadGroupsSem.Release();
            }
        }

        private void OpenNewGroupChat(Controls.GroupControlViewModel group)
        {
            if (this.ActiveGroupsChats.Any(g => g.Id == group.Id))
            {
                // this group or chat is already open, we just need to move it to the front
                var openGroup = this.ActiveGroupsChats.First(g => g.Id == group.Id);
                var indexOpenGroup = this.ActiveGroupsChats.IndexOf(openGroup);
                this.ActiveGroupsChats.Move(indexOpenGroup, 0);
            }
            else
            {
                // open a new group or chat
                var groupContentsDisplay = new Controls.GroupContentsControlViewModel(group.MessageContainer)
                {
                    CloseGroup = new RelayCommand<Controls.GroupContentsControlViewModel>(this.CloseChat),
                };

                this.ActiveGroupsChats.Insert(0, groupContentsDisplay);

                _ = this.PushClient.SubscribeAsync(group.MessageContainer);

                // mark all messages as read
                var groupChatState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == group.Id);
                groupChatState.LastTotalMessageCount = group.MessageContainer.TotalMessageCount;
                groupChatState.LastReadMessageId = group.MessageContainer.LatestMessage.Id;

                // clear the notification bubble
                group.TotalUnreadCount = 0;

                this.SettingsManager.SaveSettings();
            }

            // limit to three multi-chats at a time
            while (this.ActiveGroupsChats.Count > 3)
            {
                var removeGroup = this.ActiveGroupsChats.Last();
                this.PushClient.Unsubscribe(group.MessageContainer);

                this.ActiveGroupsChats.Remove(removeGroup);
            }
        }

        private void CloseChat(Controls.GroupContentsControlViewModel groupContentsControlViewModel)
        {
            this.ActiveGroupsChats.Remove(groupContentsControlViewModel);
            this.PushClient.Unsubscribe(groupContentsControlViewModel.MessageContainer);

            ((IDisposable)groupContentsControlViewModel).Dispose();
        }
    }
}
