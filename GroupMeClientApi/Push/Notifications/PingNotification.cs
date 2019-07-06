using Newtonsoft.Json;

namespace GroupMeClientApi.Push.Notifications
{
    /// <summary>
    /// Represents a placeholder PING type notification.
    /// </summary>
    public class PingNotification : Notification
    {
        /// <inheritdoc/>
        public override string Type { get; } = "ping";
    }
}
