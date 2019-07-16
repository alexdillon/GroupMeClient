using GroupMeClientApi.Models;
using Newtonsoft.Json;

namespace GroupMeClientApi.Push.Notifications
{
    /// <summary>
    /// Represents the subject of a notification.
    /// </summary>
    public class NotificationSubject
    {
        /// <summary>
        /// Gets a preview of the created message.
        /// </summary>
        [JsonProperty("line")]
        public Message LineMessage { get; internal set; }

        /// <summary>
        /// Gets a preview of the created message.
        /// </summary>
        [JsonProperty("direct_message")]
        public Message DirectMessage { get; internal set; }

        /// <summary>
        /// Gets any message provided as subject by GroupMe.
        /// </summary>
        public Message Message
        {
            get
            {
                if (this.LineMessage != null)
                {
                    return this.LineMessage;
                }
                else
                {
                    return this.DirectMessage;
                }
            }
        }
    }
}
