﻿namespace GroupMeClientCached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GroupMeClientApi;
    using GroupMeClientApi.Models;
    using Microsoft.EntityFrameworkCore;

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

        /// <summary>
        /// Gets a enumeration of <see cref="Group"/>s controlled by the cache system.
        /// </summary>
        public IEnumerable<Group> Groups => this.Database.Groups;

        /// <summary>
        /// Gets a enumeration of <see cref="Chat"/>s controlled by the cache system.
        /// </summary>
        public IEnumerable<Chat> Chats => this.Database.Chats;

        private Context.DatabaseContext Database { get; set; }

        /// <inheritdoc/>
        public override async Task<IList<Group>> GetGroupsAsync()
        {
            var groups = await base.GetGroupsAsync();

            foreach (var group in groups)
            {
               // AddOrUpdate(group);
                if (!this.Database.Groups.Any(g => g.Id == group.Id))
                {
                   this.Database.Groups.Add(group);
                }
                else
                {
                    // Replace EF version with new data from GroupMe and begin tracking it
                    var oldGroup = this.Database.Groups.Find(group.Id);
                    this.Database.Entry(oldGroup).CurrentValues.SetValues(group);
                }
            }

            this.Database.SaveChanges();

            return groups;
        }

        /// <inheritdoc/>
        public override async Task<IList<Chat>> GetChatsAsync()
        {
            var chats = await base.GetChatsAsync();

            foreach (var chat in chats)
            {
                if (!this.Database.Chats.Any(c => c.Id == chat.Id))
                {
                    // Attach new GroupMe response to EF Context
                    this.Database.Chats.Add(chat);
                }
                else
                {
                    // Replace EF version with new data from GroupMe and begin tracking it
                    var oldChat = this.Database.Chats.Find(chat.Id);
                    this.Database.Entry(oldChat).CurrentValues.SetValues(chat);
                }
            }

            this.Database.SaveChanges();

            return chats;
        }

        /// <summary>
        /// Saves the state of all known GroupMe objects to the cache database.
        /// </summary>
        public async void SaveAll()
        {
            await this.Database.SaveChangesAsync();
        }
    }
}