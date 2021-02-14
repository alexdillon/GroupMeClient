using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core.Caching.Models;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Tasks;
using GroupMeClientApi;
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
        /// <param name="settingsManager">The application settings manager.</param>
        public CacheManager(string databasePath, TaskManager taskScheduler, SettingsManager settingsManager)
        {
            this.Path = databasePath;
            this.SuperIndexer = new SuperIndexer(this, taskScheduler, settingsManager);
            this.HasBeenUpgradeChecked = false;
        }

        /// <summary>
        /// Gets the <see cref="SuperIndexer"/> that is operating on the current database.
        /// </summary>
        public SuperIndexer SuperIndexer { get; }

        private string Path { get; }

        private bool HasBeenUpgradeChecked { get; set; }

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
        /// Creates a new instance of the message cache context without performing EF Migrations.
        /// </summary>
        /// <returns>A new <see cref="CacheContext"/>.</returns>
        public CacheContext OpenUnmigratedContext()
        {
            var context = new CacheContext(this.Path, doDatabaseUpgrade: false);
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
            /// Gets or sets index status for each <see cref="Group"/> or <see cref="Chat"/> stored in this cache.
            /// </summary>
            public DbSet<GroupIndexStatus> IndexStatus { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Group"/> metadata stored in the database.
            /// </summary>
            public DbSet<Group> GroupMetadata { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Chat"/> metadata stored in the database.
            /// </summary>
            public DbSet<Chat> ChatMetadata { get; set; }

            private string DatabaseName { get; set; }

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
            /// Returns a <see cref="IQueryable"/> of all the messages in a given <see cref="IMessageContainer"/>
            /// that are cached in the database.
            /// </summary>
            /// <param name="group">The <see cref="IMessageContainer"/> which to return messages for.</param>
            /// <returns>Returns a <see cref="Queryable"/> collection of all the messages in a given <see cref="IMessageContainer"/>.</returns>
            public IQueryable<Message> GetMessagesForGroup(IMessageContainer group)
            {
                if (group is Group g)
                {
                    return this.Messages
                        .AsNoTracking()
                        .Where(m => m.GroupId == g.Id);
                }
                else if (group is Chat c)
                {
                    // Chat.Id returns the Id of the other user
                    // Conversation IDs are user1+user2.
                    return this.Messages
                        .AsNoTracking()
                        .Where(m => m.ConversationId == c.ConversationId);
                }
                else
                {
                    return Enumerable.Empty<Message>().AsQueryable();
                }
            }

            /// <summary>
            /// Returns a <see cref="IList{T}"/> of all the unique <see cref="Group"/>s and <see cref="Chat"/>s
            /// stored in cache.
            /// </summary>
            /// <returns>An <see cref="IQueryable{T}"/> of all the unique <see cref="Group"/>s and <see cref="Chat"/>s.</returns>
            public IList<IMessageContainer> GetGroupsAndChats()
            {
                var groupIds = this.Messages.Select(m => m.GroupId).Distinct().Where(i => i != null);
                var chatIds = this.Messages.Select(m => m.ConversationId).Distinct().Where(i => i != null);

                var results = new List<IMessageContainer>();

                foreach (var id in groupIds)
                {
                    var group = this.GroupMetadata
                        .Include(m => m.Members)
                        .FirstOrDefault(m => m.Id == id);

                    if (group == null)
                    {
                        group = Placeholders.CreatePlaceholderGroup(id, "Deleted Group");
                    }

                    group.AssociateWithClient(SimpleIoc.Default.GetInstance<GroupMeClientApi.GroupMeClient>());
                    results.Add(group);
                }

                foreach (var conversationId in chatIds)
                {
                    var chat = this.ChatMetadata
                        .Include(c => c.OtherUser)
                        .FirstOrDefault(c => c.ConversationId == conversationId);

                    if (chat == null)
                    {
                        chat = Placeholders.CreatePlaceholderChat(
                            id: conversationId, // This is technically wrong, but doesn't really matter
                            otherUserName: "Deleted Chat",
                            conversationId: conversationId);
                    }

                    chat.AssociateWithClient(SimpleIoc.Default.GetInstance<GroupMeClientApi.GroupMeClient>());
                    results.Add(chat);
                }

                return results;
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

                modelBuilder.Entity<Message>().Ignore(m => m.Group);
                modelBuilder.Entity<Message>().Ignore(m => m.Chat);

                this.WorkaroundsForMessageConversion(modelBuilder);
                this.WorkaroundsForGroupConversion(modelBuilder);
                this.WorkaroundsForChatConversion(modelBuilder);
                this.WorkaroundsForMemberConversion(modelBuilder);
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
                    .HasKey(m => m.Id);

                // Index on GroupId to improve lookup speed
                modelBuilder.Entity<Message>()
                    .HasIndex(m => m.GroupId);

                // Index on ConversationId to improve lookup speed
                modelBuilder.Entity<Message>()
                    .HasIndex(m => m.ConversationId);

                // Provide mapping for the Message.ICollection<FavoritedBy>
                modelBuilder.Entity<Message>()
                    .Property(m => m.FavoritedBy)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => new List<string>(v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));

                // Provide JSON serialization for Attachment list
                modelBuilder.Entity<Message>()
                    .Property(m => m.Attachments)
                    .HasConversion(
                        v => JsonConvert.SerializeObject(v),
                        v => JsonConvert.DeserializeObject<List<Attachment>>(v));
            }

            /// <summary>
            /// Provides workarounds to serialize the <see cref="Group"/> object with EntityFramework.
            /// </summary>
            /// <param name="modelBuilder">The EF ModelBuilder Object.</param>
            protected void WorkaroundsForGroupConversion(ModelBuilder modelBuilder)
            {
                // Set group primary key (Id)
                modelBuilder.Entity<Group>().HasKey(x => x.Id);

                modelBuilder.Entity<Group>().Ignore(g => g.LatestMessage);
                modelBuilder.Entity<Group>().Ignore(g => g.Messages);
                modelBuilder.Entity<Group>().Ignore(g => g.MsgPreview);
                modelBuilder.Entity<Group>().Ignore(g => g.ReadReceipt);
            }

            /// <summary>
            /// Provides workarounds to serialize the <see cref="Chat"/> object with EntityFramework.
            /// </summary>
            /// <param name="modelBuilder">The EF ModelBuilder Object.</param>
            protected void WorkaroundsForChatConversion(ModelBuilder modelBuilder)
            {
                // Set group primary key (Id)
                modelBuilder.Entity<Chat>().HasKey(x => x.Id);

                modelBuilder.Entity<Chat>().Ignore(c => c.LatestMessage);
                modelBuilder.Entity<Chat>().Ignore(c => c.Messages);
                modelBuilder.Entity<Chat>().Ignore(c => c.ReadReceipt);
            }

            /// <summary>
            /// Provides workarounds to serialize the <see cref="Member"/> object with EntityFramework.
            /// </summary>
            /// <param name="modelBuilder">The EF ModelBuilder Object.</param>
            protected void WorkaroundsForMemberConversion(ModelBuilder modelBuilder)
            {
                // Set group primary key (Id)
                modelBuilder.Entity<Member>().HasKey(m => m.Id);

                // Provide mapping for the Message.ICollection<FavoritedBy>
                modelBuilder.Entity<Member>()
                    .Property(x => x.Roles)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => new List<string>(v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));
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

                var doesMigrationHistoryExist = this.DoesTableExist(conn, "__EFMigrationsHistory");
                var doesMessagesTableExist = this.DoesTableExist(conn, "Messages");

                if (!doesMessagesTableExist)
                {
                    // If the messages table does not exist, just wipe the whole DB out and start clean
                    // There is no usable data existing, so the best path forward is treating it as a clean
                    // installation and not using any of the migration or compatibility paths

                    // Just delete the file
                    conn.Close();
                    System.IO.File.Delete(this.DatabaseName);
                }
                else if (!doesMigrationHistoryExist && doesMessagesTableExist)
                {
                    // If the DB has already been established with the Messages table, but is not migration compatible,
                    // it is from a pre-GMDC 27 installation.

                    // Databases created prior to GMDC 27 were created with EnsureCreated,
                    // and do not support migrations. The InitialCreate Migration
                    // is schema compatible with the legacy database format, so just mark
                    // it as being compliant with "20200629053659_InitialCreate" under EF "3.1.5"

                    // The messages table exists so mark this as migration ready
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

            private bool DoesTableExist(DbConnection connection, string tableName)
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $@"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
                    var exists = command.ExecuteScalar() != null;

                    return exists;
                }
            }
        }
    }
}
