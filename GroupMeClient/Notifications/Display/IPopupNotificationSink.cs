using System.Threading.Tasks;

namespace GroupMeClient.Notifications.Display
{
    interface IPopupNotificationSink
    {
        Task ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar);
        Task ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl);
        Task ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar);

        void RegisterClient(GroupMeClientApi.GroupMeClient client);
    }
}
