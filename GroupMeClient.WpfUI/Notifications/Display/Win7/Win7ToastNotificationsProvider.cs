using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using GroupMeClient.Core.Settings;
using GroupMeClient.WpfUI.Notifications.Activation;
using GroupMeClientPlugin.Notifications.Display;
using Notification.Wpf;
using static GroupMeClient.WpfUI.Notifications.Display.Win10.Win10ToastNotificationsProvider;

namespace GroupMeClient.WpfUI.Notifications.Display.Win7
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
        public Win7ToastNotificationsProvider(SettingsManager settingsManager)
        {
            this.SettingsManager = settingsManager;
            this.NotificationManager = new SingularNotificationManager();
        }

        private SettingsManager SettingsManager { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private INotificationManager NotificationManager { get; }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar, string containerId)
        {
            var action = $"action={LaunchActions.ShowGroup};conversationId={containerId}";
            var notification = new Win7ToastNotificationViewModel(
                title: title,
                message: body,
                imageData: await this.GroupMeClient.ImageDownloader.DownloadAvatarImageAsync(avatarUrl, !roundedAvatar));

            this.ShowToast(notification, action);
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl, string containerId, string messageId)
        {
            var action = $"action={LaunchActions.ShowGroup};conversationId={containerId}";
            var notification = new Win7ToastNotificationViewModel(
               title: title,
               message: body,
               imageData: await this.GroupMeClient.ImageDownloader.DownloadAvatarImageAsync(avatarUrl, !roundedAvatar));

            this.ShowToast(notification, action);
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar, string containerId, string messageId)
        {
            var action = $"action={LaunchActions.ShowGroup};conversationId={containerId}";
            var notification = new Win7ToastNotificationViewModel(
               title: title,
               message: body,
               imageData: await this.GroupMeClient.ImageDownloader.DownloadAvatarImageAsync(avatarUrl, !roundedAvatar));

            this.ShowToast(notification, action);
        }

        /// <inheritdoc/>
        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }

        private void ShowToast(Win7ToastNotificationViewModel toastContent, string activationCommand)
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

            void ActivationAction() => ActivationHandler.HandleActivation(
                arguments: activationCommand,
                userInput: new Dictionary<string, object>());

            this.NotificationManager.Show(toastContent, onClick: ActivationAction);
        }
    }
}
