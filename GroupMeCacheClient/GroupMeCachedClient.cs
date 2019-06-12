namespace GroupMeClientCached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GroupMeClientApi;
    using GroupMeClientApi.Models;
    using GroupMeClientCached.Models;

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

            this.Database.Database.EnsureCreated();
        }

        private Context.DatabaseContext Database { get; set; }

        /// <inheritdoc/>
        public override async Task<IList<Group>> GetGroupsAsync()
        {
            var groups = await base.GetGroupsAsync();

            // cache a copy of groups that aren't already in the DB
            foreach (var group in groups)
            {
                if (!this.Database.Groups.Any(g => g.Id == group.Id))
                {
                    var cachedGroup = new CachedGroup(group);
                    this.Database.Groups.Add(cachedGroup);
                }
            }

            return groups;
        }

        /// <inheritdoc/>
        public override async Task<IList<Chat>> GetChatsAsync()
        {
            var chats = await base.GetChatsAsync();

            // cache a copy of chats that aren't already in the DB
            foreach (var chat in chats)
            {
                if (!this.Database.Chats.Any(c => c.OtherUser == chat.OtherUser))
                {
                    var cachedChat = new CachedChat(chat);
                    this.Database.Chats.Add(cachedChat);
                }
            }

            return chats;
        }

        public async void Test()
        {
            var groups = await this.GetGroupsAsync();
            var chats = await this.GetChatsAsync();

            this.Database.SaveChanges();
        }
    }
}
