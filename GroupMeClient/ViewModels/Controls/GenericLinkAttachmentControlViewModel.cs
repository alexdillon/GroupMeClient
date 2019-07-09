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

namespace GroupMeClient.ViewModels.Controls
{
    public class GenericLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public GenericLinkAttachmentControlViewModel(string url) :
            base(url)
        {
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

        protected async Task DownloadFaviconImage(string url)
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

        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImage(this.LinkInfo.AnyPreviewPictureUrl);
            _ = this.DownloadFaviconImage(this.LinkInfo.Favicon);
            RaisePropertyChanged("");
        }
    }
}
