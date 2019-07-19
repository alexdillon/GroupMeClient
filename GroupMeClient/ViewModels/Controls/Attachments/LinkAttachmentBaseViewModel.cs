using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="LinkAttachmentBaseViewModel"/> provides a base for controls that display Web Content.
    /// Access to GroupMe's Inline Downloader Service is provided.
    /// </summary>
    public abstract class LinkAttachmentBaseViewModel : ViewModelBase
    {
        private string url;
        private ImageSource renderedImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkAttachmentBaseViewModel"/> class.
        /// </summary>
        public LinkAttachmentBaseViewModel()
        {
            // in case the inline-downloader is not needed
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkAttachmentBaseViewModel"/> class
        /// and begins retreiving data from GroupMe's Inline Downloader Service.
        /// </summary>
        /// <param name="url">The website to resolve against GroupMe's downloader.</param>
        public LinkAttachmentBaseViewModel(string url)
        {
            this.Url = url;

            if (this.Uri != null)
            {
                _ = this.LoadGroupMeInfoAsync();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Uri"/> this ViewModel is displaying.
        /// If the URL used to initialize this class is invalid, null will be returned.
        /// </summary>
        public Uri Uri { get; protected set; }

        /// <summary>
        /// Gets the action that will occur when the control is clicked.
        /// </summary>
        public ICommand ClickAction { get; }

        /// <summary>
        /// Gets or sets the raw Url this control is displaying.
        /// </summary>
        public string Url
        {
            get
            {
                return this.url;
            }

            set
            {
                this.url = value;

                if (this.url.Contains(" "))
                {
                    this.url = this.url.Substring(0, this.url.IndexOf(" "));
                }

                if (Uri.TryCreate(this.url, UriKind.Absolute, out var uri))
                {
                    this.Uri = uri;
                }
            }
        }

        /// <summary>
        /// Gets the rendered image.
        /// </summary>
        public ImageSource RenderedImage
        {
            get { return this.renderedImage; }
            private set { this.Set(() => this.RenderedImage, ref this.renderedImage, value); }
        }

        /// <summary>
        /// Gets the information downloaded from GroupMe's Downloader Service.
        /// </summary>
        protected GroupMeInlineDownloaderInfo LinkInfo { get; private set; }

        /// <summary>
        /// Downloads an image and stores it in <see cref="RenderedImage"/>.
        /// </summary>
        /// <param name="url">The image to download.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task DownloadImageAsync(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var httpClient = new HttpClient();
                var result = await httpClient.GetByteArrayAsync(url);

                this.RenderedImage = Extensions.ImageUtils.BytesToImageSource(result);
            }
        }

        /// <summary>
        /// Downloads metadata from GroupMe and stores it in <see cref="LinkInfo"/>.
        /// </summary>
        /// <param name="url">The website to download metadata for.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task LoadGroupMeInfoAsync()
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
                this.RaisePropertyChanged(string.Empty);
            }
        }

        /// <summary>
        /// Callback when the Metadata download is completed.
        /// Results are available in <see cref="LinkInfo"/>.
        /// </summary>
        protected abstract void MetadataDownloadCompleted();

        /// <summary>
        /// <see cref="GroupMeInlineDownloaderInfo"/> can be deserialized from GroupMe's Inline Downloader JSON response.
        /// </summary>
        protected class GroupMeInlineDownloaderInfo
        {
            /// <summary>
            /// Gets the website title.
            /// </summary>
            [JsonProperty("title")]
            public string Title { get; internal set; }

            /// <summary>
            /// Gets the favicon for the website.
            /// </summary>
            [JsonProperty("favicon")]
            public string Favicon { get; internal set; }

            /// <summary>
            /// Gets a brief description of the website.
            /// </summary>
            [JsonProperty("summary")]
            public string Summary { get; internal set; }

            /// <summary>
            /// Gets the Url of the website preview image.
            /// </summary>
            [JsonProperty("image")]
            public string ImageUrl { get; internal set; }

            /// <summary>
            /// Gets a brief description of the website's contents.
            /// </summary>
            [JsonProperty("text")]
            public string Text { get; internal set; }

            /// <summary>
            /// Gets the name of the website.
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; internal set; }

            /// <summary>
            /// Gets the social media handle for social media posts.
            /// </summary>
            [JsonProperty("screen_name")]
            public string ScreenName { get; internal set; }

            /// <summary>
            /// Gets the social media avatar for social media posts.
            /// </summary>
            [JsonProperty("profile_image_url")]
            public string ProfileImageUrl { get; internal set; }

            /// <summary>
            /// Gets the Url of the website preview image.
            /// </summary>
            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl { get; internal set; }

            /// <summary>
            /// Gets any available preview image for a website.
            /// This property should be available for any website type.
            /// </summary>
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
