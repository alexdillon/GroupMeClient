using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Settings
{
    /// <summary>
    /// <see cref="ChatsSettings"/> defines the settings and persistant storage needed for Groups and Chats displays.
    /// </summary>
    public class ChatsSettings
    {
        /// <summary>
        /// Gets or sets a listing of <see cref="GroupOrChatState"/> corresponding to each Group
        /// or Chat available.
        /// </summary>
        public List<GroupOrChatState> GroupChatStates { get; set; } = new List<GroupOrChatState>();

        /// <summary>
        /// <see cref="GroupOrChatState"/> saves the state of a specific <see cref="GroupMeClientApi.Models.Group"/>
        /// or <see cref="GroupMeClientApi.Models.Chat"/>.
        /// </summary>
        public class GroupOrChatState
        {
            /// <summary>
            /// Gets or sets the identifier for the Group or Chat this configuration data applies to.
            /// </summary>
            public string GroupOrChatId { get; set; }

            /// <summary>
            /// Gets or sets the identifier for the last read message.
            /// </summary>
            public string LastReadMessageId { get; set; }

            /// <summary>
            /// Gets or sets the number of total messages in the Group or Chat
            /// when it was last opened.
            /// </summary>
            public int LastTotalMessageCount { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the most recent contiguous message that
            /// has been downloaded and index for searching.
            /// </summary>
            public string LastFullyIndexedId { get; set; }
        }
    }
}
