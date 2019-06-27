namespace GroupMeClientApi
{
    using System.Drawing;
    using System.Threading.Tasks;

    /// <summary>
    /// Allows for downloading images from GroupMe.
    /// </summary>
    public class ImageDownloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageDownloader"/> class.
        /// </summary>
        public ImageDownloader()
        {
            this.HttpClient = new System.Net.Http.HttpClient();
        }

        /// <summary>
        /// Gets the HttpClient used for downloading images.
        /// </summary>
        protected System.Net.Http.HttpClient HttpClient { get; }

        /// <summary>
        /// Downloads an Avatar Image from GroupMe.
        /// </summary>
        /// <param name="url">The URL of the avatar image.</param>
        /// <param name="isGroup">Indicates if the avatar is for a group (true) or a chat (false).</param>
        /// <returns>An image.</returns>
        public virtual async Task<Image> DownloadAvatarImage(string url, bool isGroup = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (isGroup)
                {
                    return this.GetDefaultGroupAvatar();
                }
                else
                {
                    return this.GetDefaultPersonAvatar();
                }
            }
            else
            {
                url = $"{url}.avatar";
            }

            return await this.DownloadRawImage(url);
        }

        /// <summary>
        /// Downloads a posted Image from GroupMe.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>An image.</returns>
        public virtual async Task<Image> DownloadPostImage(string url)
        {
            return await this.DownloadRawImage(url);
        }

        /// <summary>
        /// Downloads a image from GroupMe.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>An image.</returns>
        protected virtual async Task<Image> DownloadRawImage(string url)
        {
            var bytes = await this.HttpClient.GetByteArrayAsync(url);
            return this.BytesToImage(bytes);
        }

        /// <summary>
        /// Converts a Byte Array of image data to an <see cref="Image"/>.
        /// </summary>
        /// <param name="bytes">The raw image data.</param>
        /// <returns>A image object.</returns>
        protected virtual Image BytesToImage(byte[] bytes)
        {
            var ms = new System.IO.MemoryStream(bytes);
            var image = System.Drawing.Bitmap.FromStream(ms);

            return image;
        }

        /// <summary>
        /// Gets the default avatar for a person.
        /// </summary>
        /// <returns>A image object.</returns>
        protected Image GetDefaultPersonAvatar()
        {
            var bytes = GroupMeClientApi.Properties.Resources.DefaultPersonAvatar;
            return this.BytesToImage(bytes);
        }

        /// <summary>
        /// Gets the default avatar for a group.
        /// </summary>
        /// <returns>A image object.</returns>
        protected Image GetDefaultGroupAvatar()
        {
            var bytes = GroupMeClientApi.Properties.Resources.DefaultGroupAvatar;
            return this.BytesToImage(bytes);
        }
    }
}
