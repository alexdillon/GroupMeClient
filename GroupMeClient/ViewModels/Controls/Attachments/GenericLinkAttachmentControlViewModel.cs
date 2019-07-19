using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="GenericLinkAttachmentControlViewModel"/> provides a ViewModel for controls to display a generic webpage attachment.
    /// </summary>
    public class GenericLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        private ImageSource faviconImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLinkAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="url">The url of the attached website.</param>
        public GenericLinkAttachmentControlViewModel(string url)
            : base(url)
        {
            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        /// <summary>
        /// Gets the website title.
        /// </summary>
        public string Title => this.LinkInfo?.Title;

        /// <summary>
        /// Gets the website short URL name.
        /// </summary>
        public string Site => this.Uri?.Host;

        /// <summary>
        /// Gets the action to occur when the website is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <summary>
        /// Gets the favicon image.
        /// </summary>
        public ImageSource FaviconImage
        {
            get { return this.faviconImage; }
            private set { this.Set(() => this.FaviconImage, ref this.faviconImage, value); }
        }

        /// <inheritdoc/>
        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImageAsync(this.LinkInfo.AnyPreviewPictureUrl);
            _ = this.DownloadFaviconImage(this.LinkInfo.Favicon);
            this.RaisePropertyChanged(string.Empty);
        }

        private async Task DownloadFaviconImage(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var httpClient = new HttpClient();
                var result = await httpClient.GetByteArrayAsync(url);

                this.FaviconImage = Extensions.ImageUtils.BytesToImageSource(result);
            }
        }

        private void ClickedAction()
        {
            Extensions.WebBrowserHelper.OpenUrl(this.Url);
        }
    }
}
