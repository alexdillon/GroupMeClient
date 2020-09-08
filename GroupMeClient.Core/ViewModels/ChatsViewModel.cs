using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Caching.Models;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Utilities;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;
using GroupMeClientPlugin.Notifications;
using ReactiveUI;

namespace GroupMeClient.Core.ViewModels
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
        /// <param name="cacheManager">The caching context for messages that should be used.</param>
        /// <param name="persistManager">The persistant storage should be used.</param>
        public ChatsViewModel(GroupMeClientApi.GroupMeClient groupMeClient, SettingsManager settingsManager, CacheManager cacheManager, PersistManager persistManager)
        {
            this.GroupMeClient = groupMeClient;
            this.SettingsManager = settingsManager;
            this.CacheManager = cacheManager;
            this.PersistManager = persistManager;

            this.AllGroupsChats = new SourceList<GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<GroupContentsControlViewModel>();

            Messenger.Default.Register<Messaging.ShowChatRequestMessage>(this, this.ShowChatRequest);

            this.MarkAllAsRead = new RelayCommand(this.MarkAllGroupsChatsRead);
            this.SearchToggled = new RelayCommand<bool>((t) => this.GroupChatFilter = t ? this.GroupChatFilter : string.Empty);

            this.SortedFilteredGroupChats = new ObservableCollectionExtended<GroupControlViewModel>();

            var filter = this.WhenValueChanged(t => t.GroupChatFilter)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Select(this.BuildGroupFilter);

            var updatedSort = this.AllGroupsChats
                .Connect()
                .WhenPropertyChanged(c => c.LastUpdated)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => Unit.Default);

            this.AllGroupsChats.AsObservableList()
                .Connect()
                .Filter(filter)
                .Sort(SortExpressionComparer<GroupControlViewModel>.Descending(g => g.LastUpdated), resort: updatedSort)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(this.SortedFilteredGroupChats)
                .Subscribe();

            Task.Run(async () => await this.Loaded());
        }

        /// <summary>
        /// Gets a view of the Groups and Chats that are sorted and filtered to
        /// display in the left-panel.
        /// </summary>
        public IObservableCollection<GroupControlViewModel> SortedFilteredGroupChats { get; private set; }

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
            get => this.groupChatFilter;
            set => this.Set(() => this.GroupChatFilter, ref this.groupChatFilter, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether minibar mode is enabled.
        /// </summary>
        public bool MiniBarModeEnabled
        {
            get => this.miniBarModeEnabled;
            set => this.Set(() => this.MiniBarModeEnabled, ref this.miniBarModeEnabled, value);
        }

        private SourceList<GroupControlViewModel> AllGroupsChats { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }

        private SettingsManager SettingsManager { get; }

        private CacheManager CacheManager { get; }

        private PersistManager PersistManager { get; }

        private PushClient PushClient { get; set; }

        private SemaphoreSlim ReloadGroupsSem { get; } = new SemaphoreSlim(1, 1);

        private ReliabilityStateMachine ReliabilityStateMachine { get; } = new ReliabilityStateMachine();

        private Timer RetryTimer { get; set; }

        /// <inheritdoc/>
        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container)
        {
            this.CacheManager.SuperIndexer.BeginAsyncTransaction();

            _ = Task.Run(async () => await this.LoadGroupsAndChats(true));

            var groupVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == container.Id);
            if (groupVm != null)
            {
                await groupVm.LoadNewMessages();
            }

            this.CacheManager.SuperIndexer.EndTransaction();
        }

        /// <inheritdoc/>
        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container)
        {
            _ = Task.Run(async () => await this.LoadGroupsAndChats(true));
            var chatVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == container.Id);
            if (chatVm != null)
            {
                await chatVm.LoadNewMessages();
            }

            this.CacheManager.SuperIndexer.EndTransaction();
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
            await this.LoadGroupsAndChats(true);
            foreach (var container in this.ActiveGroupsChats)
            {
                await container.LoadNewMessages();
            }

            this.CacheManager.SuperIndexer.EndTransaction();
        }

        private async Task Loaded()
        {
            this.AllGroupsChats.Clear();
            await this.LoadGroupsAndChats(true);
            this.CacheManager.SuperIndexer.EndTransaction();
            this.CheckForRestore();
        }

        private Func<GroupControlViewModel, bool> BuildGroupFilter(string searchText)
        {
            return group => group.Title.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async Task LoadGroupsAndChats(bool updateSuperIndexer = false)
        {
            await this.ReloadGroupsSem.WaitAsync();

            var persistContext = this.PersistManager.OpenNewContext();

            try
            {
                await this.GroupMeClient.GetGroupsAsync();
                await this.GroupMeClient.GetChatsAsync();

                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());

                if (updateSuperIndexer)
                {
                    // Submit new Group and Chat listings to SuperIndexer to allow for background indexing if needed.
                    this.CacheManager.SuperIndexer.BeginAsyncTransaction(groupsAndChats);
                }

                foreach (var group in groupsAndChats)
                {
                    // check the last-read message status from peristant storage
                    var groupState = persistContext.GroupChatStates.FirstOrDefault(g => g.GroupOrChatId == group.Id);
                    if (groupState == null)
                    {
                        groupState = new GroupOrChatState()
                        {
                            GroupOrChatId = group.Id,
                            LastTotalMessageCount = group.TotalMessageCount,
                        };
                        persistContext.GroupChatStates.Add(groupState);
                    }

                    // Code to update the UI needs to be run on the Application Dispatcher
                    // This is typically the case, but Timer events from ReliabilityStateMachine for
                    // retry-callbacks will NOT run on the original thread.
                    var uiDispatcher = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IUserInterfaceDispatchService>();
                    await uiDispatcher.InvokeAsync(() =>
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

                        var existingVm = this.AllGroupsChats.Items.FirstOrDefault(g => g.Id == group.Id);
                        if (existingVm == null)
                        {
                            // create a new GroupControl ViewModel for this Group
                            var vm = new GroupControlViewModel(group)
                            {
                                GroupSelected = new RelayCommand<GroupControlViewModel>((g) => this.OpenNewGroupChat(g), (g) => true, true),
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

                await persistContext.SaveChangesAsync();
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
                persistContext.Dispose();
                this.ReloadGroupsSem.Release();
            }
        }

        private void ShowChatRequest(Messaging.ShowChatRequestMessage requestMessage)
        {
            var groupOrChat = this.AllGroupsChats.Items.FirstOrDefault(g => g.Id == requestMessage.GroupOrChatId);
            if (groupOrChat != null)
            {
                this.OpenNewGroupChat(
                    group: groupOrChat,
                    skipClose: !this.SettingsManager.UISettings.StrictlyEnforceMultiChatLimits);
            }
        }

        private void OpenNewGroupChat(GroupControlViewModel group, bool skipClose = false)
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
                var groupContentsDisplay = new GroupContentsControlViewModel(group.MessageContainer, this.SettingsManager)
                {
                    CloseGroup = new RelayCommand<GroupContentsControlViewModel>(this.CloseChat),
                };

                this.ActiveGroupsChats.Insert(0, groupContentsDisplay);

                Task.Run(async () => await this.PushClient.SubscribeAsync(group.MessageContainer));

                // mark all messages as read
                this.MarkGroupChatAsRead(group);

                this.PublishTotalUnreadCount();
            }

            if (!skipClose)
            {
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
            }

            this.SaveRestoreState();
        }

        private void CloseChat(GroupContentsControlViewModel groupContentsControlViewModel)
        {
            this.ActiveGroupsChats.Remove(groupContentsControlViewModel);
            this.PushClient.Unsubscribe(groupContentsControlViewModel.MessageContainer);

            ((IDisposable)groupContentsControlViewModel).Dispose();
            this.SaveRestoreState();
        }

        private void MarkAllGroupsChatsRead()
        {
            foreach (var groupChatVm in this.AllGroupsChats.Items)
            {
                this.MarkGroupChatAsRead(groupChatVm);
            }

            this.SettingsManager.SaveSettings();
            this.PublishTotalUnreadCount();
        }

        private void MarkGroupChatAsRead(GroupControlViewModel groupChatVm)
        {
            using (var persistContext = this.PersistManager.OpenNewContext())
            {
                // mark all messages as read
                var groupChatState = persistContext.GroupChatStates.FirstOrDefault(g => g.GroupOrChatId == groupChatVm.Id);
                groupChatState.LastTotalMessageCount = groupChatVm.MessageContainer.TotalMessageCount;
                persistContext.SaveChanges();
            }

            // clear the notification bubble
            groupChatVm.TotalUnreadCount = 0;
        }

        private void PublishTotalUnreadCount()
        {
            var count = 0;

            foreach (var group in this.AllGroupsChats.Items)
            {
                count += group.TotalUnreadCount;
            }

            var updateRequest = new Messaging.UnreadRequestMessage(count);
            Messenger.Default.Send(updateRequest);
        }

        private void CheckForRestore()
        {
            var restoreService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IRestoreService>();
            if (restoreService.ShouldRestoreState)
            {
                using (var persistContext = this.PersistManager.OpenNewContext())
                {
                    var state = this.PersistManager.GetDefaultRecoveryState(persistContext);

                    var openChats = state.OpenChats.ToList();
                    openChats.Reverse();

                    foreach (var chatId in openChats)
                    {
                        var chat = this.AllGroupsChats.Items.First(c => c.Id == chatId);
                        this.OpenNewGroupChat(chat);
                    }
                }
            }
        }

        private void SaveRestoreState()
        {
            using (var persistContext = this.PersistManager.OpenNewContext())
            {
                var state = this.PersistManager.GetDefaultRecoveryState(persistContext);
                state.OpenChats = new List<string>(this.ActiveGroupsChats.Select(x => x.Id));
                persistContext.SaveChanges();
            }
        }
    }
}
