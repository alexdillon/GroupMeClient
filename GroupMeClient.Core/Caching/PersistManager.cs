using System;
using System.Collections.Generic;
using System.Linq;
using GroupMeClient.Core.Caching.Models;
using GroupMeClientApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GroupMeClient.Core.Caching
{
    /// <summary>
    /// <see cref="PersistManager"/> provides an interface to create <see cref="PersistContext"/> to access the SQLite Database.
    /// </summary>
    public class PersistManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersistManager"/> class.
        /// </summary>
        /// <param name="databasePath">The name of the database file to open.</param>
        public PersistManager(string databasePath)
        {
            this.Path = databasePath;

            this.SharedContext = new Lazy<PersistContext>(() => new PersistContext(this.Path), isThreadSafe: true);
        }

        /// <summary>
        /// Gets the <see cref="SuperIndexer"/> that is operating on the current database.
        /// </summary>
        public SuperIndexer SuperIndexer { get; }

        private string Path { get; }

        private Lazy<PersistContext> SharedContext { get; }

        /// <summary>
        /// Returns a <see cref="Queryable"/> collection of all the starred messages in a given <see cref="IMessageContainer"/>
        /// that are cached in the database.
        /// </summary>
        /// <param name="group">The <see cref="IMessageContainer"/> which to return starred messages for.</param>
        /// <param name="persistContext">The persistance instance mnessages should be retreived from.</param>
        /// <returns>Returns a <see cref="Queryable"/> collection of all the starred messages in a given <see cref="IMessageContainer"/>.</returns>
        public static IQueryable<StarredMessage> GetStarredMessagesForGroup(IMessageContainer group, PersistContext persistContext)
        {
            if (group is Group g)
            {
                return persistContext.StarredMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == g.Id);
            }
            else if (group is Chat c)
            {
                // Chat.Id returns the Id of the other user
                // However, GroupMe messages are natively returned with a Conversation Id instead
                // Conversation IDs are user1+user2.
                var conversationId = c.LatestMessage.ConversationId;

                return persistContext.StarredMessages
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
        /// <param name="persistContext">The persistance instance mnessages should be retreived from.</param>
        /// <returns>Returns a <see cref="Queryable"/> collection of all the hidden messages in a given <see cref="IMessageContainer"/>.</returns>
        public static IQueryable<HiddenMessage> GetHiddenMessagesForGroup(IMessageContainer group, PersistContext persistContext)
        {
            if (group is Group g)
            {
                return persistContext.HiddenMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == g.Id);
            }
            else if (group is Chat c)
            {
                // Chat.Id returns the Id of the other user
                // However, GroupMe messages are natively returned with a Conversation Id instead
                // Conversation IDs are user1+user2.
                var conversationId = c.LatestMessage.ConversationId;

                return persistContext.HiddenMessages
                    .AsNoTracking()
                    .Where(m => m.ConversationId == conversationId);
            }
            else
            {
                return Enumerable.Empty<HiddenMessage>().AsQueryable();
            }
        }

        /// <summary>
        /// Returns the generic recovery state instance (shared across all GMDC instances).
        /// If a default state has not been saved, a blank one will be created.
        /// </summary>
        /// <param name="persistContext">The persistance instance state data should be retreived from.</param>
        /// <returns>A recovery state.</returns>
        public RecoveryState GetDefaultRecoveryState(PersistContext persistContext)
        {
            const int DefaultInstanceId = 1;
            var state = persistContext.RecoveryStates.Find(DefaultInstanceId);

            if (state == null)
            {
                state = new RecoveryState()
                {
                    WindowId = DefaultInstanceId,
                };
                persistContext.RecoveryStates.Add(state);
            }

            return state;
        }

        /// <summary>
        /// Creates a new instance of the persistant storage context.
        /// </summary>
        /// <returns>A new <see cref="PersistContext"/>.</returns>
        public PersistContext OpenNewContext()
        {
            var context = new PersistContext(this.Path);
            return context;
        }

        /// <summary>
        /// Returns a shared context instance for persistant storage that can be
        /// used for lookups. This context should NOT be disposed.
        /// </summary>
        /// <returns>A shared <see cref="PersistContext"/>.</returns>
        public PersistContext OpenSharedReadOnlyContext()
        {
            return this.SharedContext.Value;
        }

        /// <summary>
        /// <see cref="PersistContext"/> provides an interface to the SQLite Database for persistant application storage.
        /// </summary>
        public class PersistContext : DbContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersistContext"/> class. This is provided
            /// for Entity Framework Migration support.
            /// </summary>
            public PersistContext()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PersistContext"/> class.
            /// </summary>
            /// <param name="databaseName">The name of the database file to open.</param>
            public PersistContext(string databaseName)
            {
                this.DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
                this.Database.Migrate();
            }

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

            /// <summary>
            /// Gets or sets the the Group/Chat persistant storage state.
            /// </summary>
            public DbSet<GroupOrChatState> GroupChatStates { get; set; }

            /// <summary>
            /// Gets or sets the Recovery State for instances of GMDC.
            /// </summary>
            public DbSet<RecoveryState> RecoveryStates { get; set; }

            private string DatabaseName { get; set; }

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
                // Index StarredMessages to improve lookup speed
                modelBuilder.Entity<StarredMessage>()
                    .HasIndex(p => p.ConversationId);

                // Index HiddenMessages to improve lookup speed
                modelBuilder.Entity<HiddenMessage>()
                    .HasIndex(p => p.ConversationId);

                // Provide JSON serialization for Open Window list
                modelBuilder.Entity<RecoveryState>()
                    .Property(x => x.OpenChats)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => new List<string>(v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));
            }
        }
    }
}
