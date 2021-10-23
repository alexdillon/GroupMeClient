using System;

namespace GroupMeClient.WpfUI.Notifications.Activation
{
    /// <summary>
    /// Arguments that can be included in a toast notification argument string.
    /// </summary>
    public class NotificationArguments
    {
        /// <summary>
        /// The launch action to perform when the notification is opened.
        /// </summary>
        public const string Action = "action";

        /// <summary>
        /// The conversation ID of the group that generated the notification.
        /// </summary>
        public const string ConversationId = "conversationId";

        /// <summary>
        /// The message ID of the message that generated the notification.
        /// </summary>
        public const string MessageId = "messageId";

        /// <summary>
        /// The local URI to the avatar icon for the container that generated the notification.
        /// </summary>
        public const string ContainerAvatar = "containerAvatar";

        /// <summary>
        /// The name of the container that generated the notification.
        /// </summary>
        public const string ContainerName = "containerName";
    }
}
