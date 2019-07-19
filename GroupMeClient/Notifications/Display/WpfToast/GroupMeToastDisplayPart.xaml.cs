using System.Windows;
using ToastNotifications.Core;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    /// <summary>
    /// Interaction logic for WpfToastDisplayPart.xaml.
    /// </summary>
    public partial class GroupMeToastDisplayPart : NotificationDisplayPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeToastDisplayPart"/> class bound to a ViewModel.
        /// </summary>
        /// <param name="groupMeToast">The ViewModel to bind to.</param>
        public GroupMeToastDisplayPart(GroupMeToastNotification groupMeToast)
        {
            this.InitializeComponent();
            this.Bind(groupMeToast);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeToastDisplayPart"/> class.
        /// </summary>
        public GroupMeToastDisplayPart()
        {
            this.InitializeComponent();
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            this.Notification.Close();
        }
    }
}
