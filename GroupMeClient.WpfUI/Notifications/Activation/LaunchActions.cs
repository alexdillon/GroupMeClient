using System;

namespace GroupMeClient.WpfUI.Notifications.Activation
{
    /// <summary>
    /// <see cref="LaunchActions"/> define the actions that can be performed upon activation of a Windows 10 Notification.
    /// </summary>
    public enum LaunchActions
    {
        /// <summary>
        /// The <see cref="Group"/> or <see cref="Chat"/> should be opened and displayed.
        /// </summary>
        ShowGroup,

        /// <summary>
        /// The <see cref="Member"/> should be liked.
        /// </summary>
        LikeMessage,

        /// <summary>
        /// A quick reply should be initiated.
        /// </summary>
        InitiateReplyMessage,

        /// <summary>
        /// A quick reply is completed and ready to send.
        /// </summary>
        SendReplyMessage,
    }
}
