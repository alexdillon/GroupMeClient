using System;
using System.Windows;
using System.Windows.Controls;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// Wrapper for <see cref="UserControl"/> that exposes <see cref="RoutedEvent"/>s around CLR events for binding.
    /// </summary>
    public class FocusedUserControl : UserControl
    {
        /// <summary>
        /// Routed event for wrapping the <see cref="UIElement.IsKeyboardFocusWithinChanged"/> event.
        /// </summary>
        public static readonly RoutedEvent ContainsKeyboardFocusChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ContainsKeyboardFocusChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(FocusedUserControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusedUserControl"/> class.
        /// </summary>
        public FocusedUserControl()
        {
            this.IsKeyboardFocusWithinChanged += this.FocusedUserControl_IsKeyboardFocusWithinChanged;
        }

        /// <summary>
        /// Routed Event wrapper for the <see cref="UIElement.IsKeyboardFocusWithinChanged"/> CLR event.
        /// </summary>
        public event RoutedEventHandler ContainsKeyboardFocusChanged
        {
            add => this.AddHandler(ContainsKeyboardFocusChangedEvent, value);
            remove => this.RemoveHandler(ContainsKeyboardFocusChangedEvent, value);
        }

        private void FocusedUserControl_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(ContainsKeyboardFocusChangedEvent);
            this.RaiseEvent(newEventArgs);
        }
    }
}
