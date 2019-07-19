using System.Linq;
using System.Threading.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient.Notifications.Display
{
    class PopupNotificationProvider : INotificationSink
    {
        private PopupNotificationProvider(IPopupNotificationSink sink)
        {
            this.PopupNotificationSink = sink;
        }

        private IPopupNotificationSink PopupNotificationSink { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        public static PopupNotificationProvider CreatePlatformNotificationProvider()
        {
            // TODO: actually test to see if platform is Windows 10
            return new PopupNotificationProvider(new Win10.Win10ToastNotificationsProvider());
        }

        public static PopupNotificationProvider CreateInternalNotificationProvider()
        {
            return new PopupNotificationProvider(new WpfToast.WpfToastNotificationProvider());
        }

        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container)
        {
            if (!string.IsNullOrEmpty(notification.Alert) && !this.DidISendIt(notification.Message))
            {
                var image = notification.Message.Attachments.FirstOrDefault(a => a is ImageAttachment);

                if (image != null)
                {
                    await this.PopupNotificationSink.ShowLikableImageMessage(
                        container.Name,
                        notification.Alert,
                        notification.Message.AvatarUrl,
                        (notification.Message as IAvatarSource).IsRoundedAvatar,
                        (image as ImageAttachment).Url);
                }
                else
                {
                    await this.PopupNotificationSink.ShowLikableMessage(
                       container.Name,
                       notification.Alert,
                       notification.Message.AvatarUrl,
                       (notification.Message as IAvatarSource).IsRoundedAvatar);
                }
            }
        }

        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container)
        {
            if (!string.IsNullOrEmpty(notification.Alert) && !this.DidISendIt(notification.Message))
            {
                var image = notification.Message.Attachments.FirstOrDefault(a => a is ImageAttachment);

                if (image != null)
                {
                    await this.PopupNotificationSink.ShowLikableImageMessage(
                        container.Name,
                        notification.Alert,
                        container.ImageOrAvatarUrl,
                        container.IsRoundedAvatar,
                        (image as ImageAttachment).Url);
                }
                else
                {
                    await this.PopupNotificationSink.ShowLikableMessage(
                        container.Name,
                        notification.Alert,
                        container.ImageOrAvatarUrl,
                        container.IsRoundedAvatar);
                }
            }
        }

        async Task INotificationSink.MessageUpdated(Message message, string alert, IMessageContainer container)
        {
            if (!string.IsNullOrEmpty(alert))
            {
                await this.PopupNotificationSink.ShowNotification(
                    container.Name,
                    alert,
                    container.ImageOrAvatarUrl,
                    container.IsRoundedAvatar);
            }
        }

        void INotificationSink.HeartbeatReceived()
        {
        }

        void INotificationSink.RegisterPushSubscriptions(PushClient pushClient, GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
            this.PopupNotificationSink.RegisterClient(client);
        }

        private bool DidISendIt(Message message)
        {
            var me = this.GroupMeClient.WhoAmI();
            return (message.UserId == me.Id);
        }
    }
}
