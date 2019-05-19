using Newtonsoft.Json;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    /// <summary>
    /// <see cref="ChatsList"/> provides a list of Chats, along with additional status information
    /// </summary>
    public class ChatsList
    {
        /// <summary>
        /// Gets or sets the list of Chats
        /// </summary>
        [JsonProperty("response")]
        public IList<Chat> Chats { get; set; }

        /// <summary>
        /// Gets or sets the Metadata for the API Call
        /// </summary>
        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
