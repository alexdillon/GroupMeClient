using System;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using LinqToTwitter;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public class GenericLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public GenericLinkAttachmentControlViewModel(string url) :
            base(url)
        {
            this.Clicked = new RelayCommand(ClickedAction);
        }

        private ImageSource faviconImage;

        /// <summary>
        /// Gets the favicon image.
        /// </summary>
        public ImageSource FaviconImage
        {
            get { return faviconImage; }
            set { Set(() => this.FaviconImage, ref faviconImage, value); }
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

                using (MemoryStream stream = new MemoryStream(result))
                {
                    this.FaviconImage = BitmapFrame.Create(
                        stream,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad);
                }
            }
        }

        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImage(this.LinkInfo.AnyPreviewPictureUrl);
            _ = this.DownloadFaviconImage(this.LinkInfo.Favicon);
            RaisePropertyChanged("");
        }

        private void ClickedAction()
        {
            Extensions.WebBrowserHelper.OpenUrl(this.Url);
        }
    }
}
