namespace GroupMeClientCached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GroupMeClientApi;
    using GroupMeClientApi.Models;

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

        public async void Test()
        {
            var groups = await this.GetGroupsAsync();
            var chats = await this.GetChatsAsync();

            this.Database.Groups.AddRange(groups);
            this.Database.Chats.AddRange(chats);

            this.Database.SaveChanges();
        }
    }
}
