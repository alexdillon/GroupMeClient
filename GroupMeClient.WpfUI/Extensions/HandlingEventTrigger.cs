using System;
using System.Windows;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// <see cref="HandlingEventTrigger"/> is an event trigger behavior that ensures the source event is marked
    /// as handled to prevent it from bubbling up to high-level controls in the visual tree.
    /// </summary>
    public class HandlingEventTrigger : Microsoft.Xaml.Behaviors.EventTrigger
    {
        /// <inheritdoc/>
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            if (eventArgs is RoutedEventArgs routedEventArgs)
            {
                routedEventArgs.Handled = true;
            }

            base.OnEvent(eventArgs);
        }
    }
}
