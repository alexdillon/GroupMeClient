using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GroupMeClientApi;
using GroupMeClientApi.Models;

namespace GroupMeClientCached
{
    /// <summary>
    /// <see cref="GroupMeCachedClient"/> provides access to the GroupMe API backed with a local cache database.
    /// </summary>
    public class GroupMeCachedClient : GroupMeClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeCachedClient"/> class.
        /// </summary>
        /// <param name="authToken">The authorization token to be used for GroupMe operations.</param>
        /// <param name="databasePath">The path to the database file.</param>
        public GroupMeCachedClient(string authToken, string databasePath)
            : base(authToken)
        {
            this.Database = new Context.DatabaseContext(databasePath);

            // Keep images on a seperate context to allow async operations for images
            // to occur at the same time as other database operations
            var contextForImages = new Context.DatabaseContext(databasePath);
            this.ImageDownloader = new Images.CachedImageDownloader(contextForImages);

            this.Database.Database.EnsureCreated();
        }

        /// <inheritdoc/>
        public override ImageDownloader ImageDownloader { get; }

        private Context.DatabaseContext Database { get; set; }

        private SemaphoreSlim DatabaseSem { get; } = new SemaphoreSlim(1, 1);

        /// <inheritdoc />
        public override IEnumerable<Group> Groups()
        {
            this.DatabaseSem.Wait();

            try
            {
                foreach (var group in this.Database.Groups)
                {
                    group.Client = this;
                    yield return group;
                }
            }
            finally
            {
                this.DatabaseSem.Release();
            }
        }

        /// <inheritdoc />
        public override IEnumerable<Chat> Chats()
        {
            this.DatabaseSem.Wait();

            try
            {
                foreach (var chat in this.Database.Chats)
                {
                    chat.Client = this;
                    yield return chat;
                }
            }
            finally
            {
                this.DatabaseSem.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task<ICollection<Group>> GetGroupsAsync()
        {
            var groups = await base.GetGroupsAsync();

            foreach (var group in groups)
            {
                Group oldGroup = this.Database.Groups.Find(group.Id);

                if (oldGroup == null)
                {
                   this.Database.Groups.Add(group);
                }
                else
                {
                    DataMerger.MergeGroup(oldGroup, group);
                }
            }

            await this.Update();

            return groups;
        }

        /// <inheritdoc/>
        public override async Task<ICollection<Chat>> GetChatsAsync()
        {
            var chats = await base.GetChatsAsync();

            foreach (var chat in chats)
            {
                var oldChat = this.Database.Chats.Find(chat.Id);

                if (oldChat == null)
                {
                    this.Database.Chats.Add(chat);
                }
                else
                {
                    DataMerger.MergeChat(oldChat, chat);
                }
            }

            await this.Update();

            return chats;
        }

        /// <inheritdoc/>
        public override async Task Update()
        {
            await this.DatabaseSem.WaitAsync();

            try
            {
                await this.Database.SaveChangesAsync();
            }
            finally
            {
                this.DatabaseSem.Release();
            }
        }
    }
}
