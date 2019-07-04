using System;
using System.Collections.Generic;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GroupMeClientCached.Context
{
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
        public DbSet<Group> Groups { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Chat"/>s stored in the database.
        /// </summary>
        public DbSet<Chat> Chats { get; set; }

        /// <summary>
        /// Gets or sets the cached avatar images stored in the database.
        /// </summary>
        public DbSet<Images.CachedAvatar> AvatarImages { get; set; }

        /// <summary>
        /// Gets or sets the cached post images stored in the database.
        /// </summary>
        public DbSet<Images.CachedImage> PostImages { get; set; }

        private string DatabaseName { get; set; } = "cache.db";

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
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
                v => new List<string>(v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)));

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

            //// Always say that cached messages are not updated
            //modelBuilder.Entity<Message>()
            //.Property(x => x.IsUpdated)
            //.HasConversion(
            //    v => false,
            //    v => false);

            // Primary key for Attachment is never used
            modelBuilder.Entity<Attachment>()
                .HasKey(x => x.FakeId);

            this.WorkaroundsForGroupConversion(modelBuilder);
            this.WorkaroundsForChatConversion(modelBuilder);
        }

        /// <summary>
        /// Provides workarounds to serialize the <see cref="Group"/> object with EntityFramework.
        /// Without Primary Keys or IDs provided by GroupMe, some parts cannot be serialized to the Database.
        /// Convert unserializable parts to JSON and store as BLOBs instead.
        /// </summary>
        /// <param name="modelBuilder">The EF ModelBuilder Object.</param>
        protected void WorkaroundsForGroupConversion(ModelBuilder modelBuilder)
        {
            // Provide JSON serialization for MessagePreview
            modelBuilder.Entity<Group>()
            .Property(x => x.MsgPreview)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Group.MessagesPreview>(v));

            // Primary key for Message Preview is never used
            modelBuilder.Entity<Group.MessagesPreview>()
                .HasKey(x => x.LastMessageId);

            // Provide JSON serialization for PreviewContents
            modelBuilder.Entity<Group.MessagesPreview>()
            .Property(x => x.Preview)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Group.MessagesPreview.PreviewContents>(v));

            // Primary key for Message Preview is never used
            modelBuilder.Entity<Group.MessagesPreview.PreviewContents>()
                .HasKey(x => x.Text);
        }

        /// <summary>
        /// Provides workarounds to serialize the <see cref="Chat"/> object with EntityFramework.
        /// The LatestMessage member can conflict with the Messages list since that message
        /// will have the same ID. As a workaround consistent with the Group object
        /// Convert to JSON and store as BLOBs instead.
        /// </summary>
        /// <param name="modelBuilder">The EF ModelBuilder Object.</param>
        protected void WorkaroundsForChatConversion(ModelBuilder modelBuilder)
        {
            // Provide JSON serialization for MessagePreview
            modelBuilder.Entity<Chat>()
            .Property(x => x.LatestMessage)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Message>(v));
        }
    }
 }