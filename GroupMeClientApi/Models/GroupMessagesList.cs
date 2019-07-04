using System.Collections.Generic;
using Newtonsoft.Json;

namespace GroupMeClientApi.Models
{
    /// <summary>
    /// Contains a list of <see cref="Message"/>s returned from a <see cref="Group"/>, along with status information.
    /// </summary>
    public class GroupMessagesList
    {
        /// <summary>
        /// Gets the response data containing the messages.
        /// </summary>
        [JsonProperty("response")]
        public MessageListResponse Response { get; internal set; }

        /// <summary>
        /// Gets the metadata containing additional status information from GroupMe.
        /// </summary>
        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }

        /// <summary>
        /// Contains a list of messages and supporting information for a <see cref="Group"/>.
        /// </summary>
        public class MessageListResponse
        {
            /// <summary>
            /// Gets the number of messages in a <see cref="Group"/>.
            /// </summary>
            [JsonProperty("count")]
            public int Count { get; internal set; }

            /// <summary>
            /// Gets the <see cref="Message"/>s in a <see cref="Group"/>.
            /// </summary>
            [JsonProperty("messages")]
            public IList<Message> Messages { get; internal set; }
        }
    }
}