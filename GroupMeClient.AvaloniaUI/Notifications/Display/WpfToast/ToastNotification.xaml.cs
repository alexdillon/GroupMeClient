using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GroupMeClient.AvaloniaUI.Notifications.Display.WpfToast
{
    public class ToastNotification : UserControl
    {
        public ToastNotification()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
