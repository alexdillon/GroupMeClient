using System.Windows;
using System.Windows.Input;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// <see cref="NotRightClickTrigger"/> defines a XAML Behavior Trigger that is invoked by any mouse button press
    /// except for a right click. This allows for using custom mouse buttons, the center scroll button, and side clicker buttons
    /// for invoking functionality in the UI.
    /// </summary>
    public class NotRightClickTrigger : Microsoft.Xaml.Behaviors.TriggerBase<DependencyObject>
    {
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject is UIElement element)
            {
                element.MouseDown += this.Element_MouseDown;
            }
        }

        private void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right)
            {
                this.InvokeActions(e);
            }
        }
    }
}
