using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GroupMeClient.Core.Caching.Models
{
    /// <summary>
    /// <see cref="RecoveryState"/> defines parameters about a GMDC instance that can be persisted across application reboots.
    /// </summary>
    public class RecoveryState
    {
        /// <summary>
        /// Gets or sets the identitier of the window this restore state is associated with.
        /// </summary>
        [Key]
        public int WindowId { get; set; }

        /// <summary>
        /// Gets or sets a listing of the IDs of the Groups and Chats that were
        /// lasted opened in the GMDC Client.
        /// </summary>
        public List<string> OpenChats { get; set; } = new List<string>();
    }
}
