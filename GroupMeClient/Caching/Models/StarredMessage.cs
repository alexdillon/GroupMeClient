using System;
using System.ComponentModel.DataAnnotations;

namespace GroupMeClient.Caching.Models
{
    /// <summary>
    /// Represents a message that has been starred by the user.
    /// </summary>
    public class StarredMessage
    {
        /// <summary>
        /// Gets or sets the ID of the Message that has been starred.
        /// </summary>
        [Key]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the conversation this starred message belongs to.
        /// </summary>
        public string ConversationId { get; set; }
    }
}
