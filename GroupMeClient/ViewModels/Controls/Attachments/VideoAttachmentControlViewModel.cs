using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

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
        public VideoAttachmentControlViewModel(string url)
        {
            this.Url = url;

            if (this.Uri != null && this.Uri.Host == "v.groupme.com")
            {
                var newUri = this.Uri.AbsoluteUri;
                newUri = newUri.Replace(".mp4", ".jpg");

                _ = this.DownloadImageAsync(newUri);
            }

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
