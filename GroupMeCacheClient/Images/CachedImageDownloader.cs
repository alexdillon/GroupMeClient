using System.Threading;
using System.Threading.Tasks;
using GroupMeClientApi;
using GroupMeClientCached.Context;

namespace GroupMeClientCached.Images
{
    /// <summary>
    /// Allows for downloading images from GroupMe using a cache database.
    /// </summary>
    public class CachedImageDownloader : ImageDownloader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedImageDownloader"/> class.
        /// </summary>
        /// <param name="db">The database context to use.</param>
        internal CachedImageDownloader(DatabaseContext db)
        {
            this.Database = db;
        }

        private DatabaseContext Database { get; }

        private SemaphoreSlim DatabaseSem { get; } = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public override async Task<byte[]> DownloadAvatarImage(string url, bool isGroup = true)
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

            await this.DatabaseSem.WaitAsync();
            try
            {
                var dbResults = await this.Database.AvatarImages.FindAsync(new object[] { url });

                if (dbResults != null)
                {
                    return dbResults.Image;
                }
                else
                {
                    var bytes = await this.HttpClient.GetByteArrayAsync(url);

                    var cachedAvatar = new CachedAvatar()
                    {
                        Key = url,
                        Image = bytes,
                    };

                    this.Database.AvatarImages.Add(cachedAvatar);

                    return bytes;
                }
            }
            finally
            {
                await this.Database.SaveChangesAsync();
                this.DatabaseSem.Release();
            }
        }
    }
}
