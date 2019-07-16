using GroupMeClientApi.Models;
using Newtonsoft.Json;

namespace GroupMeClientApi.Push.Notifications
{
    /// <summary>
    /// Represents a notification indicating that the likers for a <see cref="Message"/> have changed.
    /// </summary>
    public class FavoriteUpdate : Notification
    {
        /// <inheritdoc/>
        public override string Type { get; } = "favorite";

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

        /// <summary>
        /// Gets a preview of the favorited message.
        /// </summary>
        [JsonProperty("subject")]
        public NotificationSubject FavoriteSubject { get; internal set; }
    }
}
