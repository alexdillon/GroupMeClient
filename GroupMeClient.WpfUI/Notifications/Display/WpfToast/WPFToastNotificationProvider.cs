using System.Threading.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientPlugin.Notifications.Display;

namespace GroupMeClient.WpfUI.Notifications.Display.WpfToast
{
    /// <summary>
    /// Provides an adapter for <see cref="PopupNotificationProvider"/> to use Windows 10 Toast Notifications.
    /// </summary>
    public class WpfToastNotificationProvider : IPopupNotificationSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WpfToastNotificationProvider"/> class.
        /// </summary>
        /// <param name="manager">The manager to use for displaying toast notifications.</param>
        public WpfToastNotificationProvider(WpfToast.ToastHolderViewModel manager)
        {
            this.Manager = manager;
        }

        private WpfToast.ToastHolderViewModel Manager { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        /// <inheritdoc />
        Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar, string containerId)
        {
            var toast = new ToastNotificationViewModel(
                body,
                new DummyAvatarSource(avatarUrl, roundedAvatar),
                this.GroupMeClient.ImageDownloader);

            this.Manager.DisplayNewToast(toast);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl, string containerId, string messageId)
        {
            var toast = new ToastNotificationViewModel(
              body,
              new DummyAvatarSource(avatarUrl, roundedAvatar),
              this.GroupMeClient.ImageDownloader);

            this.Manager.DisplayNewToast(toast);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar, string containerId, string messageId)
        {
            var toast = new ToastNotificationViewModel(
              body,
              new DummyAvatarSource(avatarUrl, roundedAvatar),
              this.GroupMeClient.ImageDownloader);

            this.Manager.DisplayNewToast(toast);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
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
