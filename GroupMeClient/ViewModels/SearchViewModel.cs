using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="SearchViewModel"/> provides a ViewModel for the <see cref="Controls.SearchView"/> view.
    /// </summary>
    public class SearchViewModel : ViewModelBase
    {
        private ViewModelBase popupDialog;
        private string searchTerm = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The client to use.</param>
        /// <param name="settingsManager">The settings to use.</param>
        public SearchViewModel(GroupMeCachedClient groupMeClient, Settings.SettingsManager settingsManager)
        /// <param name="cacheContext">The cache database to use.</param>
        public SearchViewModel(GroupMeClientApi.GroupMeClient groupMeClient, Settings.SettingsManager settingsManager, Caching.CacheContext cacheContext)
        {
            this.GroupMeClient = groupMeClient ?? throw new System.ArgumentNullException(nameof(groupMeClient));
            this.SettingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            this.CacheContext = cacheContext ?? throw new ArgumentNullException(nameof(cacheContext));

            this.AllGroupsChats = new ObservableCollection<GroupControlViewModel>();

            this.ClosePopup = new RelayCommand(this.CloseBigPopup);
            this.EasyClosePopup = new RelayCommand(this.CloseBigPopup);

            this.Loaded = new RelayCommand(async () => await this.IndexGroups(), true);
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
            get
            {
                return this.searchTerm;
            }

            set
            {
                this.Set(() => this.SearchTerm, ref this.searchTerm, value);
            }
        }

        private GroupMeCachedClient GroupMeClient { get; }

        private Settings.SettingsManager SettingsManager { get; }

        private async Task IndexGroups()
        {
            try
            {
                var loadingDialog = new LoadingControlViewModel();
                this.PopupDialog = loadingDialog;

                // disable automatic database commits to improve performance
                this.GroupMeClient.DatabaseUpdatingEnabled = false;

                var groups = await this.GroupMeClient.GetGroupsAsync();
                var chats = await this.GroupMeClient.GetChatsAsync();

                var groupsAndChats = Enumerable.Concat<IMessageContainer>(groups, chats);

                this.AllGroupsChats.Clear();

                foreach (var group in groupsAndChats)
                {
                    loadingDialog.Message = $"Indexing {group.Name}";
                    await this.IndexGroup(group);

                    // Add Group/Chat to the list
                    var vm = new GroupControlViewModel(group)
                    {
                        GroupSelected = new RelayCommand<GroupControlViewModel>(this.OpenNewGroupChat, (g) => true),
                    };
                    this.AllGroupsChats.Add(vm);
                }

                this.PopupDialog = null;
            }
            catch (Exception)
            {
            }
            finally
            {
                // ensure automatic commits are always turned back on to prevent
                // unintended behavior elsewhere in the application.
                this.GroupMeClient.DatabaseUpdatingEnabled = true;
            }
        }

        private async Task IndexGroup(IMessageContainer container)
        {
            var groupState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == container.Id);

            var newestMessages = await container.GetMessagesAsync();

            long.TryParse(groupState.LastFullyIndexedId, out var lastIndexId);
            long.TryParse(newestMessages.Last().Id, out var retreiveFrom);

            while (lastIndexId < retreiveFrom)
            {
                // not up-to-date, we need to retreive the delta
                var results = await container.GetMaxMessagesAsync(GroupMeClientApi.MessageRetreiveMode.BeforeId, retreiveFrom.ToString());

                if (results.Count == 0)
                {
                    // we've hit the top.
                    break;
                }

                long.TryParse(results.Last().Id, out var latestRetreivedOldestId);
                retreiveFrom = latestRetreivedOldestId;
            }

            groupState.LastFullyIndexedId = newestMessages.First().Id; // everything is downloaded
            this.SettingsManager.SaveSettings();

            await this.GroupMeClient.ForceUpdateAsync();
        }

        private void OpenNewGroupChat(GroupControlViewModel group)
        {
            //if (this.ActiveGroupsChats.Any(g => g.Id == group.Id))
            //{
            //    // this group or chat is already open, we just need to move it to the front
            //    var openGroup = this.ActiveGroupsChats.First(g => g.Id == group.Id);
            //    var indexOpenGroup = this.ActiveGroupsChats.IndexOf(openGroup);
            //    this.ActiveGroupsChats.Move(indexOpenGroup, 0);
            //}
            //else
            //{
            //    // open a new group or chat
            //    var groupContentsDisplay = new GroupContentsControlViewModel(group.MessageContainer)
            //    {
            //        CloseGroup = new RelayCommand<GroupContentsControlViewModel>(this.CloseChat),
            //    };

            //    this.ActiveGroupsChats.Insert(0, groupContentsDisplay);

            //    _ = this.PushClient.SubscribeAsync(group.MessageContainer);

            //    // mark all messages as read
            //    var groupChatState = this.SettingsManager.ChatsSettings.GroupChatStates.Find(g => g.GroupOrChatId == group.Id);
            //    groupChatState.LastTotalMessageCount = group.MessageContainer.TotalMessageCount;
            //    groupChatState.LastReadMessageId = group.MessageContainer.LatestMessage.Id;

            //    // clear the notification bubble
            //    group.TotalUnreadCount = 0;

            //    this.SettingsManager.SaveSettings();
            //}

            //// limit to three multi-chats at a time
            //while (this.ActiveGroupsChats.Count > 3)
            //{
            //    var removeGroup = this.ActiveGroupsChats.Last();
            //    this.PushClient.Unsubscribe(group.MessageContainer);

            //    this.ActiveGroupsChats.Remove(removeGroup);
            //}
        }

        private void CloseBigPopup()
        {
            if (this.PopupDialog is LoadingControlViewModel)
            {
                // Can't cancel a loading screen.
                return;
            }

            if (this.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.PopupDialog = null;
        }
    }
}