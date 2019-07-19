using GroupMeClientApi;
using GroupMeClientApi.Models;
using ToastNotifications;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    /// <summary>
    /// Provides extension methods to simplify displaying internal toasts.
    /// </summary>
    internal static class GroupMeToastExtensions
    {
        /// <summary>
        /// Displays a GroupMe Toast Notification.
        /// </summary>
        /// <param name="notifier">The Notfifier to display with.</param>
        /// <param name="message">The notification body.</param>
        /// <param name="avatar">The notification avatar.</param>
        /// <param name="imageDownloader">The downloader used to download the avatar.</param>
        public static void ShowGroupMeToastMessage(this Notifier notifier, string message, IAvatarSource avatar, ImageDownloader imageDownloader)
        {
            notifier.Notify(() => new GroupMeToastNotification(message, avatar, imageDownloader));
        }
    }
}
