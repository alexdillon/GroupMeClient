using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi.Models;
using GroupMeClientCached;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="SearchViewModel"/> provides a ViewModel for the <see cref="Controls.SearchView"/> view.
    /// </summary>
    public class SearchViewModel : ViewModelBase
    {
        private ViewModelBase smallDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchViewModel"/> class.
        /// </summary>
        /// <param name="groupMeClient">The client to use.</param>
        /// <param name="settingsManager">The settings to use.</param>
        public SearchViewModel(GroupMeCachedClient groupMeClient, Settings.SettingsManager settingsManager)
        {
            this.GroupMeClient = groupMeClient ?? throw new System.ArgumentNullException(nameof(groupMeClient));
            this.SettingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));

            this.Loaded = new RelayCommand(async () => await this.IndexGroups(), true);
        }

        /// <summary>
        /// Gets the action that should be executed when the search page loads.
        /// </summary>
        public ICommand Loaded { get; private set; }

        /// <summary>
        /// Gets the Small Dialog that should be displayed as a popup.
        /// Gets null if no dialog should be displayed.
        /// </summary>
        public ViewModelBase SmallDialog
        {
            get { return this.smallDialog; }
            private set { this.Set(() => this.SmallDialog, ref this.smallDialog, value); }
        }

        private GroupMeCachedClient GroupMeClient { get; }

        private Settings.SettingsManager SettingsManager { get; }

        private async Task IndexGroups()
        {
            try
            {
                var loadingDialog = new LoadingControlViewModel();
                this.SmallDialog = loadingDialog;

                // disable automatic database commits to improve performance
                this.GroupMeClient.DatabaseUpdatingEnabled = false;

                var groups = await this.GroupMeClient.GetGroupsAsync();
                var chats = await this.GroupMeClient.GetChatsAsync();

                var groupsAndChats = Enumerable.Concat<IMessageContainer>(groups, chats);

                foreach (var group in groupsAndChats)
                {
                    loadingDialog.Message = $"Indexing {group.Name}";
                    await this.IndexGroup(group);
                }

                this.SmallDialog = null;
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

            // restart the context to "dump" old messages out of memory
            // and reset the DbContext internal state.
            await this.GroupMeClient.ForceUpdateAsync();
        }
    }
}