using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DesktopNotifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace GroupMeClient.Notifications.Display.Win10
{
    /// <summary>
    /// Provides an adapter for <see cref="PopupNotificationProvider"/> to use Toast Notifications within the Client Window.
    /// </summary>
    public class Win10ToastNotificationsProvider : IPopupNotificationSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Win10ToastNotificationsProvider"/> class.
        /// </summary>
        public Win10ToastNotificationsProvider()
        {
            // Register AUMID and COM server (for Desktop Bridge apps, this no-ops)
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<GroupMeNotificationActivator>(this.ApplicationId);
            DesktopNotificationManagerCompat.RegisterActivator<GroupMeNotificationActivator>();
        }

        private string ApplicationId => "com.squirrel.GroupMeDesktopClient.GroupMeClient";

        private bool HasPerformedCleanup { get; set; } = false;

        private string ToastImagePath => Path.GetTempPath() + "WindowsNotifications.GroupMeToasts.Images";

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar)
        {
            ToastContent toastContent = new ToastContent()
            {
                Launch = "action=viewConversation&conversationId=5",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                            },
                            new AdaptiveText()
                            {
                                Text = body,
                            },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = await this.DownloadImageToDiskCached(
                                image: avatarUrl,
                                isAvatar: true,
                                isRounded: roundedAvatar),
                            HintCrop = roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default,
                        },
                    },
                },
            };

            this.ShowToast(toastContent);
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl)
        {
            ToastContent toastContent = new ToastContent()
            {
                Launch = "action=viewConversation&conversationId=5",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                            },
                            new AdaptiveText()
                            {
                                Text = body,
                            },
                            new AdaptiveImage()
                            {
                                 Source = await this.DownloadImageToDiskCached(imageUrl),
                            },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = await this.DownloadImageToDiskCached(
                                image: avatarUrl,
                                isAvatar: true,
                                isRounded: roundedAvatar),
                            HintCrop = roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default,
                        },
                    },
                },
            };

            this.ShowToast(toastContent);
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar)
        {
            ToastContent toastContent = new ToastContent()
            {
                Launch = "action=viewConversation&conversationId=5",

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                            },
                            new AdaptiveText()
                            {
                                Text = body,
                            },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = await this.DownloadImageToDiskCached(
                                image: avatarUrl,
                                isAvatar: true,
                                isRounded: roundedAvatar),
                            HintCrop = roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default,
                        },
                    },
                },
            };

            this.ShowToast(toastContent);
        }

        /// <inheritdoc/>
        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }

        private void ShowToast(ToastContent toastContent)
        {
            bool isActive = false;
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                isActive = Application.Current.MainWindow.IsActive;
            }));

            if (isActive)
            {
                // don't show Windows 10 Notifications if the window is focused.
                return;
            }

            var doc = new XmlDocument();
            doc.LoadXml(toastContent.GetContent());

            // And create the toast notification
            var toast = new ToastNotification(doc);

            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
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

                string hashName = this.Hash(image) + ".png";
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

        private string Hash(string input)
        {
            using (var sha1Managed = new SHA1Managed())
            {
                var hash = sha1Managed.ComputeHash(Encoding.UTF8.GetBytes(input));
                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }
    }
}
