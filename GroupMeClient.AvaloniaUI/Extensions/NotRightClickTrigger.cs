using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="NotRightClickTrigger"/> defines a XAML Behavior Trigger that is invoked by any mouse button press
    /// except for a right click. This allows for using custom mouse buttons, the center scroll button, and side clicker buttons
    /// for invoking functionality in the UI.
    /// </summary>
    public class NotRightClickTrigger : Trigger<AvaloniaObject>
    {
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (this.AssociatedObject is Control control)
            {
                control.PointerPressed += this.Element_PointerPressed;
            }
        }

        private void Element_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this.AssociatedObject as Control).Properties.IsRightButtonPressed)
            {
                Interaction.ExecuteActions(this.AssociatedObject, this.Actions, null);
            }
        }
    }
}
