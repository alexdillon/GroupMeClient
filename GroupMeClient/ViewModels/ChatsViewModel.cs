using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.Caching;
using GroupMeClient.Notifications;
using GroupMeClient.Settings;
using GroupMeClient.Utilities;
using GroupMeClient.ViewModels.Controls;
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
        private string groupChatFilter = string.Empty;
        private bool miniBarModeEnabled = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatsViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The API client that should be used.</param>
        /// <param name="settingsManager">The application settings manager.</param>
        /// <param name="cacheContext">The caching context for messages that should be used.</param>
        public ChatsViewModel(GroupMeClientApi.GroupMeClient groupMeClient, SettingsManager settingsManager, CacheContext cacheContext)
        {
            this.GroupMeClient = groupMeClient;
            this.SettingsManager = settingsManager;
            this.CacheContext = cacheContext;

            this.AllGroupsChats = new ObservableCollection<GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<GroupContentsControlViewModel>();

            this.MarkAllAsRead = new RelayCommand(this.MarkAllGroupsChatsRead);
            this.SearchToggled = new RelayCommand<bool>((t) => this.GroupChatFilter = t ? this.GroupChatFilter : string.Empty);

            this.SortedFilteredGroupChats = CollectionViewSource.GetDefaultView(this.AllGroupsChats);
            this.SortedFilteredGroupChats.SortDescriptions.Add(new SortDescription("LastUpdated", ListSortDirection.Descending));
            this.SortedFilteredGroupChats.Filter = o => (o as GroupControlViewModel).Title.ToLower().Contains(this.GroupChatFilter.ToLower());

            var liveCollection = this.SortedFilteredGroupChats as ICollectionViewLiveShaping;
            liveCollection.LiveSortingProperties.Add("LastUpdated");
            liveCollection.IsLiveSorting = true;

            _ = this.Loaded();
        }

        /// <summary>
        /// Gets a view of the Groups and Chats that are sorted and filtered to
        /// display in the left-panel.
        /// </summary>
        public ICollectionView SortedFilteredGroupChats { get; private set; }

        /// <summary>
        /// Gets a collection of all the Groups and Chats currently opened.
        /// </summary>
        public ObservableCollection<GroupContentsControlViewModel> ActiveGroupsChats { get; private set; }

        /// <summary>
        /// Gets the action to be performed to mark all Groups/Chats as "read".
        /// </summary>
        public ICommand MarkAllAsRead { get; }

        /// <summary>
        /// Gets the action to be performed when the Group Search box is toggled.
        /// </summary>
        public ICommand SearchToggled { get; }

        /// <summary>
        /// Gets or sets the string entered to filter the available groups or chat with.
        /// </summary>
        public string GroupChatFilter
        {
            get
            {
                return this.groupChatFilter;
            }

            set
            {
                this.Set(() => this.GroupChatFilter, ref this.groupChatFilter, value);
                this.SortedFilteredGroupChats.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether minibar mode is enabled.
        /// </summary>
        public bool MiniBarModeEnabled
        {
            get
            {
                return this.miniBarModeEnabled;
            }

            set
            {
                this.Set(() => this.MiniBarModeEnabled, ref this.miniBarModeEnabled, value);
            }
        }

        private ObservableCollection<GroupControlViewModel> AllGroupsChats { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }

        private SettingsManager SettingsManager { get; }

        private CacheContext CacheContext { get; }

        private PushClient PushClient { get; set; }

        private SemaphoreSlim ReloadGroupsSem { get; } = new SemaphoreSlim(1, 1);

        private ReliabilityStateMachine ReliabilityStateMachine { get; } = new ReliabilityStateMachine();

        private Timer RetryTimer { get; set; }

        /// <inheritdoc/>
        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container)
        {
            _ = this.LoadGroupsAndChats();

            var groupVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == container.Id);
            if (groupVm != null)
            {
                await groupVm.LoadNewMessages();
            }
        }

        /// <inheritdoc/>
        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container)
        {
            _ = this.LoadGroupsAndChats();
            var chatVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == container.Id);
            if (chatVm != null)
            {
                await chatVm.LoadNewMessages();
            }
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

        /// <summary>
        /// Refreshes all elements displayed in the Chats View Tab. This includes the sidebar,
        /// and any opened Groups or Chats.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshEverything()
        {
            await this.LoadGroupsAndChats();
            foreach (var container in this.ActiveGroupsChats)
            {
                await container.LoadNewMessages();
            }
        }

        private async Task Loaded()
        {
            this.AllGroupsChats.Clear();
            await this.LoadGroupsAndChats();
            this.CheckForRestore();
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
                            LastTotalMessageCount = group.TotalMessageCount,
                        };
                        this.SettingsManager.ChatsSettings.GroupChatStates.Add(groupState);
                    }

                    // Code to update the UI needs to be run on the Application Dispatcher
                    // This is typically the case, but Timer events from ReliabilityStateMachine for
                    // retry-callbacks will NOT run on the original thread.
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // calculate how many new messages have been added since the group/chat was last read
                        var unreadMessages = group.TotalMessageCount - groupState.LastTotalMessageCount;

                        if (unreadMessages < 0)
                        {
                            // strange errors can occur when the Group Listing lags behind the
                            // actual group contents. If this occurs, cancel and reload the sidebar.
                            this.RetryTimer = this.ReliabilityStateMachine.GetRetryTimer(async () => await this.LoadGroupsAndChats());
                            return;
                        }

                        var existingVm = this.AllGroupsChats.FirstOrDefault(g => g.Id == group.Id);
                        if (existingVm == null)
                        {
                            // create a new GroupControl ViewModel for this Group
                            var vm = new GroupControlViewModel(group)
                            {
                                GroupSelected = new RelayCommand<GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
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

                        var openChatGroup = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == group.Id);
                        if (openChatGroup != null)
                        {
                            // chat is open and already receiving new messages, so mark all messages as "read"
                            existingVm.TotalUnreadCount = 0;
                            groupState.LastTotalMessageCount = openChatGroup.MessageContainer.TotalMessageCount;
                        }
                    });
                }

                this.SettingsManager.SaveSettings();
                this.PublishTotalUnreadCount();

                // if everything was successful, reset the reliability monitor
                this.ReliabilityStateMachine.Succeeded();
                this.RetryTimer?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in {nameof(this.LoadGroupsAndChats)} - {ex.Message}. Retrying...");
                this.RetryTimer = this.ReliabilityStateMachine.GetRetryTimer(async () => await this.LoadGroupsAndChats());
            }
            finally
            {
                this.ReloadGroupsSem.Release();
            }
        }

        private void OpenNewGroupChat(GroupControlViewModel group)
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
                var groupContentsDisplay = new GroupContentsControlViewModel(group.MessageContainer, this.CacheContext, this.SettingsManager)
                {
                    CloseGroup = new RelayCommand<GroupContentsControlViewModel>(this.CloseChat),
                };

                this.ActiveGroupsChats.Insert(0, groupContentsDisplay);

                Task.Run(async () => await this.PushClient.SubscribeAsync(group.MessageContainer));

                // mark all messages as read
                this.MarkGroupChatAsRead(group);

                this.PublishTotalUnreadCount();
            }

            // Limit to three multi-chats at a time
            // But, only close a single chat. Users will not expect multiple
            // chats to close at once. This could occur if the user opened several chats in MiniBar mode,
            // and then switched back to regular mode.
            var maximumChats = this.MiniBarModeEnabled ? this.SettingsManager.UISettings.MaximumNumberOfMultiChatsMinibar : this.SettingsManager.UISettings.MaximumNumberOfMultiChatsNormal;
            if (this.ActiveGroupsChats.Count > maximumChats)
            {
                var removeGroup = this.ActiveGroupsChats.Last();
                this.PushClient.Unsubscribe(group.MessageContainer);

                this.ActiveGroupsChats.Remove(removeGroup);
            }

            this.SettingsManager.ChatsSettings.OpenChats.Clear();
            this.SettingsManager.ChatsSettings.OpenChats.AddRange(this.ActiveGroupsChats.Select(x => x.Id));

            // Save both the updated read status and the currently opened list
            this.SettingsManager.SaveSettings();
        }

        private void CloseChat(GroupContentsControlViewModel groupContentsControlViewModel)
        {
            this.ActiveGroupsChats.Remove(groupContentsControlViewModel);
            this.PushClient.Unsubscribe(groupContentsControlViewModel.MessageContainer);

            ((IDisposable)groupContentsControlViewModel).Dispose();

            this.SettingsManager.ChatsSettings.OpenChats.Clear();
            this.SettingsManager.ChatsSettings.OpenChats.AddRange(this.ActiveGroupsChats.Select(x => x.Id));
            this.SettingsManager.SaveSettings();
        }

        private void MarkAllGroupsChatsRead()
        {
            foreach (var groupChatVm in this.AllGroupsChats)
            {
                this.MarkGroupChatAsRead(groupChatVm);
            }

            this.SettingsManager.SaveSettings();
            this.PublishTotalUnreadCount();
        }

        private void MarkGroupChatAsRead(GroupControlViewModel groupChatVm)
        {
            // mark all messages as read
            var groupChatState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == groupChatVm.Id);
            groupChatState.LastTotalMessageCount = groupChatVm.MessageContainer.TotalMessageCount;

            // clear the notification bubble
            groupChatVm.TotalUnreadCount = 0;
        }

        private void PublishTotalUnreadCount()
        {
            var count = 0;

            foreach (var group in this.AllGroupsChats)
            {
                count += group.TotalUnreadCount;
            }

            var updateRequest = new Messaging.UnreadRequestMessage(count);
            Messenger.Default.Send(updateRequest);
        }

        private void CheckForRestore()
        {
            if (Environment.GetCommandLineArgs().Contains(Native.RecoveryManager.RestartCommandLine))
            {
                var openChats = this.SettingsManager.ChatsSettings.OpenChats.ToList();
                openChats.Reverse();

                foreach (var chatId in openChats)
                {
                    var chat = this.AllGroupsChats.First(c => c.Id == chatId);
                    this.OpenNewGroupChat(chat);
                }
            }
        }
    }
}
