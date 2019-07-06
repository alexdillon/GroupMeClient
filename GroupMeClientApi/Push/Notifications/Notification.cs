using JsonSubTypes;
using Newtonsoft.Json;

namespace GroupMeClientApi.Push.Notifications
{
    /// <summary>
    /// Generic type to represent a push notification from GroupMe.
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(PingNotification), "ping")]
    [JsonSubtypes.KnownSubType(typeof(DirectMessageCreateNotification), "direct_message.create")]
    [JsonSubtypes.KnownSubType(typeof(LineMessageCreateNotification), "line.create")]
    [JsonSubtypes.KnownSubType(typeof(LikeCreateNotification), "like.create")]
    public class Notification
    {
        /// <summary>
        /// Gets the attachment type.
        /// </summary>
        [JsonProperty("type")]
        public virtual string Type { get; }
    }
}
