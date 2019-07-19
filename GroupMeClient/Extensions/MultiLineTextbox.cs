using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GroupMeClient.Extensions
{
    public class MultiLineSendBox : TextBox
    {
        public MultiLineSendBox()
        {
            this.KeyDown += this.TextBoxKeyDown;
            this.PreviewKeyDown += this.TextBoxPreviewKeyDown;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            // This will never happen because the Enter Key is handeled before
            // That means TextBoxKeyDown is not triggered for the Enter key

            if (e.Key == Key.Enter &&
                !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftShift)))
            {
                this.RaiseSendEvent();
            }
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Enter key is routed and the PreviewKeyDown is also fired with the
            // Enter key
            // You don't want to clear the box when CTRL and/or SHIFT is down
            if (e.Key == Key.Enter &&
                !(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                e.Handled = true;
                this.RaiseSendEvent();
            }
        }

        public static readonly RoutedEvent SendEvent = EventManager.RegisterRoutedEvent(
         "Send", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MultiLineSendBox));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Send
        {
            add { this.AddHandler(SendEvent, value); }
            remove { this.RemoveHandler(SendEvent, value); }
        }

        void RaiseSendEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SendEvent);
            this.RaiseEvent(newEventArgs);
        }
    }
}
