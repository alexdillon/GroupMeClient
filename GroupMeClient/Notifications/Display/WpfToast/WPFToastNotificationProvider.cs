using System;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    class WpfToastNotificationProvider : IPopupNotificationSink
    {
        public WpfToastNotificationProvider()
        {
            Notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private Notifier Notifier { get; set; }

        Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar)
        {
            Notifier.ShowInformation(body);

            return Task.CompletedTask;
        }

        Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl)
        {
            Notifier.ShowInformation(body);

            return Task.CompletedTask;
        }

        Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar)
        {
            Notifier.ShowInformation(body);

            return Task.CompletedTask;
        }

        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }
    }
}
