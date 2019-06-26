namespace GroupMeClientCached.Images
{
    using System.Drawing;
    using System.Threading.Tasks;
    using GroupMeClientApi;
    using GroupMeClientCached.Context;

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

        /// <inheritdoc/>
        public override async Task<Image> DownloadAvatarImage(string url, bool isGroup = true)
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

            var dbResults = await this.Database.AvatarImages.FindAsync(new object[] { url });

            if (dbResults != null)
            {
                return this.BytesToImage(dbResults.Image);
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
                this.Database.SaveChanges();

                var images = this.BytesToImage(bytes);
                return images;
            }
        }

        /// <inheritdoc/>
        public override async Task<Image> DownloadPostImage(string url)
        {
            var dbResults = await this.Database.PostImages.FindAsync(new object[] { url });

            if (dbResults != null)
            {
                return this.BytesToImage(dbResults.Image);
            }
            else
            {
                var bytes = await this.HttpClient.GetByteArrayAsync(url);

                var cachedImage = new CachedImage()
                {
                    Key = url,
                    Image = bytes,
                };
                this.Database.PostImages.Add(cachedImage);
                this.Database.SaveChanges();

                var images = this.BytesToImage(bytes);
                return images;
            }
        }
    }
}
