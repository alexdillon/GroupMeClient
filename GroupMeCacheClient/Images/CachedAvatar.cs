namespace GroupMeClientCached.Images
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a cached avatar in the database.
    /// </summary>
    internal class CachedAvatar
    {
        /// <summary>
        /// Gets or sets the identifier for this avatar.
        /// </summary>
        [Key]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the raw image data for this avatar.
        /// </summary>
        public byte[] Image { get; set; }
    }
}
