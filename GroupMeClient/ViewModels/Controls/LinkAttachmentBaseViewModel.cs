using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Net.Http;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace GroupMeClient.ViewModels.Controls
{
    public abstract class LinkAttachmentBaseViewModel : ViewModelBase
    {
        public LinkAttachmentBaseViewModel(string url)
        {
            this.Url = url;

            if (this.Url.Contains(" "))
            {
                this.Url = this.Url.Substring(0, this.Url.IndexOf(" "));
            }

            if (Uri.TryCreate(this.Url, UriKind.Absolute, out var uri))
            {
                this.Uri = uri;
                LoadGroupMeInfo().ContinueWith(a => this.MetadataDownloadCompleted());
            }
        }

        public string Url { get; private set; }

        public Uri Uri { get; private set; }

        protected GroupMeInlineDownloaderInfo LinkInfo { get; set; }

        public ICommand ClickAction { get; }

        private ImageSource renderedImage;

        /// <summary>
        /// Gets the rendered image.
        /// </summary>
        public ImageSource RenderedImage
        {
            get { return renderedImage; }
            set { Set(() => this.RenderedImage, ref renderedImage, value); }
        }

        protected async Task DownloadImage(string url)
        {
            var httpClient = new HttpClient();
            var result = await httpClient.GetByteArrayAsync(url);

            using (MemoryStream stream = new MemoryStream(result))
            {
                this.RenderedImage = BitmapFrame.Create(
                    stream,
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);
            }
        }

        protected async Task LoadGroupMeInfo()
        {
            const string GROUPME_INLINE_URL = "https://inline-downloader.groupme.com/info?url=";

            var downloader = new HttpClient();
            var data = await downloader.GetStringAsync($"{GROUPME_INLINE_URL}{this.Url}");

            var results = JsonConvert.DeserializeObject<GroupMeInlineDownloaderInfo>(data);
            this.LinkInfo = results;
        }

        protected abstract void MetadataDownloadCompleted();

        protected class GroupMeInlineDownloaderInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("favicon")]
            public string Favicon { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }

            [JsonProperty("image")]
            public string ImageUrl { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("screen_name")]
            public string ScreenName { get; set; }

            [JsonProperty("profile_image_url")]
            public string ProfileImageUrl { get; set; }

            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; set; }

            public string AnyPreviewPictureUrl
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.ImageUrl))
                    {
                        return this.ImageUrl;
                    }
                    else if (!string.IsNullOrEmpty(this.ThumbnailUrl))
                    {
                        return this.ThumbnailUrl;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }
    }
}
