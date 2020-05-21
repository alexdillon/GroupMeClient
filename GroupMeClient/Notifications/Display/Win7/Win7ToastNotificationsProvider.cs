using System;
using System.Threading.Tasks;
using System.Windows;
using Notification.Wpf;
using static GroupMeClient.Notifications.Display.Win10.Win10ToastNotificationsProvider;

namespace GroupMeClient.Notifications.Display.Win7
{
    /// <summary>
    /// Provides an adapter for <see cref="PopupNotificationProvider"/> to use Toast Notifications within the Client Window.
    /// </summary>
    public class Win7ToastNotificationsProvider : IPopupNotificationSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Win7ToastNotificationsProvider"/> class.
        /// </summary>
        /// <param name="settingsManager">The settings instance to use.</param>
        public Win7ToastNotificationsProvider(Settings.SettingsManager settingsManager)
        {
            this.SettingsManager = settingsManager;
            this.NotificationManager = new SingularNotificationManager();
            this.NotificationActivator = new Win10.GroupMeNotificationActivator();
        }

        private Settings.SettingsManager SettingsManager { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private INotificationManager NotificationManager { get; }

        private Win10.GroupMeNotificationActivator NotificationActivator { get; }

        /// <inheritdoc/>
        Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar, string containerId)
        {
            var action = $"action={LaunchActions.ShowGroup}&conversationId={containerId}";
            var notification = new Notification.Wpf.NotificationContent()
            {
                Message = body,
                Title = title,
                Type = NotificationType.Notification,
            };

            this.ShowToast(notification, action);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl, string containerId, string messageId)
        {
            var action = $"action={LaunchActions.ShowGroup}&conversationId={containerId}";
            var notification = new Notification.Wpf.NotificationContent()
            {
                Message = body,
                Title = title,
                Type = NotificationType.Notification,
            };

            this.ShowToast(notification, action);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar, string containerId, string messageId)
        {
            var action = $"action={LaunchActions.ShowGroup}&conversationId={containerId}";
            var notification = new Notification.Wpf.NotificationContent()
            {
                Message = body,
                Title = title,
                Type = NotificationType.Notification,
            };

            this.ShowToast(notification, action);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }

        private void ShowToast(NotificationContent toastContent, string activationCommand)
        {
            bool isActive = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                isActive = Application.Current.MainWindow?.IsActive ?? false;
            });

            if (isActive)
            {
                // don't show Windows Notifications if the window is focused.
                return;
            }

            void ActivationAction() => this.NotificationActivator.Activate(
                appUserModelId: "unused",
                invokedArgs: activationCommand,
                data: null,
                dataCount: 0);

            this.NotificationManager.Show(toastContent, onClick: ActivationAction);
        }
    }
}
