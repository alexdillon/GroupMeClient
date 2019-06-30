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
        public virtual async Task<byte[]> DownloadAvatarImage(string url, bool isGroup = true)
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
        public virtual async Task<byte[]> DownloadPostImage(string url)
        {
            return await this.DownloadRawImage(url);
        }

        /// <summary>
        /// Downloads a image from GroupMe.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>An image.</returns>
        protected virtual async Task<byte[]> DownloadRawImage(string url)
        {
            var bytes = await this.HttpClient.GetByteArrayAsync(url);
            return bytes;
        }

        /// <summary>
        /// Gets the default avatar for a person.
        /// </summary>
        /// <returns>A image object.</returns>
        protected byte[] GetDefaultPersonAvatar()
        {
            var bytes = GroupMeClientApi.Properties.Resources.DefaultPersonAvatar;
            return bytes;
        }

        /// <summary>
        /// Gets the default avatar for a group.
        /// </summary>
        /// <returns>A image object.</returns>
        protected byte[] GetDefaultGroupAvatar()
        {
            var bytes = GroupMeClientApi.Properties.Resources.DefaultGroupAvatar;
            return bytes;
        }
    }
}