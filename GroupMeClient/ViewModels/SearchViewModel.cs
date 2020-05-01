using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using GroupMeClientPlugin.GroupChat;
using Microsoft.EntityFrameworkCore;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="SearchViewModel"/> provides a ViewModel for the <see cref="Controls.SearchView"/> view.
    /// </summary>
    public class SearchViewModel : ViewModelBase, ICachePluginUIIntegration
    {
        private ViewModelBase popupDialog;
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

            this.ClosePopup = new RelayCommand(this.CloseLittlePopup);
            this.EasyClosePopup = new RelayCommand(this.CloseLittlePopup);
            this.ResetFilters = new RelayCommand<bool>(this.ResetFilterFields);

            this.GroupChatCachePlugins = new ObservableCollection<IGroupChatCachePlugin>();
            this.GroupChatCachePluginActivated =
                new RelayCommand<IGroupChatCachePlugin>(this.ActivateGroupPlugin);

            foreach (var plugin in Plugins.PluginManager.Instance.GroupChatCachePlugins)
            {
                this.GroupChatCachePlugins.Add(plugin);
            }

            this.Loaded = null; // new RelayCommand(this.StartIndexing);
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
        /// Gets the collection of ViewModels for <see cref="Message"/>s to be displayed.
        /// </summary>
        public ObservableCollection<IGroupChatCachePlugin> GroupChatCachePlugins { get; }

        /// <summary>
        /// Gets the action to be performed when a Plugin in the
        /// Options Menu is activated.
        /// </summary>
        public ICommand GroupChatCachePluginActivated { get; }

        /// <summary>
        /// Gets or sets the plugin that should be automatically executed when indexing is complete.
        /// This property is only used for UI-automation tasks. If null, the UI will be displayed normally
        /// when loading is complete. If a plugin is specified, the group specified in <see cref="ActivatePluginForGroupOnLoad"/>
        /// will be used as a parameter.
        /// </summary>
        public IGroupChatCachePlugin ActivatePluginOnLoad { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which group should be passed as a parameter to an automatically executed
        /// plugin. See <see cref="ActivatePluginOnLoad"/> for more information.
        /// </summary>
        public IMessageContainer ActivatePluginForGroupOnLoad { get; set; }

        /// <summary>
        /// Gets the Big Dialog that should be displayed as a popup.
        /// Gets null if no dialog should be displayed.
        /// </summary>
        public ViewModelBase PopupDialog
        {
            get { return this.popupDialog; }
            private set { this.Set(() => this.PopupDialog, ref this.popupDialog, value); }
        }

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

        private Caching.CacheManager.CacheContext CurrentlyDisplayedContext { get; set; }

        private IMessageContainer SelectedGroupChat { get; set; }

        private Task IndexingTask { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        private bool DeferSearchUpdating { get; set; }

        /// <inheritdoc/>
        void ICachePluginUIIntegration.GotoContextView(Message message, IMessageContainer container)
        {
            this.OpenNewGroupChat(container);
            this.UpdateContextView(message);
        }

        private void SetSearchProperty<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression, ref T field, T newValue)
        {
            this.Set(propertyExpression, ref field, newValue);
            this.UpdateSearchResults();
        }

        //private void StartIndexing()
        //{
        //    if (this.IndexingTask != null && !(this.IndexingTask.IsCompleted || this.IndexingTask.IsCanceled))
        //    {
        //        // handle cancellation and restart
        //        this.CancellationTokenSource.Cancel();
        //        this.IndexingTask.ContinueWith(async (l) =>
        //        {
        //            await Application.Current.Dispatcher.InvokeAsync(() => this.StartIndexing());
        //        });
        //        return;
        //    }

        //    this.CancellationTokenSource = new CancellationTokenSource();
        //    this.IndexingTask = this.IndexGroups();
        //}

        //private async Task IndexGroups()
        //{
        //    var loadingDialog = new LoadingControlViewModel();
        //    this.PopupDialog = loadingDialog;

        //    var groups = await this.GroupMeClient.GetGroupsAsync();
        //    var chats = await this.GroupMeClient.GetChatsAsync();
        //    var groupsAndChats = Enumerable.Concat<IMessageContainer>(groups, chats);

        //    this.AllGroupsChats.Clear();

        //    foreach (var group in groupsAndChats)
        //    {
        //        this.CancellationTokenSource.Token.ThrowIfCancellationRequested();

        //        if (this.ActivatePluginForGroupOnLoad != null && this.ActivatePluginOnLoad != null)
        //        {
        //            // if a plugin is set to automatically execute for only a single group,
        //            // index only that group to improve performance
        //            if (this.ActivatePluginForGroupOnLoad.Id != group.Id)
        //            {
        //                continue;
        //            }
        //        }

        //        loadingDialog.Message = $"Indexing {group.Name}";
        //        await this.IndexGroup(group);

        //        // Add Group/Chat to the list
        //        var vm = new GroupControlViewModel(group)
        //        {
        //            GroupSelected = new RelayCommand<GroupControlViewModel>((s) => this.OpenNewGroupChat(s.MessageContainer), (g) => true, true),
        //        };
        //        this.AllGroupsChats.Add(vm);
        //    }

        //    this.PopupDialog = null;

        //    // Check to see if a plugin should be automatically executed.
        //    if (this.ActivatePluginForGroupOnLoad != null && this.ActivatePluginOnLoad != null)
        //    {
        //        var cache = this.GetMessagesForGroup(this.ActivatePluginForGroupOnLoad);
        //        _ = this.ActivatePluginOnLoad.Activated(this.ActivatePluginForGroupOnLoad, cache, this);

        //        this.ActivatePluginForGroupOnLoad = null;
        //        this.ActivatePluginOnLoad = null;
        //    }
        //}

        private void OpenNewGroupChat(IMessageContainer group)
        {
            this.SelectedGroupChat = group;

            this.ResetFilterFields(skipUpdating: true);

            this.SearchTerm = string.Empty;
            this.SelectedGroupName = group.Name;
            this.ContextView.Messages = null;
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

        private IQueryable<Message> GetMessagesForGroup(IMessageContainer group)
        {
            this.CurrentlyDisplayedContext?.Dispose();
            this.CurrentlyDisplayedContext = this.CacheManager.OpenNewContext();
            if (group is Group g)
            {
                return this.CurrentlyDisplayedContext.Messages
                    .AsNoTracking()
                    .Where(m => m.GroupId == g.Id);
            }
            else if (group is Chat c)
            {
                // Chat.Id returns the Id of the other user
                // However, GroupMe messages are natively returned with a Conversation Id instead
                // Conversation IDs are user1+user2.
                var sampleMessage = c.Messages.FirstOrDefault();

                return this.CurrentlyDisplayedContext.Messages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == sampleMessage.ConversationId);
            }
            else
            {
                return Enumerable.Empty<Message>().AsQueryable();
            }
        }

        private void UpdateSearchResults()
        {
            if (this.DeferSearchUpdating)
            {
                return;
            }

            this.ResultsView.Messages = null;

            var messagesForGroupChat = this.GetMessagesForGroup(this.SelectedGroupChat);

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
            this.ResultsView.Messages = orderedMessages;
            this.ResultsView.ChangePage(0);
        }

        private void UpdateContextView(Message message)
        {
            this.ContextView.Messages = null;

            var messagesForGroupChat = this.GetMessagesForGroup(this.SelectedGroupChat)
                .OrderBy(m => m.Id);

            this.ContextView.AssociateWith = this.SelectedGroupChat;
            this.ContextView.Messages = messagesForGroupChat;
            this.ContextView.EnsureVisible(message);
        }

        private void ActivateGroupPlugin(IGroupChatCachePlugin plugin)
        {
            var cache = this.GetMessagesForGroup(this.SelectedGroupChat);
            _ = plugin.Activated(this.SelectedGroupChat, cache, this);
        }

        private void CloseLittlePopup()
        {
            if (this.PopupDialog is LoadingControlViewModel)
            {
                if (this.IndexingTask != null && !(this.IndexingTask.IsCompleted || this.IndexingTask.IsCanceled))
                {
                    // handle cancellation and restart
                    this.CancellationTokenSource.Cancel();
                    this.IndexingTask.ContinueWith((l) =>
                    {
                        Application.Current.Dispatcher.Invoke(() => this.CloseLittlePopup());
                    });
                    return;
                }
            }

            if (this.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.PopupDialog = null;
        }
    }
}