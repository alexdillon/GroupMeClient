using System;
using System.Threading.Tasks;
using System.Windows;
using GroupMeClientApi.Models;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    class WpfToastNotificationProvider : IPopupNotificationSink
    {
        public WpfToastNotificationProvider()
        {
            this.Notifier = new Notifier(cfg =>
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 10,
                    offsetY: 100);

                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                        notificationLifetime: TimeSpan.FromSeconds(3),
                        maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                    cfg.Dispatcher = Application.Current.Dispatcher;
                }));
            });
        }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private Notifier Notifier { get; set; }

        Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar)
        {
            // Don't show like notifications and similar

            //Notifier.ShowGroupMeToastMessage(
            //    body,
            //    new DummyAvatarSource(avatarUrl, roundedAvatar),
            //    this.GroupMeClient.ImageDownloader);

            return Task.CompletedTask;
        }

        Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl)
        {
            this.Notifier.ShowGroupMeToastMessage(
                body,
                new DummyAvatarSource(avatarUrl, roundedAvatar),
                this.GroupMeClient.ImageDownloader);

            return Task.CompletedTask;
        }

        Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar)
        {
            this.Notifier.ShowGroupMeToastMessage(
                body,
                new DummyAvatarSource(avatarUrl, roundedAvatar),
                this.GroupMeClient.ImageDownloader);

            return Task.CompletedTask;
        }

        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }

        private class DummyAvatarSource : IAvatarSource
        {
            public DummyAvatarSource(string imageOrAvatarUrl, bool isRounded)
            {
                this.ImageOrAvatarUrl = imageOrAvatarUrl;
                this.IsRoundedAvatar = isRounded;
            }

            public string ImageOrAvatarUrl { get; set; }

            public bool IsRoundedAvatar { get; set; }
        }
    }
}
