using System.ComponentModel.DataAnnotations;

namespace GroupMeClient.Core.Caching.Models
{
    /// <summary>
    /// <see cref="GroupOrChatState"/> saves the state of a specific <see cref="GroupMeClientApi.Models.Group"/>
    /// or <see cref="GroupMeClientApi.Models.Chat"/>. This is used in the GMDC UI when computing unread message
    /// notifications.
    /// </summary>
    public class GroupOrChatState
    {
        /// <summary>
        /// Gets or sets the identifier for the Group or Chat this configuration data applies to.
        /// </summary>
        [Key]
        public string GroupOrChatId { get; set; }

        /// <summary>
        /// Gets or sets the number of total messages in the Group or Chat
        /// when it was last opened.
        /// </summary>
        public int LastTotalMessageCount { get; set; }
    }
}
