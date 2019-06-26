using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace GroupMeClientCached.Images
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a cached image in the database.
    /// </summary>
    internal class CachedImage
    {
        /// <summary>
        /// Gets or sets the identifier for this image.
        /// </summary>
        [Key]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the raw image data for this image.
        /// </summary>
        public byte[] Image { get; set; }
    }
}
