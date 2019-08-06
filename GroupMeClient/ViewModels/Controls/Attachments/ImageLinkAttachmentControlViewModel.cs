using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="ImageLinkAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.ImageLinkAttachmentControl"/> control.
    /// </summary>
    public class ImageLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLinkAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="url">The URL of the image to display.</param>
        /// <param name="imageDownloader">The downloader to use when retreiving data.</param>
        public ImageLinkAttachmentControlViewModel(string url, ImageDownloader imageDownloader)
            : base(imageDownloader)
        {
            this.Url = url;

            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        /// <summary>
        /// Gets the command to be performed when the image is clicked.
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
