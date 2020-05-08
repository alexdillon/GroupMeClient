using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GroupMeClient.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GroupMeClient.Caching
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
        }

        /// <summary>
        /// Gets the <see cref="SuperIndexer"/> that is operating on the current database.
        /// </summary>
        public SuperIndexer SuperIndexer { get; }

        private string Path { get; }

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
                var conversatonId = c.LatestMessage.ConversationId;

                return cacheContext.Messages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversatonId);
            }
            else
            {
                return Enumerable.Empty<Message>().AsQueryable();
            }
        }

        /// <summary>
        /// Creates a new instance of the message cache context.
        /// </summary>
        /// <returns>A new <see cref="CacheContext"/>.</returns>
        public CacheContext OpenNewContext()
        {
            var context = new CacheContext(this.Path);
            return context;
        }

        /// <summary>
        /// <see cref="CacheContext"/> provides an interface to the SQLite Database.
        /// </summary>
        public class CacheContext : DbContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CacheContext"/> class.
            /// </summary>
            /// <param name="databaseName">The name of the database file to open.</param>
            public CacheContext(string databaseName)
            {
                this.DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
                this.Database.EnsureCreated();
            }

            /// <summary>
            /// Gets or sets the <see cref="Message"/>s stored in the database.
            /// </summary>
            public DbSet<Message> Messages { get; set; }

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
            /// Represents the current index status for a specific <see cref="Group"/> or <see cref="Chat"/>.
            /// </summary>
            public class GroupIndexStatus
            {
                /// <summary>
                /// Gets or sets the <see cref="Group"/> or <see cref="Chat"/> identifier.
                /// </summary>
                [Key]
                public string Id { get; set; }

                /// <summary>
                /// Gets or sets the identifer for the last message that has been continuously indexed.
                /// All messages prior to this ID are guaranteed to be stored in the cache database.
                /// </summary>
                public string LastIndexedId { get; set; }
            }
        }
    }
}
