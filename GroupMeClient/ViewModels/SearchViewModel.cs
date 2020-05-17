using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using Microsoft.EntityFrameworkCore;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="SearchViewModel"/> provides a ViewModel for the <see cref="Controls.SearchView"/> view.
    /// </summary>
    public class SearchViewModel : ViewModelBase, IPluginUIIntegration
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The client to use.</param>
        /// <param name="cacheManager">The cache database to use.</param>
        public SearchViewModel(GroupMeClientApi.GroupMeClient groupMeClient, Caching.CacheManager cacheManager)
        {
            this.GroupMeClient = groupMeClient ?? throw new ArgumentNullException(nameof(groupMeClient));
            this.CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));

            this.AllGroupsChats = new ObservableCollection<GroupControlViewModel>();

            this.ResultsView = new PaginatedMessagesControlViewModel(this.CacheManager)
            {
                MessageSelectedCommand = new RelayCommand<MessageControlViewModelBase>(this.MessageSelected),
                ShowLikers = false,
                NewestAtBottom = false,
            };

            this.ContextView = new PaginatedMessagesControlViewModel(this.CacheManager)
            {
                ShowTitle = false,
                ShowLikers = true,
                SyncAndUpdate = true,
                NewestAtBottom = true,
            };

            this.ResetFilters = new RelayCommand<bool>(this.ResetFilterFields);

            this.Loaded = new RelayCommand(async () => await this.LoadIndexedGroups(), true);
        }

        /// <summary>
        /// Gets the action that should be executed when the search page loads.
        /// </summary>
        public ICommand Loaded { get; private set; }

        /// <summary>
        /// Gets a listing of all available Groups and Chats.
        /// </summary>
        public ObservableCollection<GroupControlViewModel> AllGroupsChats { get; }

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
            set => this.SetSearchProperty(() => this.SearchTerm, ref this.searchTerm, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing an image attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedImage
        {
            get => this.filterHasAttachedImage;
            set => this.SetSearchProperty(() => this.FilterHasAttachedImage, ref this.filterHasAttachedImage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing a linked image attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedLinkedImage
        {
            get => this.filterHasAttachedLinkedImage;
            set => this.SetSearchProperty(() => this.FilterHasAttachedLinkedImage, ref this.filterHasAttachedLinkedImage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages mentioning other users should be shown.
        /// </summary>
        public bool FilterHasAttachedMentions
        {
            get => this.filterHasAttachedMentions;
            set => this.SetSearchProperty(() => this.FilterHasAttachedMentions, ref this.filterHasAttachedMentions, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing a video attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedVideo
        {
            get => this.filterHasAttachedVideo;
            set => this.SetSearchProperty(() => this.FilterHasAttachedVideo, ref this.filterHasAttachedVideo, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only messages containing a document attachment should be shown.
        /// </summary>
        public bool FilterHasAttachedDocument
        {
            get => this.filterHasAttachedDocument;
            set => this.SetSearchProperty(() => this.FilterHasAttachedDocument, ref this.filterHasAttachedDocument, value);
        }

        /// <summary>
        /// Gets or sets the beginning date for the time period of messages to display.
        /// </summary>
        public DateTime FilterStartDate
        {
            get => this.filterStartDate;
            set => this.SetSearchProperty(() => this.FilterStartDate, ref this.filterStartDate, value);
        }

        /// <summary>
        /// Gets or sets the ending date for the time period of messages to display.
        /// </summary>
        public DateTime FilterEndDate
        {
            get => this.filterEndDate;
            set => this.SetSearchProperty(() => this.FilterEndDate, ref this.filterEndDate, value);
        }

        /// <summary>
        /// Gets the name of the selected group.
        /// </summary>
        public string SelectedGroupName
        {
            get { return this.selectedGroupName; }
            private set { this.Set(() => this.SelectedGroupName, ref this.selectedGroupName, value); }
        }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }

        private Caching.CacheManager CacheManager { get; }

        private IMessageContainer SelectedGroupChat { get; set; }

        private bool DeferSearchUpdating { get; set; }

        /// <summary>
        /// Provides cleanup services after a plugin terminates with a specific <see cref="CacheSession"/>.
        /// </summary>
        /// <param name="cacheSession">The session used by the terminated plugin.</param>
        public static void CleanupPlugin(CacheSession cacheSession)
        {
            (cacheSession.Tag as Caching.CacheManager.CacheContext).Dispose();
        }

        /// <summary>
        /// Begins execution of a GroupMe plugin (<see cref="IGroupChatPlugin"/>) for a specific <see cref="IMessageContainer"/>.
        /// </summary>
        /// <param name="group">The <see cref="IMessageContainer"/> to execute the plugin on.</param>
        /// <param name="plugin">The plugin that should be executed.</param>
        public void RunPlugin(IMessageContainer group, IGroupChatPlugin plugin)
        {
            var cacheContext = this.CacheManager.OpenNewContext();

            var cacheSession = new CacheSession(
                 Caching.CacheManager.GetMessagesForGroup(group, cacheContext),
                 cacheContext.Messages.AsNoTracking(),
                 cacheContext);

            _ = plugin.Activated(group, cacheSession, this, CleanupPlugin);
        }

        /// <inheritdoc/>
        void IPluginUIIntegration.GotoContextView(Message message, IMessageContainer container)
        {
            var command = new Messaging.SwitchToPageRequestMessage(Messaging.SwitchToPageRequestMessage.Page.Search);
            Messenger.Default.Send(command);

            this.OpenNewGroupChat(container);
            this.UpdateContextView(message);
        }

        private void SetSearchProperty<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression, ref T field, T newValue)
        {
            this.Set(propertyExpression, ref field, newValue);
            this.UpdateSearchResults();
        }

        private async Task LoadIndexedGroups()
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
                    GroupSelected = new RelayCommand<GroupControlViewModel>((s) => this.OpenNewGroupChat(s.MessageContainer), (g) => true, true),
                };
                this.AllGroupsChats.Add(vm);
            }
        }

        private void OpenNewGroupChat(IMessageContainer group)
        {
            this.SelectedGroupChat = group;

            this.ResetFilterFields(skipUpdating: true);

            this.SearchTerm = string.Empty;
            this.SelectedGroupName = group.Name;
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
            var messagesForGroupChat = Caching.CacheManager.GetMessagesForGroup(this.SelectedGroupChat, cacheContext);

            var startDate = this.FilterStartDate;
            var endDate = (this.FilterEndDate == DateTime.MinValue) ? DateTime.Now : this.FilterEndDate.AddDays(1);

            var startDateUnix = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
            var endDateUnix = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

            var results = messagesForGroupChat
                .Where(m => m.Text.ToLower().Contains(this.SearchTerm.ToLower()))
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

            var messagesForGroupChat = Caching.CacheManager.GetMessagesForGroup(this.SelectedGroupChat, cacheContext)
                .OrderBy(m => m.Id);

            this.ContextView.AssociateWith = this.SelectedGroupChat;
            this.ContextView.DisplayMessages(messagesForGroupChat, cacheContext);
            _ = this.ContextView.EnsureVisible(message);
        }
    }
}