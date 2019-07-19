using System.Threading.Tasks;

namespace GroupMeClient.Notifications.Display
{
    /// <summary>
    /// <see cref="IPopupNotificationSink"/> defines an interface for a visual notification provider.
    /// </summary>
    internal interface IPopupNotificationSink
    {
        /// <summary>
        /// Show a plain notification.
        /// </summary>
        /// <param name="title">The notification title.</param>
        /// <param name="body">The notification body text.</param>
        /// <param name="avatarUrl">The URL of the avatar image to include in the notification.</param>
        /// <param name="roundedAvatar">A value indicating whether the avatar image should be rounded.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar);

        /// <summary>
        /// Show a notification and optionally include a "Like" button.
        /// </summary>
        /// <param name="title">The notification title.</param>
        /// <param name="body">The notification body text.</param>
        /// <param name="avatarUrl">The URL of the avatar image to include in the notification.</param>
        /// <param name="roundedAvatar">A value indicating whether the avatar image should be rounded.</param>
        /// <param name="imageUrl">The URL of the large image to include in the notification.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl);

        /// <summary>
        /// Show a notification with image, and optionally include a "Like" button.
        /// </summary>
        /// <param name="title">The notification title.</param>
        /// <param name="body">The notification body text.</param>
        /// <param name="avatarUrl">The URL of the avatar image to include in the notification.</param>
        /// <param name="roundedAvatar">A value indicating whether the avatar image should be rounded.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar);

        /// <summary>
        /// Executed when an observer is registered with <see cref="NotificationRouter"/>.
        /// Observers gain access the the <see cref="GroupMeClientApi.GroupMeClient"/> for API operations.
        /// </summary>
        /// <param name="client">The used API client.</param>
        void RegisterClient(GroupMeClientApi.GroupMeClient client);
    }
}
