using System;
using System.Collections.Generic;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GroupMeClient.Caching
{
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
    }
}
