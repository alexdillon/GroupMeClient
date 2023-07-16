using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace GroupMeClient.AvaloniaUI.Extensions
{
    /// <summary>
    /// <see cref="FocusOnLoadBehavior"/> provides support for assigning focus when a control loads.
    /// </summary>
    public class FocusOnLoadBehavior : Behavior<Visual>
    {
        /// <summary>
        /// Gets an Avalonia Property for the control to focus on load.
        /// </summary>
        public static readonly AvaloniaProperty FocusControlProperty =
            AvaloniaProperty.Register<FocusOnLoadBehavior, IInputElement>(
                nameof(FocusControl),
                inherits: false,
                defaultBindingMode: Avalonia.Data.BindingMode.OneTime);

        /// <summary>
        /// Gets or sets the command to execute when send behavior is invoked.
        /// </summary>
        public IInputElement FocusControl
        {
            get => (IInputElement)this.GetValue(FocusControlProperty);
            set => this.SetValue(FocusControlProperty, value);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.AttachedToVisualTree += this.AssociatedObject_AttachedToVisualTree;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            try
            {
                this.AssociatedObject.AttachedToVisualTree -= this.AssociatedObject_AttachedToVisualTree;
            }
            catch (Exception)
            {
            }
        }

        private void AssociatedObject_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            this.FocusControl.Focus();
        }
    }
}
