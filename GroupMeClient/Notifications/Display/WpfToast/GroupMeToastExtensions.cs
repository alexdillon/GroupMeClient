using GroupMeClientApi;
using GroupMeClientApi.Models;
using ToastNotifications;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    internal static class GroupMeToastExtensions
    {
        public static void ShowGroupMeToastMessage(this Notifier notifier, string message, IAvatarSource avatar, ImageDownloader imageDownloader)
        {
            notifier.Notify(() => new GroupMeToastNotification(message, avatar, imageDownloader));
        }
    }
}
