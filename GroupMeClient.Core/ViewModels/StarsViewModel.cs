﻿using System;
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
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Utilities;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ReactiveUI;

namespace GroupMeClient.Core.ViewModels
{
    /// <summary>
    /// <see cref="StarsViewModel"/> provides a ViewModel for the Starred Chats view.
    /// </summary>
    public class StarsViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarsViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The client to use.</param>
        /// <param name="cacheManager">The cache database to use.</param>
        /// <param name="persistManager">The persistant storage database to use.</param>
        /// <param name="settingsManager">The SettingsManager instance to load settings from.</param>
        public StarsViewModel(GroupMeClientApi.GroupMeClient groupMeClient, CacheManager cacheManager, PersistManager persistManager, SettingsManager settingsManager)
        {
            this.GroupMeClient = groupMeClient ?? throw new ArgumentNullException(nameof(groupMeClient));
            this.CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            this.PersistManager = persistManager ?? throw new ArgumentNullException(nameof(persistManager));
            this.SettingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));

            this.ActiveGroupsChats = new ObservableCollection<StarredMessageGroup>();

            this.AllGroupsChats = new SourceList<GroupControlViewModel>();
            this.SortedGroupChats = new ObservableCollectionExtended<GroupControlViewModel>();

            var updatedSort = this.AllGroupsChats
              .Connect()
              .WhenPropertyChanged(c => c.LastUpdated)
              .Throttle(TimeSpan.FromMilliseconds(250))
              .ObserveOn(RxApp.MainThreadScheduler)
              .Select(_ => Unit.Default);

            this.AllGroupsChats.AsObservableList()
                .Connect()
                .Sort(SortExpressionComparer<GroupControlViewModel>.Descending(g => g.LastUpdated), resort: updatedSort)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(this.SortedGroupChats)
                .Subscribe();

            this.Loaded = new RelayCommand(() => Task.Run(this.LoadIndexedGroups));
            this.CloseGroup = new RelayCommand<StarredMessageGroup>(this.CloseChat);
        }

        /// <summary>
        /// Gets the action that should be executed when the search page loads.
        /// </summary>
        public ICommand Loaded { get; }

        /// <summary>
        /// Gets the action that should be executed to close a group.
        /// </summary>
        public ICommand CloseGroup { get; }

        /// <summary>
        /// Gets a view of the Groups and Chats that are sorted to display in the left-panel.
        /// </summary>
        public IObservableCollection<GroupControlViewModel> SortedGroupChats { get; private set; }

        /// <summary>
        /// Gets a collection of all the Groups and Chats currently opened.
        /// </summary>
        public ObservableCollection<StarredMessageGroup> ActiveGroupsChats { get; private set; }

        /// <summary>
        /// Gets the action to be be performed when the big popup has been closed.
        /// </summary>
        public ICommand ClosePopup { get; }

        /// <summary>
        /// Gets the action to be be performed when the big popup has been closed indirectly.
        /// This typically is from the user clicking in the gray area around the popup to dismiss it.
        /// </summary>
        public ICommand EasyClosePopup { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }

        private CacheManager CacheManager { get; }

        private PersistManager PersistManager { get; }

        private SettingsManager SettingsManager { get; }

        private ReliabilityStateMachine ReliabilityStateMachine { get; } = new ReliabilityStateMachine();

        private Timer RetryTimer { get; set; }

        private SourceList<GroupControlViewModel> AllGroupsChats { get; }

        private async Task LoadIndexedGroups()
        {
            try
            {
                await this.GroupMeClient.GetGroupsAsync();
                await this.GroupMeClient.GetChatsAsync();
                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());

                this.CacheManager.SuperIndexer.BeginAsyncTransaction(groupsAndChats);
                this.CacheManager.SuperIndexer.EndTransaction();

                this.AllGroupsChats.Clear();

                foreach (var group in groupsAndChats)
                {
                    // Add Group/Chat to the list
                    var vm = new GroupControlViewModel(group)
                    {
                        GroupSelected = new RelayCommand<GroupControlViewModel>((s) => this.OpenNewGroupChat(s.MessageContainer), (g) => true),
                    };
                    this.AllGroupsChats.Add(vm);
                }

                this.ReliabilityStateMachine.Succeeded();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in {nameof(this.LoadIndexedGroups)} - {ex.Message}. Retrying...");
                this.RetryTimer = this.ReliabilityStateMachine.GetRetryTimer(async () => await this.LoadIndexedGroups());
            }
        }

        private void OpenNewGroupChat(IMessageContainer group)
        {
            if (this.ActiveGroupsChats.Any(g => g.MessageContainer.Id == group.Id))
            {
                // this group or chat is already open, we just need to move it to the front
                var openGroup = this.ActiveGroupsChats.First(g => g.MessageContainer.Id == group.Id);
                var indexOpenGroup = this.ActiveGroupsChats.IndexOf(openGroup);
                this.ActiveGroupsChats.Move(indexOpenGroup, 0);
            }
            else
            {
                // open a new group or chat
                var starGroup = new StarredMessageGroup(group, this.CacheManager, this.PersistManager);
                this.ActiveGroupsChats.Insert(0, starGroup);
            }

            // Limit to three multi-chats at a time
            // But, only close a single chat. Users will not expect multiple
            // chats to close at once. This could occur if the user opened several chats in MiniBar mode,
            // and then switched back to regular mode.
            var maximumChats = this.SettingsManager.UISettings.MaximumNumberOfMultiChatsNormal;
            if (this.ActiveGroupsChats.Count > maximumChats)
            {
                var removeGroup = this.ActiveGroupsChats.Last();

                this.ActiveGroupsChats.Remove(removeGroup);
            }
        }

        private void CloseChat(StarredMessageGroup paginatedDisplay)
        {
            this.ActiveGroupsChats.Remove(paginatedDisplay);
        }

        /// <summary>
        /// <see cref="StarredMessageGroup"/> provides a displayable collection of starred messages for a specific <see cref="IMessageContainer"/>.
        /// </summary>
        public class StarredMessageGroup : ObservableObject
        {
            private bool isShowingStars;
            private bool isShowingHidden;
            private string type;
            private bool isEmpty;

            /// <summary>
            /// Initializes a new instance of the <see cref="StarredMessageGroup"/> class.
            /// </summary>
            /// <param name="messageContainer">The container to display starred messages for.</param>
            /// <param name="cacheManager">The CacheManager instance to load messages from.</param>
            /// <param name="persistManager">The PersistManager instance to load star and hide data from.</param>
            public StarredMessageGroup(IMessageContainer messageContainer, CacheManager cacheManager, PersistManager persistManager)
            {
                this.TopBarAvatar = new AvatarControlViewModel(messageContainer, messageContainer.Client.ImageDownloader);
                this.MessageContainer = messageContainer;

                this.MessagesList = new PaginatedMessagesControlViewModel()
                {
                    AssociateWith = messageContainer,
                    ShowLikers = null,
                    SyncAndUpdate = false,
                    ShowTitle = false,
                    NewestAtBottom = true,
                };

                this.CacheManager = cacheManager;
                this.PersistManager = persistManager;

                this.IsShowingStars = true;
                this.IsShowingHidden = false;
            }

            /// <summary>
            /// Gets the container starred messages are being shown from.
            /// </summary>
            public IMessageContainer MessageContainer { get; }

            /// <summary>
            /// Gets the paginated display of starred messages.
            /// </summary>
            public PaginatedMessagesControlViewModel MessagesList { get; }

            /// <summary>
            /// Gets the group or chat avatar to display in the top bar.
            /// </summary>
            public AvatarControlViewModel TopBarAvatar { get; }

            /// <summary>
            /// Gets the type of messages being shown.
            /// </summary>
            public string Type
            {
                get => this.type;
                private set => this.SetProperty(ref this.type, value);
            }

            /// <summary>
            /// Gets a value indicating whether no starred messages are present for this group.
            /// </summary>
            public bool IsEmpty
            {
                get => this.isEmpty;
                private set => this.SetProperty(ref this.isEmpty, value);
            }

            /// <summary>
            /// Gets or sets a value indicating whether starred messages are being shown.
            /// </summary>
            public bool IsShowingStars
            {
                get => this.isShowingStars;
                set
                {
                    this.SetProperty(ref this.isShowingStars, value);
                    if (value)
                    {
                        this.LoadStarredMessages();
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether hidden messages are being shown.
            /// </summary>
            public bool IsShowingHidden
            {
                get => this.isShowingHidden;
                set
                {
                    this.SetProperty(ref this.isShowingHidden, value);
                    if (value)
                    {
                        this.LoadHiddenMessages();
                    }
                }
            }

            private CacheManager CacheManager { get; }

            private PersistManager PersistManager { get; }

            private void LoadStarredMessages()
            {
                using (var persistContext = this.PersistManager.OpenNewContext())
                {
                    var cacheContext = this.CacheManager.OpenNewContext();
                    var starList = PersistManager.GetStarredMessagesForGroup(this.MessageContainer, persistContext);
                    var messagesList = new List<Message>();

                    foreach (var star in starList)
                    {
                        var msg = cacheContext.Messages.FirstOrDefault(m => m.Id == star.MessageId);
                        messagesList.Add(msg);
                    }

                    this.IsEmpty = messagesList.Count == 0;
                    this.Type = "Starred";

                    var sortedMessages = messagesList
                        .AsQueryable()
                        .OrderBy(m => m.CreatedAtUnixTime);

                    this.MessagesList.DisplayMessages(sortedMessages, cacheContext);
                    _ = this.MessagesList.LoadPage(0);
                }
            }

            private void LoadHiddenMessages()
            {
                using (var persistContext = this.PersistManager.OpenNewContext())
                {
                    var cacheContext = this.CacheManager.OpenNewContext();

                    var hiddenList = PersistManager.GetHiddenMessagesForGroup(this.MessageContainer, persistContext);
                    var messagesList = new List<Message>();

                    foreach (var hidden in hiddenList)
                    {
                        var msg = cacheContext.Messages.FirstOrDefault(m => m.Id == hidden.MessageId);
                        messagesList.Add(msg);
                    }

                    this.IsEmpty = messagesList.Count == 0;
                    this.Type = "Hidden";

                    var sortedMessages = messagesList
                        .AsQueryable()
                        .OrderBy(m => m.CreatedAtUnixTime);

                    this.MessagesList.DisplayMessages(sortedMessages, cacheContext);
                    _ = this.MessagesList.LoadPage(0);
                }
            }
        }
    }
}