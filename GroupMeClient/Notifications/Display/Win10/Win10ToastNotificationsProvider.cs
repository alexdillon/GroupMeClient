using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GroupMeClientApi.Models;
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
        /// <param name="settingsManager">The settings instance to use.</param>
        public Win10ToastNotificationsProvider(Settings.SettingsManager settingsManager)
        {
            // Register AUMID and COM server (for Desktop Bridge apps, this no-ops)
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<GroupMeNotificationActivator>(ApplicationId);
            DesktopNotificationManagerCompat.RegisterActivator<GroupMeNotificationActivator>();

            this.SettingsManager = settingsManager;
        }

        /// <summary>
        /// <see cref="LaunchActions"/> define the actions that can be performed upon activation of a Windows 10 Notification.
        /// </summary>
        public enum LaunchActions
        {
            /// <summary>
            /// The <see cref="Group"/> or <see cref="Chat"/> should be opened and displayed.
            /// </summary>
            ShowGroup,

            /// <summary>
            /// The <see cref="Member"/> should be liked.
            /// </summary>
            LikeMessage,

            /// <summary>
            /// A quick reply should be initiated.
            /// </summary>
            InitiateReplyMessage,

            /// <summary>
            /// A quick reply is completed and ready to send.
            /// </summary>
            SendReplyMessage,
        }

        /// <summary>
        /// Gets the AUMID identifier used for Windows 10 Toast Notifications.
        /// </summary>
        public static string ApplicationId => "com.squirrel.GroupMeDesktopClient.GroupMeClient";

        private bool HasPerformedCleanup { get; set; } = false;

        private Settings.SettingsManager SettingsManager { get; }

        private string ToastImagePath => Path.GetTempPath() + "WindowsNotifications.GroupMeToasts.Images";

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar, string containerId)
        {
            ToastContent toastContent = new ToastContent()
            {
                Launch = $"action={LaunchActions.ShowGroup}&conversationId={containerId}",

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
        async Task IPopupNotificationSink.ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl, string containerId, string messageId)
        {
            var avatar = await this.DownloadImageToDiskCached(
                              image: avatarUrl,
                              isAvatar: true,
                              isRounded: roundedAvatar);

            ToastActionsCustom actions = null;

            if (this.SettingsManager.UISettings.EnableNotificationInteractions)
            {
                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());
                var source = groupsAndChats.FirstOrDefault(g => g.Id == containerId);

                actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Like", $"action={LaunchActions.LikeMessage}&conversationId={containerId}&messageId={messageId}")
                        {
                            ActivationType = ToastActivationType.Background,
                        },

                        new ToastButton("Reply", $"action={LaunchActions.InitiateReplyMessage}&conversationId={containerId}&messageId={messageId}&containerName={source.Name}&containerAvatar={avatar}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ActivationOptions = new ToastActivationOptions()
                            {
                                AfterActivationBehavior = ToastAfterActivationBehavior.PendingUpdate,
                            },
                        },
                    },
                };
            }

            ToastContent toastContent = new ToastContent()
            {
                Launch = $"action={LaunchActions.ShowGroup}&conversationId={containerId}",

                Actions = actions,

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
                            Source = avatar,
                            HintCrop = roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default,
                        },
                    },
                },
            };

            this.ShowToast(toastContent, $"{containerId}{messageId}");
        }

        /// <inheritdoc/>
        async Task IPopupNotificationSink.ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar, string containerId, string messageId)
        {
            var avatar = await this.DownloadImageToDiskCached(
                                image: avatarUrl,
                                isAvatar: true,
                                isRounded: roundedAvatar);

            ToastActionsCustom actions = null;

            if (this.SettingsManager.UISettings.EnableNotificationInteractions)
            {
                var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Groups(), this.GroupMeClient.Chats());
                var source = groupsAndChats.FirstOrDefault(g => g.Id == containerId);

                actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Like", $"action={LaunchActions.LikeMessage}&conversationId={containerId}&messageId={messageId}")
                        {
                            ActivationType = ToastActivationType.Background,
                        },

                        new ToastButton("Reply", $"action={LaunchActions.InitiateReplyMessage}&conversationId={containerId}&messageId={messageId}&containerName={source.Name}&containerAvatar={avatar}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ActivationOptions = new ToastActivationOptions()
                            {
                                AfterActivationBehavior = ToastAfterActivationBehavior.PendingUpdate,
                            },
                        },
                    },
                };
            }

            ToastContent toastContent = new ToastContent()
            {
                Launch = $"action={LaunchActions.ShowGroup}&conversationId={containerId}",

                Actions = actions,

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
                            Source = avatar,
                            HintCrop = roundedAvatar ? ToastGenericAppLogoCrop.Circle : ToastGenericAppLogoCrop.Default,
                        },
                    },
                },
            };

            this.ShowToast(toastContent, $"{containerId}{messageId}");
        }

        /// <inheritdoc/>
        void IPopupNotificationSink.RegisterClient(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
        }

        private void ShowToast(ToastContent toastContent, string tag = "")
        {
            bool isActive = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                isActive = Application.Current.MainWindow?.IsActive ?? false;
            });

            if (isActive)
            {
                // don't show Windows 10 Notifications if the window is focused.
                return;
            }

            var doc = new XmlDocument();
            doc.LoadXml(toastContent.GetContent());

            if (string.IsNullOrEmpty(tag))
            {
                // Windows 10 likes to not show Toasts with a blank tag
                tag = Guid.NewGuid().ToString().Substring(0, 15);
            }

            // And create the toast notification
            var toast = new ToastNotification(doc)
            {
                Tag = tag,
            };

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
