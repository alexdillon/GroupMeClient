using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.ViewModels;
using GroupMeClient.WpfUI.Notifications.Activation;
using GroupMeClient.WpfUI.Utilities;
using GroupMeClientApi.Models;
using GroupMeClientPlugin.Notifications.Display;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GroupMeClient.WpfUI.Notifications.Display.Win10
{
    /// <summary>
    /// Provides an adapter for <see cref="PopupNotificationProvider"/> to use Toast Notifications within the Client Window.
    /// </summary>
    public class Win10ToastNotificationsProvider : IPopupNotificationSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Win10ToastNotificationsProvider"/> class.
        /// </summary>
        /// <param name="settingsManager">The settings instance to use.</param>
        public Win10ToastNotificationsProvider(SettingsManager settingsManager)
        {
            this.ToastNotifier = ToastNotificationManagerCompat.CreateToastNotifier();
            ToastNotificationManagerCompat.OnActivated += this.ToastNotificationManagerCompat_OnActivated;

            this.SettingsManager = settingsManager;
            this.ChatsViewModel = Ioc.Default.GetService<ChatsViewModel>();
        }

        private ToastNotifierCompat ToastNotifier { get; }

        private bool HasPerformedCleanup { get; set; } = false;

        private SettingsManager SettingsManager { get; }

        private ChatsViewModel ChatsViewModel { get; }

        private string ToastImagePath => Path.GetTempPath() + "WindowsNotifications.GroupMeToasts.Images";

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar, string containerId)
        {
            if (this.ShouldShowToast(containerId))
            {
                this.GotNewNotification(containerId);

                var toastBuilder = new ToastContentBuilder();

                this.AddGrouping(toastBuilder, title, containerId);

                toastBuilder
                    .AddArgument(NotificationArguments.ConversationId, containerId)
                    .AddText(body, AdaptiveTextStyle.Body)
                    .AddAppLogoOverride(
                        uri: new Uri(await this.DownloadImageToDiskCached(
                                    image: avatarUrl,
                                    isAvatar: true,
                                    isRounded: roundedAvatar)),
                        hintCrop: roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default);

                toastBuilder.Show(toast =>
                    {
                        toast.Tag = Guid.NewGuid().ToString().Substring(0, 15);
                        toast.Group = containerId;
                    });
            }
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl, string containerId, string messageId)
        {
            if (this.ShouldShowToast(containerId))
            {
                this.GotNewNotification(containerId);

                var avatar = await this.DownloadImageToDiskCached(
                                  image: avatarUrl,
                                  isAvatar: true,
                                  isRounded: roundedAvatar);

                var toastBuilder = new ToastContentBuilder();

                this.AddGrouping(toastBuilder, title, containerId);

                toastBuilder
                    .AddArgument(NotificationArguments.ConversationId, containerId)
                    .AddArgument(NotificationArguments.MessageId, messageId)
                    .AddText(body, AdaptiveTextStyle.Body)
                    .AddInlineImage(new Uri(await this.DownloadImageToDiskCached(imageUrl)))
                    .AddAppLogoOverride(
                        uri: new Uri(avatar),
                        hintCrop: roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default);

                this.AddInteractiveElements(toastBuilder, containerId, avatar);

                toastBuilder
                    .Show(toast =>
                    {
                        toast.Tag = $"{containerId}{messageId}";
                        toast.Group = containerId;
                    });
            }
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar, string containerId, string messageId)
        {
            if (this.ShouldShowToast(containerId))
            {
                this.GotNewNotification(containerId);

                var avatar = await this.DownloadImageToDiskCached(
                                  image: avatarUrl,
                                  isAvatar: true,
                                  isRounded: roundedAvatar);

                var toastBuilder = new ToastContentBuilder();

                this.AddGrouping(toastBuilder, title, containerId);

                toastBuilder
                    .AddArgument(NotificationArguments.ConversationId, containerId)
                    .AddArgument(NotificationArguments.MessageId, messageId)
                    .AddText(body, AdaptiveTextStyle.Body)
                    .AddAppLogoOverride(
                        uri: new Uri(avatar),
                        hintCrop: roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default);

                this.AddInteractiveElements(toastBuilder, containerId, avatar);

                toastBuilder
                    .Show(toast =>
                    {
                        toast.Tag = $"{containerId}{messageId}";
                        toast.Group = containerId;
                    });
            }
        }

        /// <inheritdoc/>
        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            ActivationHandler.HandleActivation(e.Argument, e.UserInput);
        }

        /// <summary>
        /// Determines whether notifications should be shown for a given chat.
        /// If the main GMDC window is active or the given chat is open as a MiniChat, UWP notifications
        /// will be skipped.
        /// </summary>
        /// <param name="containerId">The ID of the group or chat generating the notification.</param>
        /// <returns>A value indicating whether the notification should be shown.</returns>
        private bool ShouldShowToast(string containerId)
        {
            bool isActive = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                isActive = Application.Current.MainWindow?.IsActive ?? false;

                foreach (var miniChat in this.ChatsViewModel.ActiveMiniChats)
                {
                    if (miniChat.Id == containerId)
                    {
                        isActive = true;
                    }
                }
            });

            return !isActive;
        }

        private void AddInteractiveElements(ToastContentBuilder toastBuilder, string containerId, string avatarUri)
        {
            if (this.SettingsManager.UISettings.EnableNotificationInteractions)
            {
                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());
                var source = groupsAndChats.FirstOrDefault(g => g.Id == containerId);

                toastBuilder
                    .AddArgument(NotificationArguments.ContainerName, source.Name)
                    .AddArgument(NotificationArguments.ContainerAvatar, avatarUri)
                    .AddButton(new ToastButton()
                        .SetContent("Like")
                        .AddArgument(NotificationArguments.Action, LaunchActions.LikeMessage)
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Reply")
                        .AddArgument(NotificationArguments.Action, LaunchActions.InitiateReplyMessage)
                        .SetBackgroundActivation()
                        .SetAfterActivationBehavior(ToastAfterActivationBehavior.PendingUpdate));
            }
        }

        private void AddGrouping(ToastContentBuilder toastBuilder, string title, string containerId)
        {
            if (this.SettingsManager.UISettings.EnableNotificationGrouping)
            {
                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());
                var source = groupsAndChats.FirstOrDefault(g => g.Id == containerId);

                toastBuilder.AddHeader(
                    id: containerId,
                    title: source.Name,
                    arguments:
                        new ToastArguments()
                            .Add(NotificationArguments.ConversationId, containerId));
            }
            else
            {
                toastBuilder.AddText(title, AdaptiveTextStyle.Title);
            }
        }

        private void GotNewNotification(string containerId)
        {
            if (this.SettingsManager.UISettings.EnableUWPNotificationQuickExpiration)
            {
                ToastNotificationManagerCompat.History.RemoveGroup(containerId);
            }
        }

        private async Task<string> DownloadImageToDiskCached(string image, bool isAvatar = false, bool isRounded = false)
        {
            // Toasts can live for up to 3 days, so we cache images for up to 3 days.
            // Note that this is a very simple cache that doesn't account for space usage, so
            // this could easily consume a lot of space within the span of 3 days.
            if (image == null)
            {
                image = string.Empty;
            }

            try
            {
                var directory = Directory.CreateDirectory(this.ToastImagePath);

                if (!this.HasPerformedCleanup)
                {
                    // First time we run, we'll perform cleanup of old images
                    this.HasPerformedCleanup = true;

                    foreach (var d in directory.EnumerateDirectories())
                    {
                        if (d.LastAccessTime.Date < DateTime.UtcNow.Date.AddDays(-3))
                        {
                            d.Delete(true);
                        }
                    }
                }

                string hashName = HashUtils.SHA1Hash(image) + ".png";
                string imagePath = Path.Combine(directory.FullName, hashName);

                if (File.Exists(imagePath))
                {
                    return imagePath;
                }

                byte[] imageData;

                if (isAvatar)
                {
                    imageData = await this.GroupMeClient.ImageDownloader.DownloadAvatarImageAsync(image, !isRounded);
                }
                else
                {
                    imageData = await this.GroupMeClient.ImageDownloader.DownloadPostImageAsync(image);
                }

                using (var fileStream = File.OpenWrite(imagePath))
                {
                    await fileStream.WriteAsync(imageData, 0, imageData.Length);
                }

                return imagePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
