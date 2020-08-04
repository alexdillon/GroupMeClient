using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Core.Services;
using GroupMeClientApi;

namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="TwitterAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.TwitterAttachmentControl"/> control.
    /// </summary>
    public class TwitterAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="tweetUrl">The URL of the Tweet to display.</param>
        /// <param name="imageDownloader">The downloader to use when retreiving data.</param>
        public TwitterAttachmentControlViewModel(string tweetUrl, ImageDownloader imageDownloader)
            : base(tweetUrl, imageDownloader)
        {
            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        /// <summary>
        /// Gets the sender's name for the Tweet.
        /// </summary>
        public string Sender => this.LinkInfo?.Name;

        /// <summary>
        /// Gets the contents of the Tweet.
        /// </summary>
        public string Text => this.LinkInfo?.Text;

        /// <summary>
        /// Gets the sender's Twitter Handle.
        /// </summary>
        public string Handle => this.LinkInfo?.ScreenName;

        /// <summary>
        /// Gets the command to be performed when the Tweet is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            // Not needed - no unmanaged resources
        }

        /// <inheritdoc/>
        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImageAsync(this.LinkInfo.ProfileImageUrl, 30, 30);
            this.RaisePropertyChanged(string.Empty);
        }

        private void ClickedAction()
        {
            var osService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IOperatingSystemUIService>();
            osService.OpenWebBrowser(this.Url);
        }
    }
}
