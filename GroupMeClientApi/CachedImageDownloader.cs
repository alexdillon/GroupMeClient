using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClientApi
{
    /// <summary>
    /// Allows for downloading images from GroupMe and storing them in a cache.
    /// </summary>
    public class CachedImageDownloader : ImageDownloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedImageDownloader"/> class.
        /// </summary>
        /// <param name="cacheFolder">The folder path to store cached images in.</param>
        public CachedImageDownloader(string cacheFolder)
        {
            this.CachePath = cacheFolder;

            Directory.CreateDirectory(cacheFolder);
        }

        private string CachePath { get; }

        /// <inheritdoc />
        public override async Task<byte[]> DownloadAvatarImageAsync(string url, bool isGroup = true)
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

            string hashName = this.Hash(url) + ".png";
            string imagePath = Path.Combine(this.CachePath, hashName);

            if (File.Exists(imagePath))
            {
                var result = File.ReadAllBytes(imagePath);
                return result;
            }
            else
            {
                var result = await this.DownloadRawImageAsync(url);
                File.WriteAllBytes(imagePath, result);

                return result;
            }
        }

        private string Hash(string input)
        {
            using (var sha1Managed = new SHA1Managed())
            {
                var hash = sha1Managed.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }
    }
}