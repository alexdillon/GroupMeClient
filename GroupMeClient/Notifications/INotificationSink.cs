using System.Threading.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient.Notifications
{
    interface INotificationSink
    {
        Task GroupUpdated(LineMessageCreateNotification notification);
        Task ChatUpdated(DirectMessageCreateNotification notification);
        void MessageUpdated(Message message);
        void HeartbeatReceived();

        void RegisterPushSubscriptions(PushClient pushClient);
    }
}
