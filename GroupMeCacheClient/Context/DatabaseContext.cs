namespace GroupMeClientCached.Context
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GroupMeClientApi.Models;
    using GroupMeClientApi.Models.Attachments;
    using GroupMeClientCached.Models;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    /// <summary>
    /// <see cref="DatabaseContext"/> provides an interface to the SQLite Database.
    /// </summary>
    internal class DatabaseContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// </summary>
        public DatabaseContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// </summary>
        /// <param name="databaseName">The name of the database file to open.</param>
        public DatabaseContext(string databaseName)
        {
            this.DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        }

        /// <summary>
        /// Gets or sets the <see cref="Group"/>s stored in the database.
        /// </summary>
        public DbSet<CachedGroup> Groups { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Chat"/>s stored in the database.
        /// </summary>
        public DbSet<CachedChat> Chats { get; set; }

        private string DatabaseName { get; set; } = "cache.db";

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={this.DatabaseName}");
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Provide mapping for the Members.IList<Roles>
            modelBuilder.Entity<Member>()
            .Property(x => x.Roles)
            .HasConversion(
                v => string.Join(",", v),
                v => v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            // Provide mapping for the Message.IList<FavoritedBy>
            modelBuilder.Entity<Message>()
            .Property(x => x.FavoritedBy)
            .HasConversion(
                v => string.Join(",", v),
                v => v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            // Provide JSON serialization for Attachment list
            modelBuilder.Entity<Message>()
            .Property(x => x.Attachments)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<Attachment>>(v));
        }
    }
}