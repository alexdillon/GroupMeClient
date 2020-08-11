using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GroupMeClient.Core.Caching.Models;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GroupMeClient.Core.Caching
{
    /// <summary>
    /// <see cref="CacheManager"/> provides an interface to create <see cref="CacheContext"/> to access the SQLite Database.
    /// </summary>
    public class CacheManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheManager"/> class.
        /// </summary>
        /// <param name="databasePath">The name of the database file to open.</param>
        /// <param name="taskScheduler">The scheduler to use for indexing tasks.</param>
        public CacheManager(string databasePath, TaskManager taskScheduler)
        {
            this.Path = databasePath;
            this.SuperIndexer = new SuperIndexer(this, taskScheduler);
            this.HasBeenUpgradeChecked = false;
        }

        /// <summary>
        /// Gets the <see cref="SuperIndexer"/> that is operating on the current database.
        /// </summary>
        public SuperIndexer SuperIndexer { get; }

        private string Path { get; }

        private bool HasBeenUpgradeChecked { get; set; }

        /// <summary>
        /// Returns a <see cref="Queryable"/> collection of all the messages in a given <see cref="IMessageContainer"/>
        /// that are cached in the database.
        /// </summary>
        /// <param name="group">The <see cref="IMessageContainer"/> which to return messages for.</param>
        /// <param name="cacheContext">The cache instance messages should be retreived from.</param>
        /// <returns>Returns a <see cref="Queryable"/> collection of all the messages in a given <see cref="IMessageContainer"/>.</returns>
        public static IQueryable<Message> GetMessagesForGroup(IMessageContainer group, CacheContext cacheContext)
        {
            if (group is Group g)
            {
                return cacheContext.Messages
                    .AsNoTracking()
                    .Where(m => m.GroupId == g.Id);
            }
            else if (group is Chat c)
            {
                // Chat.Id returns the Id of the other user
                // However, GroupMe messages are natively returned with a Conversation Id instead
                // Conversation IDs are user1+user2.
                var conversationId = c.LatestMessage.ConversationId;

                return cacheContext.Messages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversationId);
            }
            else
            {
                return Enumerable.Empty<Message>().AsQueryable();
            }
        }

        /// <summary>
        /// Returns a <see cref="Queryable"/> collection of all the starred messages in a given <see cref="IMessageContainer"/>
        /// that are cached in the database.
        /// </summary>
        /// <param name="group">The <see cref="IMessageContainer"/> which to return starred messages for.</param>
        /// <param name="cacheContext">The cache instance messages should be retreived from.</param>
        /// <returns>Returns a <see cref="Queryable"/> collection of all the starred messages in a given <see cref="IMessageContainer"/>.</returns>
        public static IQueryable<StarredMessage> GetStarredMessagesForGroup(IMessageContainer group, CacheContext cacheContext)
        {
            if (group is Group g)
            {
                return cacheContext.StarredMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == g.Id);
            }
            else if (group is Chat c)
            {
                // Chat.Id returns the Id of the other user
                // However, GroupMe messages are natively returned with a Conversation Id instead
                // Conversation IDs are user1+user2.
                var conversationId = c.LatestMessage.ConversationId;

                return cacheContext.StarredMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversationId);
            }
            else
            {
                return Enumerable.Empty<StarredMessage>().AsQueryable();
            }
        }

        /// <summary>
        /// Returns a <see cref="Queryable"/> collection of all the hidden messages in a given <see cref="IMessageContainer"/>
        /// that are cached in the database.
        /// </summary>
        /// <param name="group">The <see cref="IMessageContainer"/> which to return hidden messages for.</param>
        /// <param name="cacheContext">The cache instance messages should be retreived from.</param>
        /// <returns>Returns a <see cref="Queryable"/> collection of all the hidden messages in a given <see cref="IMessageContainer"/>.</returns>
        public static IQueryable<HiddenMessage> GetHiddenMessagesForGroup(IMessageContainer group, CacheContext cacheContext)
        {
            if (group is Group g)
            {
                return cacheContext.HiddenMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == g.Id);
            }
            else if (group is Chat c)
            {
                // Chat.Id returns the Id of the other user
                // However, GroupMe messages are natively returned with a Conversation Id instead
                // Conversation IDs are user1+user2.
                var conversationId = c.LatestMessage.ConversationId;

                return cacheContext.HiddenMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversationId);
            }
            else
            {
                return Enumerable.Empty<HiddenMessage>().AsQueryable();
            }
        }

        /// <summary>
        /// Creates a new instance of the message cache context.
        /// </summary>
        /// <returns>A new <see cref="CacheContext"/>.</returns>
        public CacheContext OpenNewContext()
        {
            var context = new CacheContext(this.Path, !this.HasBeenUpgradeChecked);

            if (!this.HasBeenUpgradeChecked)
            {
                this.HasBeenUpgradeChecked = true;
            }

            return context;
        }

        /// <summary>
        /// <see cref="CacheContext"/> provides an interface to the SQLite Database.
        /// </summary>
        public class CacheContext : DbContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class. This is provided
            /// for Entity Framework Migration support.
            /// </summary>
            public CacheContext()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class.
            /// </summary>
            /// <param name="databaseName">The name of the database file to open.</param>
            /// <param name="doDatabaseUpgrade">A value indicating whether database upgrades should be evaluated.</param>
            public CacheContext(string databaseName, bool doDatabaseUpgrade)
            {
                this.DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));

                if (doDatabaseUpgrade)
                {
                    if (System.IO.File.Exists(databaseName))
                    {
                        this.EnsureMigrationsSupported();
                    }

                    this.Database.Migrate();
                }
            }

            /// <summary>
            /// Gets or sets the <see cref="Message"/>s stored in the database.
            /// </summary>
            public DbSet<Message> Messages { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Message"/>s stored in the database that have been starred.
            /// </summary>
            public DbSet<StarredMessage> StarredMessages { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Message"/>s stored in the database that have been hidden.
            /// </summary>
            public DbSet<HiddenMessage> HiddenMessages { get; set; }

            /// <summary>
            /// Gets or sets index status for each <see cref="Group"/> or <see cref="Chat"/> stored in this cache.
            /// </summary>
            public DbSet<GroupIndexStatus> IndexStatus { get; set; }

            private string DatabaseName { get; set; } = "cache.db";

            /// <summary>
            /// Adds a collection of <see cref="Message"/>s to the cache.
            /// </summary>
            /// <param name="messages">The messages to store to the cache.</param>
            public void AddMessages(IEnumerable<Message> messages)
            {
                foreach (var msg in messages)
                {
                    var oldMsg = this.Messages.Find(msg.Id);
                    if (oldMsg == null)
                    {
                        this.Messages.Add(msg);
                    }
                }
            }

            /// <summary>
            /// Adds a <see cref="Message"/> to the star list.
            /// </summary>
            /// <param name="message">The message to star.</param>
            public void StarMessage(Message message)
            {
                var starMessage = new StarredMessage()
                {
                    ConversationId = string.IsNullOrEmpty(message.GroupId) ? message.ConversationId : message.GroupId,
                    MessageId = message.Id,
                };

                this.StarredMessages.Add(starMessage);
            }

            /// <summary>
            /// Removes a <see cref="Message"/> from the star list.
            /// </summary>
            /// <param name="message">The message to star.</param>
            public void DeStarMessage(Message message)
            {
                var star = this.StarredMessages.FirstOrDefault(m => m.MessageId == message.Id);
                if (star != null)
                {
                    this.StarredMessages.Remove(star);
                }
            }

            /// <summary>
            /// Adds a <see cref="Message"/> to the hidden list.
            /// </summary>
            /// <param name="message">The message to star.</param>
            public void HideMessage(Message message)
            {
                var hiddenMessage = new HiddenMessage()
                {
                    ConversationId = string.IsNullOrEmpty(message.GroupId) ? message.ConversationId : message.GroupId,
                    MessageId = message.Id,
                };

                this.HiddenMessages.Add(hiddenMessage);
            }

            /// <summary>
            /// Removes a <see cref="Message"/> from the hidden list.
            /// </summary>
            /// <param name="message">The message to star.</param>
            public void DeHideMessage(Message message)
            {
                var hidden = this.HiddenMessages.FirstOrDefault(m => m.MessageId == message.Id);
                if (hidden != null)
                {
                    this.HiddenMessages.Remove(hidden);
                }
            }

            /// <inheritdoc/>
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite($"Data Source={this.DatabaseName}");
            }

            /// <inheritdoc/>
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Set global ignores
                modelBuilder.Ignore<GroupMeClientApi.GroupMeClient>();
                modelBuilder.Ignore<Group>();
                modelBuilder.Ignore<Chat>();
                modelBuilder.Ignore<Member>();

                this.WorkaroundsForMessageConversion(modelBuilder);
            }

            /// <summary>
            /// Provides workarounds to serialize the <see cref="Message"/> object with EntityFramework.
            /// Convert unserializable parts to JSON and store as BLOBs instead.
            /// </summary>
            /// <param name="modelBuilder">The EF ModelBuilder Object.</param>
            protected void WorkaroundsForMessageConversion(ModelBuilder modelBuilder)
            {
                // Set message primary key (Id)
                modelBuilder.Entity<Message>()
                    .HasKey(x => x.Id);

                // Index on GroupId to improve lookup speed
                modelBuilder.Entity<Message>()
                    .HasIndex(p => p.GroupId);

                // Index on ConversationId to improve lookup speed
                modelBuilder.Entity<Message>()
                    .HasIndex(p => p.ConversationId);

                // Index StarredMessages to improve lookup speed
                modelBuilder.Entity<StarredMessage>()
                    .HasIndex(p => p.ConversationId);

                // Provide mapping for the Message.ICollection<FavoritedBy>
                modelBuilder.Entity<Message>()
                    .Property(x => x.FavoritedBy)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => new List<string>(v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));

                // Provide JSON serialization for Attachment list
                modelBuilder.Entity<Message>()
                    .Property(x => x.Attachments)
                    .HasConversion(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<List<Attachment>>(v));
            }

            /// <summary>
            /// Checks whether the underlying database has been setup for EF Migrations by checking
            /// for the existance of the __EFMigrationsHistory table. If the database is in a legacy format,
            /// it will be upgraded to be migration compatible.
            /// </summary>
            /// <remarks>
            /// Adapted from https://stackoverflow.com/a/53473874.
            /// </remarks>
            private void EnsureMigrationsSupported()
            {
                var conn = this.Database.GetDbConnection();
                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = @"SELECT name FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
                    var exists = command.ExecuteScalar() != null;

                    if (!exists)
                    {
                        // Databases created prior to GMDC 27 were created with EnsureCreated,
                        // and do not support migrations. The InitialCreate Migration
                        // is schema compatible with the legacy database format, so just mark
                        // it as being compliant with "20200629053659_InitialCreate" under EF "3.1.5"
                        using (var createTableCmd = conn.CreateCommand())
                        {
                            createTableCmd.CommandText = "CREATE TABLE \"__EFMigrationsHistory\" ( \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, \"ProductVersion\" TEXT NOT NULL )";
                            createTableCmd.ExecuteNonQuery();
                        }

                        using (var insertHistoryCmd = conn.CreateCommand())
                        {
                            insertHistoryCmd.CommandText = "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (\"20200629053659_InitialCreate\", \"3.1.5\")";
                            insertHistoryCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
