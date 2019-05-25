namespace LibGroupMe.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// <see cref="ChatsList"/> provides a list of Chats, along with additional status information.
    /// </summary>
    public class ChatsList
    {
        /// <summary>
        /// Gets the list of Chats.
        /// </summary>
        [JsonProperty("response")]
        public IList<Chat> Chats { get; internal set; }

        /// <summary>
        /// Gets the Metadata for the API Call.
        /// </summary>
        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }
    }
}
