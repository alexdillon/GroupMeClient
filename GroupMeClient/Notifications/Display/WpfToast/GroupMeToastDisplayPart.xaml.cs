using System.Windows;
using ToastNotifications.Core;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    /// <summary>
    /// Interaction logic for WpfToastDisplayPart.xaml.
    /// </summary>
    public partial class GroupMeToastDisplayPart : NotificationDisplayPart
    {
        public GroupMeToastDisplayPart(GroupMeToastNotification groupMeToast)
        {
            this.InitializeComponent();
            this.Bind(groupMeToast);
        }

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
