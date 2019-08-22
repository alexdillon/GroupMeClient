using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="VideoAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.VideoAttachmentControl"/> control.
    /// </summary>
    public class VideoAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="url">The URL of the video to display.</param>
        /// <param name="imageDownloader">The downloader to use when retreiving data.</param>
        public VideoAttachmentControlViewModel(string url, ImageDownloader imageDownloader)
            : base(imageDownloader)
        {
            this.Url = url;

            if (this.Uri != null && this.Uri.Host == "v.groupme.com")
            {
                var newUri = this.Uri.AbsoluteUri;
                newUri = newUri.Replace(".mp4", ".jpg");

                _ = this.DownloadImageAsync(newUri, 600, 300);
            }

            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="attachment">The GroupMe Video Attachment to display.</param>
        /// <param name="imageDownloader">The downloader to use when retreiving data.</param>
        public VideoAttachmentControlViewModel(VideoAttachment attachment, ImageDownloader imageDownloader)
            : base(imageDownloader)
        {
            this.Url = attachment.Url;
            _ = this.DownloadImageAsync(attachment.PreviewUrl, 600, 300);

            this.Clicked = new RelayCommand(this.ClickedAction);
        }

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
        }

        private void ClickedAction()
        {
            Extensions.WebBrowserHelper.OpenUrl(this.Url);
        }
    }
}
