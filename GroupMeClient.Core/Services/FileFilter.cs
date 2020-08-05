using System.Collections.Generic;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="FileFilter"/> defines a type of file that can be opened or closed with a file browser dialog.
    /// </summary>
    public class FileFilter
    {
        /// <summary>
        /// Gets or sets the displayed name of the filter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a enumeration of extensions that are included in this filter.
        /// </summary>
        public ICollection<string> Extensions { get; set; } = new List<string>();
    }
}
