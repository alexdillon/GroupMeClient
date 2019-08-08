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
        /// <param name="navigateToUrl">The URL of the image to open in a web browser when the user clicks on it.</param>
        public ImageLinkAttachmentControlViewModel(string url, ImageDownloader imageDownloader, string navigateToUrl = null)
            : base(imageDownloader)
        {
            this.Url = url;
            this.NavigateToUrl = navigateToUrl;

            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        /// <summary>
        /// Gets the command to be performed when the image is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        private string NavigateToUrl { get; }

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
            var navigateUrl = !string.IsNullOrEmpty(this.NavigateToUrl) ? this.NavigateToUrl : this.Url;
            Extensions.WebBrowserHelper.OpenUrl(navigateUrl);
        }
    }
}
