using GroupMeClient.Core.Notifications.Display;
using GroupMeClient.Core.Settings;

namespace GroupMeClient.WpfUI.Notifications.Display
{
    /// <summary>
    /// <see cref="NativeDesktopNotificationProvider"/> provides support in GMDC/Wpf for creating Windows 10 Native Toast
    /// Notifications, or simulated ones for Windows 7/8.
    public class NativeDesktopNotificationProvider : PopupNotificationProvider
    {
        /// <summary>
        /// Creates a <see cref="PopupNotificationProvider"/> to display operating system level notifications.
        /// </summary>
        /// <param name="settingsManager">The settings instance to use.</param>
        /// <returns>A PopupNotificationProvider.</returns>
        public static PopupNotificationProvider CreatePlatformNotificationProvider(SettingsManager settingsManager)
        {
            var osVersion = System.Environment.OSVersion.Version;

            if (osVersion.Major == 10)
            {
                // UWP-type Toast Notifications are natively supported (modern Windows 10 builds)
                return CreateProvider(new Win10.Win10ToastNotificationsProvider(settingsManager));
            }
            else
            {
                // No system-level notification support (pre-Win 10)
                if (settingsManager.UISettings.EnableNonNativeNotifications)
                {
                    return CreateProvider(new Win7.Win7ToastNotificationsProvider(settingsManager));
                }
                else
                {
                    return CreateDoNothingProvider();
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="PopupNotificationProvider"/> to display internal (popup) toast notifications.
        /// </summary>
        /// <param name="manager">The manager to use for displaying notifications.</param>
        /// <returns>A PopupNotificationProvider.</returns>
        public static PopupNotificationProvider CreateInternalNotificationProvider(WpfToast.ToastHolderViewModel manager)
        {
            return CreateProvider(new WpfToast.WpfToastNotificationProvider(manager));
        }
    }
}
