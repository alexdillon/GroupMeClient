using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GroupMeClient.Extensions
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
            var routedEventArgs = eventArgs as RoutedEventArgs;
            if (routedEventArgs != null)
            {
                routedEventArgs.Handled = true;
            }

            base.OnEvent(eventArgs);
        }
    }
}
