using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using GroupMeClient.Core.Utilities;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using GroupMeClientPlugin.GroupChat;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ReactiveUI;

namespace GroupMeClient.Core.ViewModels
{
    /// <summary>
    /// <see cref="SearchViewModel"/> provides a ViewModel for the <see cref="Controls.SearchView"/> view.
    /// </summary>
    public class SearchViewModel : ObservableObject, IPluginUIIntegration
    {
        private string searchTerm = string.Empty;
        private string selectedGroupName = string.Empty;
        private bool filterHasAttachedImage;
        private bool filterHasAttachedLinkedImage;
        private bool filterHasAttachedMentions;
        private bool filterHasAttachedVideo;
        private bool filterHasAttachedDocument;
        private DateTime filterStartDate;
        private DateTime filterEndDate = DateTime.Now.AddDays(1);
        private Member filterMessagesFrom;
        private bool filterIsExact;
        private bool filterIsANDTerms;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The client to use.</param>
        /// <param name="cacheManager">The cache database to use.</param>
        public SearchViewModel(GroupMeClientApi.GroupMeClient groupMeClient, Caching.CacheManager cacheManager)
        {
            this.GroupMeClient = groupMeClient ?? throw new ArgumentNullException(nameof(groupMeClient));
            this.CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));

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

            this.ResultsView = new PaginatedMessagesControlViewModel()
            {
                MessageSelectedCommand = new RelayCommand<MessageControlViewModelBase>(this.MessageSelected),
                ShowLikers = false,
                NewestAtBottom = false,
            };

            this.ContextView = new PaginatedMessagesControlViewModel()
            {
                ShowTitle = false,
                ShowLikers = true,
                SyncAndUpdate = true,
                NewestAtBottom = true,
            };

            this.ResetFilters = new RelayCommand<bool>(this.ResetFilterFields);

            this.Members = new ObservableCollection<Member>();

            this.Loaded = new RelayCommand(() => Task.Run(this.LoadIndexedGroups));
        }

        /// <summary>
        /// Gets the action that should be executed when the search page loads.
        /// </summary>
        public ICommand Loaded { get; private set; }

        /// <summary>
        /// Gets a view of the Groups and Chats that are sorted to display in the left-panel.
        /// </summary>
        public IObservableCollection<GroupControlViewModel> SortedGroupChats { get; private set; }

        /// <summary>
        /// Gets a collection of the <see cref="Member"/>s who have sent messages in the selected group or chat.
        /// </summary>
        public ObservableCollection<Member> Members { get; }

        /// <summary>
        /// Gets the action to be be performed when the big popup has been closed.
        /// </summary>
        public ICommand ClosePopup { get; }

        /// <summary>
        /// Gets the action to be be performed when the big popup has been closed indirectly.
        /// This typically is from the user clicking in the gray area around the popup to dismiss it.
        /// </summary>
        public ICommand EasyClosePopup { get; }

        /// <summary>
        /// Gets the action to be be performed to reset the search filters.
        /// </summary>
        public ICommand ResetFilters { get; }

        /// <summary>
        /// Gets the ViewModel for the paginated search results.
        /// </summary>
        public PaginatedMessagesControlViewModel ResultsView { get; }

        /// <summary>
        /// Gets the ViewModel for the in-context message view.
        /// </summary>
        public PaginatedMessagesControlViewModel ContextView { get; }

        /// <summary>
        /// Gets or sets the string entered to search for.
        /// </summary>
        public string SearchTerm
        {
            get => this.searchTerm;
            set => this.SetSearchProperty(ref this.searchTerm, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing an image attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedImage
        {
            get => this.filterHasAttachedImage;
            set => this.SetSearchProperty(ref this.filterHasAttachedImage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing a linked image attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedLinkedImage
        {
            get => this.filterHasAttachedLinkedImage;
            set => this.SetSearchProperty(ref this.filterHasAttachedLinkedImage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages mentioning other users should be shown.
        /// </summary>
        public bool FilterHasAttachedMentions
        {
            get => this.filterHasAttachedMentions;
            set => this.SetSearchProperty(ref this.filterHasAttachedMentions, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing a video attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedVideo
        {
            get => this.filterHasAttachedVideo;
            set => this.SetSearchProperty(ref this.filterHasAttachedVideo, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing a document attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedDocument
        {
            get => this.filterHasAttachedDocument;
            set => this.SetSearchProperty(ref this.filterHasAttachedDocument, value);
        }

        /// <summary>
        /// Gets or sets the beginning date for the time period of messages to display.
        /// </summary>
        public DateTime FilterStartDate
        {
            get => this.filterStartDate;
            set => this.SetSearchProperty(ref this.filterStartDate, value);
        }

        /// <summary>
        /// Gets or sets the ending date for the time period of messages to display.
        /// </summary>
        public DateTime FilterEndDate
        {
            get => this.filterEndDate;
            set => this.SetSearchProperty(ref this.filterEndDate, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the search term should be queried exactly.
        /// </summary>
        public bool FilterIsExact
        {
            get => this.filterIsExact;
            set
            {
                this.filterIsANDTerms = !value;
                this.SetSearchProperty(ref this.filterIsExact, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the search term should match messages containing
        /// each word in the query (word1 AND word2 AND word3).
        /// </summary>
        public bool FilterIsANDTerm
        {
            get => this.filterIsANDTerms;
            set
            {
                this.filterIsExact = !value;
                this.SetSearchProperty(ref this.filterIsANDTerms, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Member"/> from which to display messages.
        /// </summary>
        public Member FilterMessagesFrom
        {
            get => this.filterMessagesFrom;
            set => this.SetSearchProperty(ref this.filterMessagesFrom, value);
        }

        /// <summary>
        /// Gets the name of the selected group.
        /// </summary>
        public string SelectedGroupName
        {
            get => this.selectedGroupName;
            private set => this.SetProperty(ref this.selectedGroupName, value);
        }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }

        private Caching.CacheManager CacheManager { get; }

        private SourceList<GroupControlViewModel> AllGroupsChats { get; }

        private IMessageContainer SelectedGroupChat { get; set; }

        private bool DeferSearchUpdating { get; set; }

        private ReliabilityStateMachine ReliabilityStateMachine { get; } = new ReliabilityStateMachine();

        private Timer RetryTimer { get; set; }

        private bool LoadedCachedChats { get; set; } = false;

        /// <inheritdoc/>
        void IPluginUIIntegration.GotoContextView(Message message, IMessageContainer container)
        {
            var command = new Messaging.SwitchToPageRequestMessage(Messaging.SwitchToPageRequestMessage.Page.Search);
            WeakReferenceMessenger.Default.Send(command);

            this.OpenNewGroupChat(container, isHistorical: false);
            this.UpdateContextView(message);
        }

        private void SetSearchProperty<T>(ref T field, T newValue, [CallerMemberName] string callerName = "")
        {
            if (this.SetProperty(ref field, newValue, callerName))
            {
                this.UpdateSearchResults();
            }
        }

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
                this.LoadedCachedChats = false;

                foreach (var group in groupsAndChats)
                {
                    // Add Group/Chat to the list
                    var vm = new GroupControlViewModel(group)
                    {
                        GroupSelected = new RelayCommand<GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
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

            if (!this.LoadedCachedChats)
            {
                this.LoadedCachedChats = true;

                using (var cache = this.CacheManager.OpenNewContext())
                {
                    var cachedGroupsAndChats = cache.GetGroupsAndChats();
                    foreach (var cached in cachedGroupsAndChats)
                    {
                        var liveVersion = this.AllGroupsChats.Items.FirstOrDefault(g => g.Id == cached.Id);
                        if (liveVersion == null)
                        {
                            this.AllGroupsChats.Add(new GroupControlViewModel(cached)
                            {
                                GroupSelected = new RelayCommand<GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
                                IsHistorical = true,
                            });
                        }
                    }
                }
            }
        }

        private void OpenNewGroupChat(GroupControlViewModel group)
        {
            this.OpenNewGroupChat(group.MessageContainer, group.IsHistorical);
        }

        private void OpenNewGroupChat(IMessageContainer group, bool isHistorical)
        {
            this.SelectedGroupChat = group;

            this.ResetFilterFields(skipUpdating: true);

            this.SearchTerm = string.Empty;
            this.SelectedGroupName = group.Name;

            this.Members.Clear();
            if (this.SelectedGroupChat is Group g)
            {
                if (g.Members != null)
                {
                    foreach (var member in g.Members)
                    {
                        this.Members.Add(member);
                    }
                }
            }
            else if (this.SelectedGroupChat is Chat c)
            {
                this.Members.Add(c.OtherUser);

                try
                {
                    this.Members.Add(c.WhoAmI());
                }
                catch (Exception)
                {
                    // WhoAmI may not be available offline
                    var conversationIdParts = c.ConversationId.Split('+');
                    var meId = conversationIdParts.First(i => i != c.OtherUser.Id);
                    this.Members.Add(Placeholders.CreatePlaceholderMember(meId, "Me"));
                }
            }

            // Historical groups no longer exist on GroupMe, so likes can't be synchronized anymore.
            this.ContextView.SyncAndUpdate = !isHistorical;

            this.ContextView.DisplayMessages(null, null);
        }

        private void ResetFilterFields(bool skipUpdating = false)
        {
            this.DeferSearchUpdating = true;

            this.FilterStartDate = this.SelectedGroupChat?.CreatedAtTime.AddDays(-1) ?? DateTime.MinValue;
            this.FilterEndDate = DateTime.Now.AddDays(1);
            this.FilterHasAttachedImage = false;
            this.FilterHasAttachedLinkedImage = false;
            this.FilterHasAttachedMentions = false;
            this.FilterHasAttachedVideo = false;
            this.FilterHasAttachedDocument = false;

            this.FilterIsExact = true;

            this.DeferSearchUpdating = false;
            if (!skipUpdating)
            {
                this.UpdateSearchResults();
            }
        }

        private void MessageSelected(MessageControlViewModelBase message)
        {
            if (message != null)
            {
                this.UpdateContextView(message.Message);
            }
        }

        private void UpdateSearchResults()
        {
            if (this.DeferSearchUpdating)
            {
                return;
            }

            this.ResultsView.DisplayMessages(null, null);

            var cacheContext = this.CacheManager.OpenNewContext();
            var messagesForGroupChat = cacheContext.GetMessagesForGroup(this.SelectedGroupChat);

            var startDate = this.FilterStartDate;
            var endDate = (this.FilterEndDate == DateTime.MinValue) ? DateTime.Now : this.FilterEndDate.AddDays(1);

            var startDateUnix = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
            var endDateUnix = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

            IQueryable<Message> messagesFromMemberForGroupChat;
            if (this.FilterMessagesFrom == null)
            {
                // Show messages from all chat members
                messagesFromMemberForGroupChat = messagesForGroupChat;
            }
            else
            {
                var userId = this.FilterMessagesFrom.UserId;
                if (string.IsNullOrEmpty(userId) && this.SelectedGroupChat is Chat chat)
                {
                    // In Chats, the OtherUser field doesn't have the UserId set from GroupMe's API...
                    userId = chat.Id;
                }

                messagesFromMemberForGroupChat = messagesForGroupChat
                    .Where(m => m.UserId == userId);
            }

            var results = messagesFromMemberForGroupChat;

            // Apply query terms
            if (this.FilterIsExact)
            {
                results = results.Where(m => m.Text.ToLower().Contains(this.SearchTerm.ToLower()));
            }
            else if (this.FilterIsANDTerm)
            {
                foreach (var keyword in this.SearchTerm.ToLower().Split(' '))
                {
                    results = results.Where(m => m.Text.ToLower().Contains(keyword));
                }
            }

            // Apply time range filter
            results = results
                .Where(m => m.CreatedAtUnixTime >= startDateUnix)
                .Where(m => m.CreatedAtUnixTime <= endDateUnix);

            var filteredMessages = Enumerable.Empty<Message>().AsQueryable();
            var filtersApplied = false;

            // TODO: Can we disable Client Side evaluation for filters? Breaking change in Entity Framework Core 3
            // Enabling filters will be a lot faster if we can.
            if (this.FilterHasAttachedImage)
            {
                var messagesWithImages = results.AsEnumerable()
                    .Where(m => m.Attachments.OfType<ImageAttachment>().Count() >= 1);

                filteredMessages = filteredMessages.Union(messagesWithImages);
                filtersApplied = true;
            }

            if (this.FilterHasAttachedLinkedImage)
            {
                var messagesWithLinkedImages = results.AsEnumerable()
                    .Where(m => m.Attachments.OfType<LinkedImageAttachment>().Count() >= 1);

                filteredMessages = filteredMessages.Union(messagesWithLinkedImages);
                filtersApplied = true;
            }

            if (this.FilterHasAttachedVideo)
            {
                var messagesWithVideos = results.AsEnumerable()
                    .Where(m => m.Attachments.OfType<VideoAttachment>().Count() >= 1);

                filteredMessages = filteredMessages.Union(messagesWithVideos);
                filtersApplied = true;
            }

            if (this.FilterHasAttachedDocument)
            {
                var messagesWithDocuments = results.AsEnumerable()
                    .Where(m => m.Attachments.OfType<FileAttachment>().Count() >= 1);

                filteredMessages = filteredMessages.Union(messagesWithDocuments);
                filtersApplied = true;
            }

            if (this.FilterHasAttachedMentions)
            {
                var messagesWithMentions = results.AsEnumerable()
                    .Where(m => m.Attachments.OfType<MentionsAttachment>().Count() >= 1);

                filteredMessages = filteredMessages.Union(messagesWithMentions);
                filtersApplied = true;
            }

            if (!filtersApplied)
            {
                // No attachment filters were selected, so show all messages
                filteredMessages = results;
            }

            var orderedMessages = filteredMessages
                .OrderByDescending(m => m.Id);

            this.ResultsView.AssociateWith = this.SelectedGroupChat;
            this.ResultsView.DisplayMessages(orderedMessages, cacheContext);
            _ = this.ResultsView.LoadPage(0);
        }

        private void UpdateContextView(Message message)
        {
            this.ContextView.DisplayMessages(null, null);

            var cacheContext = this.CacheManager.OpenNewContext();

            var messagesForGroupChat = cacheContext.GetMessagesForGroup(this.SelectedGroupChat)
                .OrderBy(m => m.Id);

            this.ContextView.AssociateWith = this.SelectedGroupChat;
            this.ContextView.DisplayMessages(messagesForGroupChat, cacheContext);
            _ = this.ContextView.EnsureVisible(message);
        }
    }
}