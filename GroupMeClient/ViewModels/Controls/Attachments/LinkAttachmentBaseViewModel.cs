using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Net.Http;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Net;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public abstract class LinkAttachmentBaseViewModel : ViewModelBase
    {
        public LinkAttachmentBaseViewModel()
        {
            // in case the inline-downloader is not needed
        }

        public LinkAttachmentBaseViewModel(string url)
        {
            this.Url = url;

            if (this.Uri != null)
            {
                _ = LoadGroupMeInfo();
            }
        }

        private string url;

        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;

                if (url.Contains(" "))
                {
                    url = url.Substring(0, url.IndexOf(" "));
                }

                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    this.Uri = uri;
                }
            }
        }

        public Uri Uri { get; protected set; }

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
            if (!string.IsNullOrEmpty(url))
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
        }

        protected async Task LoadGroupMeInfo()
        {
            try
            {
                const string GROUPME_INLINE_URL = "https://inline-downloader.groupme.com/info?url=";

                var downloader = new HttpClient();
                var data = await downloader.GetStringAsync($"{GROUPME_INLINE_URL}{WebUtility.UrlEncode(this.Url)}");

                var results = JsonConvert.DeserializeObject<GroupMeInlineDownloaderInfo>(data);
                this.LinkInfo = results;

                this.MetadataDownloadCompleted();
            }
            catch (Exception)
            {
                RaisePropertyChanged("");
            }
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
