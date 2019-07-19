using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public class GenericLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public GenericLinkAttachmentControlViewModel(string url)
            : base(url)
        {
            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        private ImageSource faviconImage;

        /// <summary>
        /// Gets or sets the favicon image.
        /// </summary>
        public ImageSource FaviconImage
        {
            get { return this.faviconImage; }
            set { this.Set(() => this.FaviconImage, ref this.faviconImage, value); }
        }

        public string Title => this.LinkInfo?.Title;

        public string Site => this.Uri?.Host;

        public string Handle => this.LinkInfo?.ScreenName;

        public ICommand Clicked { get; }

        protected async Task DownloadFaviconImage(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var httpClient = new HttpClient();
                var result = await httpClient.GetByteArrayAsync(url);

                this.FaviconImage = Extensions.ImageUtils.BytesToImageSource(result);
            }
        }

        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImage(this.LinkInfo.AnyPreviewPictureUrl);
            _ = this.DownloadFaviconImage(this.LinkInfo.Favicon);
            this.RaisePropertyChanged(string.Empty);
        }

        private void ClickedAction()
        {
            Extensions.WebBrowserHelper.OpenUrl(this.Url);
        }
    }
}
