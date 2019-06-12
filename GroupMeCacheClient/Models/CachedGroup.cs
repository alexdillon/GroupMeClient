namespace GroupMeClientCached.Models
{
    using System;
    using System.Collections.Generic;
    using GroupMeClientApi.Models;

    /// <summary>
    /// Provides a cachable copy of the <see cref="Group"/> class.
    /// </summary>
    public class CachedGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedGroup"/> class.
        /// </summary>
        public CachedGroup()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedGroup"/> class.
        /// </summary>
        /// <param name="group">Group object to copy metadata from.</param>
        public CachedGroup(Group group)
        {
            this.Id = group.Id;
            this.Name = group.Name;
            this.PhoneNumber = group.PhoneNumber;
            this.Type = group.Type;
            this.Description = group.Description;
            this.ImageUrl = group.ImageUrl;
            this.CreatedAtUnixTime = group.CreatedAtUnixTime;
            this.UpdatedAtUnixTime = group.UpdatedAtUnixTime;
            this.OfficeMode = group.OfficeMode;
            this.ShareUrl = group.ShareUrl;
            this.Members = group.Members;
            this.MaxMembers = group.MaxMembers;
            this.CachedMessages = new List<Message>();
        }

        /// <summary>
        /// Gets the group identifier.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the group name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the phone number that can be used to interact with this group over SMS.
        /// </summary>
        public string PhoneNumber { get; internal set; }

        /// <summary>
        /// Gets the group type.
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Gets the group description text.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the Url for the Group avatar or image.
        /// </summary>
        public string ImageUrl { get; internal set; }

        /// <summary>
        /// Gets the Unix Timestamp when the group was created.
        /// </summary>
        public int CreatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the time when the group was created.
        /// </summary>
        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets the Unix Timestamp when the group was last updated.
        /// </summary>
        public int UpdatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the time when the group was last updated.
        /// </summary>
        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets a value indicating whether the group is in office mode.
        /// </summary>
        public bool OfficeMode { get; internal set; }

        /// <summary>
        /// Gets a Url to share the group.
        /// </summary>
        public string ShareUrl { get; internal set; }

        /// <summary>
        /// Gets a list of <see cref="Members"/> in the group.
        /// </summary>
        public IList<Member> Members { get; internal set; }

        /// <summary>
        /// Gets the maximum number of members who can be in this group.
        /// </summary>
        public int MaxMembers { get; internal set; }

        /// <summary>
        /// Gets a list of <see cref="Message"/> that have been cached.
        /// </summary>
        public IList<Message> CachedMessages { get; internal set; }
    }
}