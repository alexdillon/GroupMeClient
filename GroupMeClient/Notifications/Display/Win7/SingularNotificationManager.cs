using System;
using System.Windows;
using System.Windows.Threading;
using Notification.Wpf;
using Notification.Wpf.Controls;

namespace GroupMeClient.Wpf.Notifications.Display.Win7
{
    /// <summary>
    /// <see cref="SingularNotificationManager"/> provides an implementation of <see cref="Notification.Wpf.INotificationManager"/>
    /// that only allows for a single notification to be displayed near the task bar at any given time.
    /// </summary>
    public class SingularNotificationManager : INotificationManager
    {
        private static NotificationsOverlayWindow window;

        private static NotificationArea notificationArea;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingularNotificationManager"/> class.
        /// </summary>
        public SingularNotificationManager()
        {
            this.Dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

            var workArea = SystemParameters.WorkArea;

            window = new NotificationsOverlayWindow
            {
                Left = workArea.Left,
                Top = workArea.Top,
                Width = workArea.Width,
                Height = workArea.Height,
            };

            notificationArea = window.Content as NotificationArea;
            notificationArea.MaxItems = 1;

            window.Show();

            Application.Current.MainWindow.Loaded += (s, e) =>
            {
                window.Owner = Application.Current.MainWindow;
            };
        }

        private Dispatcher Dispatcher { get; }

        /// <inheritdoc/>
        public void Show(object content, string areaName = "", TimeSpan? expirationTime = null, Action onClick = null, Action onClose = null)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.Show(content, areaName, expirationTime, onClick, onClose)));
                return;
            }

            if (window != null && notificationArea != null)
            {
                if (!window.IsVisible)
                {
                    window.Visibility = Visibility.Visible;
                    window.Show();
                    window.WindowState = WindowState.Normal;
                }

                notificationArea.Show(
                    content,
                    expirationTime ?? TimeSpan.FromSeconds(5),
                    onClick,
                    onClose);
            }
        }
    }
}