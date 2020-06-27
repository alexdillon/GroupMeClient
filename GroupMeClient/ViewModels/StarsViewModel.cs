using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Caching;
using GroupMeClient.Settings;
using GroupMeClient.Utilities;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="SearchViewModel"/> provides a ViewModel for the <see cref="Controls.SearchView"/> view.
    /// </summary>
    public class StarsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarsViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The client to use.</param>
        /// <param name="cacheManager">The cache database to use.</param>
        /// <param name="settingsManager">The SettingsManager instance to load settings from.</param>
        public StarsViewModel(GroupMeClientApi.GroupMeClient groupMeClient, Caching.CacheManager cacheManager, SettingsManager settingsManager)
        {
            this.GroupMeClient = groupMeClient ?? throw new ArgumentNullException(nameof(groupMeClient));
            this.CacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            this.SettingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));

            this.AllGroupsChats = new ObservableCollection<GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<StarredMessageGroup>();

            this.Loaded = new RelayCommand(async () => await this.LoadIndexedGroups(), true);
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
        /// Gets a listing of all available Groups and Chats.
        /// </summary>
        public ObservableCollection<GroupControlViewModel> AllGroupsChats { get; }

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

        private SettingsManager SettingsManager { get; }

        private ReliabilityStateMachine ReliabilityStateMachine { get; } = new ReliabilityStateMachine();

        private Timer RetryTimer { get; set; }

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
                        GroupSelected = new RelayCommand<GroupControlViewModel>((s) => this.OpenNewGroupChat(s.MessageContainer), (g) => true, true),
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
                var starGroup = new StarredMessageGroup(group, this.CacheManager);
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
        public class StarredMessageGroup
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StarredMessageGroup"/> class.
            /// </summary>
            /// <param name="messageContainer">The container to display starred messages for.</param>
            /// <param name="cacheManager">The CacheManager instance to load messages from.</param>
            public StarredMessageGroup(IMessageContainer messageContainer, CacheManager cacheManager)
            {
                this.TopBarAvatar = new AvatarControlViewModel(messageContainer, messageContainer.Client.ImageDownloader);
                this.MessageContainer = messageContainer;

                this.MessagesList = new PaginatedMessagesControlViewModel(cacheManager)
                {
                    AssociateWith = messageContainer,
                    ShowLikers = null,
                    SyncAndUpdate = false,
                    ShowTitle = false,
                    NewestAtBottom = true,
                };

                var context = cacheManager.OpenNewContext();
                var starList = CacheManager.GetStarredMessagesForGroup(messageContainer, context);
                var messagesList = new List<Message>();

                foreach (var star in starList)
                {
                    var msg = context.Messages.FirstOrDefault(m => m.Id == star.MessageId);
                    messagesList.Add(msg);
                }

                this.IsEmpty = messagesList.Count == 0;

                var sortedMessages = messagesList
                    .AsQueryable()
                    .OrderBy(m => m.CreatedAtUnixTime);

                this.MessagesList.DisplayMessages(sortedMessages, context);
                _ = this.MessagesList.LoadPage(0);
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
            /// Gets a value indicating whether no starred messages are present for this group.
            /// </summary>
            public bool IsEmpty { get; }

            /// <summary>
            /// Gets the group or chat avatar to display in the top bar.
            /// </summary>
            public AvatarControlViewModel TopBarAvatar { get; }
        }
    }
}