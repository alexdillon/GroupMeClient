using Newtonsoft.Json;

namespace GroupMeClientApi.Push.Notifications
{
    /// <summary>
    /// Represents a notification indicating that a message has been liked.
    /// </summary>
    public class LikeCreateNotification : Notification
    {
        /// <inheritdoc/>
        public override string Type { get; } = "like.create";

        /// <summary>
        /// Gets the alert text.
        /// </summary>
        [JsonProperty("alert")]
        public string Alert { get; internal set; }

        /// <summary>
        /// Gets the Unix Timestamp when the notification was received.
        /// </summary>
        [JsonProperty("received_at")]
        public string ReceivedAtUnixTimestamp { get; internal set; }
    }
}
