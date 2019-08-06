using System.Threading.Tasks;

namespace GroupMeClientApi
{
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
        public virtual async Task<byte[]> DownloadAvatarImageAsync(string url, bool isGroup = true)
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

            return await this.DownloadRawImageAsync(url);
        }

        /// <summary>
        /// Downloads a posted Image from GroupMe.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>An image.</returns>
        public virtual Task<byte[]> DownloadPostImageAsync(string url)
        {
            return this.DownloadRawImageAsync(url);
        }

        /// <summary>
        /// Downloads raw data from GroupMe.
        /// </summary>
        /// <param name="url">The url to download.</param>
        /// <returns>Raw bytes.</returns>
        public virtual Task<byte[]> DownloadByteDataAsync(string url)
        {
            return this.HttpClient.GetByteArrayAsync(url);
        }

        /// <summary>
        /// Downloads raw string data from GroupMe.
        /// </summary>
        /// <param name="url">The url to download.</param>
        /// <returns>Raw bytes.</returns>
        public virtual Task<string> DownloadStringDataAsync(string url)
        {
            return this.HttpClient.GetStringAsync(url);
        }

        /// <summary>
        /// Downloads a image from GroupMe.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>An image.</returns>
        protected virtual Task<byte[]> DownloadRawImageAsync(string url)
        {
            return this.HttpClient.GetByteArrayAsync(url);
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