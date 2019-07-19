using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Notifications;
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
        public ChatsViewModel(GroupMeClientApi.GroupMeClient groupMeClient)
        {
            this.GroupMeClient = groupMeClient;

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

        private PushClient PushClient { get; set; }

        private SemaphoreSlim ReloadGroupsSem { get; } = new SemaphoreSlim(1, 1);

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

                foreach (var group in this.GroupMeClient.Groups())
                {
                    var existingVm = this.AllGroupsChats.FirstOrDefault(g => g.Id == group.Id);

                    if (existingVm == null)
                    {
                        // create a new GroupControl ViewModel for this Group
                        var groupVm = new Controls.GroupControlViewModel(group)
                        {
                            GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
                        };
                        this.AllGroupsChats.Add(groupVm);
                    }
                    else
                    {
                        // Update the existing Group
                        existingVm.MessageContainer = group;
                    }
                }

                foreach (Chat chat in this.GroupMeClient.Chats())
                {
                    var existingVm = this.AllGroupsChats.FirstOrDefault(g => g.Id == chat.Id);

                    if (existingVm == null)
                    {
                        // create a new GroupControl ViewModel for this Chat
                        var chatVm = new Controls.GroupControlViewModel(chat)
                        {
                            GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
                        };
                        this.AllGroupsChats.Add(chatVm);
                    }
                    else
                    {
                        // Update the existing Chat
                        existingVm.MessageContainer = chat;
                    }
                }

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
                    CloseGroup = new RelayCommand<Controls.GroupContentsControlViewModel>(this.CloseChat, (g) => true),
                };

                this.ActiveGroupsChats.Insert(0, groupContentsDisplay);

                _ = this.PushClient.SubscribeAsync(group.MessageContainer);
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

        /// <inheritdoc/>
        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container)
        {
            _ = this.LoadGroupsAndChats();

            var groupId = notification.Message.GroupId;
            var groupVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == groupId);
            await groupVm?.LoadNewMessages();
        }

        /// <inheritdoc/>
        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container)
        {
            _ = this.LoadGroupsAndChats();

            var chatVm = this.ActiveGroupsChats.FirstOrDefault(c => c.Id == container.Id);
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
    }
}
