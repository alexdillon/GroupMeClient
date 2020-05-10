using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="MultiLikeControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.MultiLikeControl"/> control.
    /// </summary>
    public class MultiLikeControlViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private bool isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLikeControlViewModel"/> class.
        /// </summary>
        /// <param name="groupContentsControlViewModel">The Group to perform MultiLike operations on.</param>
        public MultiLikeControlViewModel(GroupContentsControlViewModel groupContentsControlViewModel)
        {
            this.GroupContentsControlViewModel = groupContentsControlViewModel;

            this.EnableMultiLikeCommand = new RelayCommand(this.EnableMultiLike);
            this.DisableMultiLikeCommand = new RelayCommand(this.DisableMultiLike);
            this.PerformMultiLikeCommand = new RelayCommand(async () => await this.DoMultiLike(), true);
        }

        /// <summary>
        /// Gets or sets the command to be performed to enable MultiLike.
        /// </summary>
        public ICommand EnableMultiLikeCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to be performed to enable MultiLike.
        /// </summary>
        public ICommand DisableMultiLikeCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to be performed to enable MultiLike.
        /// </summary>
        public ICommand PerformMultiLikeCommand { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether MultiLiking mode is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this.isEnabled;
            }

            set
            {
                this.Set(() => this.IsEnabled, ref this.isEnabled, value);
                this.RaisePropertyChanged(nameof(this.IsDisabled));
            }
        }

        /// <summary>
        /// Gets a value indicating whether MultiLiking mode is diabled.
        /// </summary>
        public bool IsDisabled => !this.IsEnabled;

        /// <summary>
        /// Gets a value indicating whether any messages are currently selected for liking.
        /// </summary>
        public bool AnyMessagesSelected
        {
            get
            {
                var range = this.GroupContentsControlViewModel.CurrentlySelectedMessages;
                if (range == null)
                {
                    return false;
                }

                try
                {
                    var itemList = (range as ObservableCollection<object>).Cast<MessageControlViewModelBase>().ToList();

                    var oldestId = itemList.Min(m => long.Parse(m.Id));
                    var newestId = itemList.Max(m => long.Parse(m.Id));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private GroupContentsControlViewModel GroupContentsControlViewModel { get; }

        private TimeSpan LikeDelay { get; } = TimeSpan.FromMilliseconds(20);

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }

        private void EnableMultiLike()
        {
            this.IsEnabled = true;
            this.GroupContentsControlViewModel.IsSelectionAllowed = true;

            this.GroupContentsControlViewModel.SmallDialogManager.PopupDialog = null;
        }

        private void DisableMultiLike()
        {
            this.IsEnabled = false;
            this.GroupContentsControlViewModel.IsSelectionAllowed = false;

            var itemList = this.GroupContentsControlViewModel.CurrentlySelectedMessages as ObservableCollection<object>;
            itemList?.Clear();

            this.GroupContentsControlViewModel.SmallDialogManager.PopupDialog = null;
        }

        private async Task DoMultiLike()
        {
            var range = this.GroupContentsControlViewModel.CurrentlySelectedMessages;
            if (range == null)
            {
                return;
            }

            var itemList = (range as ObservableCollection<object>).Cast<MessageControlViewModelBase>().ToList();

            var oldestId = itemList.Min(m => long.Parse(m.Id));
            var newestId = itemList.Max(m => long.Parse(m.Id));

            var loadingControl = new LoadingControlViewModel();
            this.GroupContentsControlViewModel.SmallDialogManager.PopupDialog = loadingControl;

            foreach (var message in this.GroupContentsControlViewModel.Messages)
            {
                var id = long.Parse(message.Id);
                if (id >= oldestId && id <= newestId && message is MessageControlViewModel mcvm)
                {
                    loadingControl.Message = $"Liking Message {mcvm.Message.Text}";
                    await mcvm.LikeMessageAsync();
                    await Task.Delay(this.LikeDelay);
                }
            }

            this.DisableMultiLike();

            this.GroupContentsControlViewModel.SmallDialogManager.PopupDialog = null;
        }

        /// <summary>
        /// <see cref="MultiLikePseudoPlugin"/> defines a plugin-style object that can be used to integrate
        /// Multi-Like functionality into a <see cref="GroupContentsControlViewModel"/>.
        /// </summary>
        public class MultiLikePseudoPlugin : GroupMeClientPlugin.PluginBase, IGroupChatPlugin
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MultiLikePseudoPlugin"/> class.
            /// </summary>
            /// <param name="groupContentsControlViewModel">The Group to perform MultiLike operations on.</param>
            public MultiLikePseudoPlugin(GroupContentsControlViewModel groupContentsControlViewModel)
            {
                this.GroupContentsControlViewModel = groupContentsControlViewModel;
                this.MultiLikeControlViewModel = new MultiLikeControlViewModel(this.GroupContentsControlViewModel);
            }

            /// <inheritdoc/>
            public override string PluginDisplayName => "Multi-Like";

            /// <inheritdoc/>
            public override string PluginVersion => ThisAssembly.SimpleVersion;

            /// <inheritdoc/>
            public override Version ApiVersion => new Version(2, 0, 0);

            /// <inheritdoc/>
            public string PluginName => this.PluginDisplayName;

            private GroupContentsControlViewModel GroupContentsControlViewModel { get; }

            private MultiLikeControlViewModel MultiLikeControlViewModel { get; }

            /// <inheritdoc/>
            public Task Activated(IMessageContainer groupOrChat, CacheSession cacheSession, IPluginUIIntegration integration, Action<CacheSession> cleanup)
            {
                this.GroupContentsControlViewModel.SmallDialogManager.PopupDialog = this.MultiLikeControlViewModel;
                return Task.CompletedTask;
            }
        }
    }
}
