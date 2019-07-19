using System.Threading.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient.Notifications
{
    public interface INotificationSink
    {
        Task GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container);
        Task ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container);
        Task MessageUpdated(Message message, string alert, IMessageContainer container);
        void HeartbeatReceived();

        void RegisterPushSubscriptions(PushClient pushClient, GroupMeClientApi.GroupMeClient client);
    }
}
