using System.Windows;
using ToastNotifications.Core;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    /// <summary>
    /// Interaction logic for WpfToastDisplayPart.xaml
    /// </summary>
    public partial class GroupMeToastDisplayPart : NotificationDisplayPart
    {
        public GroupMeToastDisplayPart(GroupMeToastNotification groupMeToast)
        {
            InitializeComponent();
            Bind(groupMeToast);
        }

        public GroupMeToastDisplayPart()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            Notification.Close();
        }
    }
}
