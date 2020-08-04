using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Core.Caching.Models
{
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
