using System;
using System.ComponentModel.DataAnnotations;

namespace GroupMeClient.Core.Caching.Models
{
    /// <summary>
    /// Represents a message that has been hidden by the user.
    /// </summary>
    public class HiddenMessage
    {
        /// <summary>
        /// Gets or sets the ID of the Message that has been hidden.
        /// </summary>
        [Key]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the conversation this hidden message belongs to.
        /// </summary>
        public string ConversationId { get; set; }
    }
}
