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
        public static readonly DirectProperty<FocusOnLoadBehavior, IInputElement> FocusControlProperty =
            AvaloniaProperty.RegisterDirect<FocusOnLoadBehavior, IInputElement>(
                nameof(FocusControl),
                o => o.FocusControl,
                (o, v) => o.FocusControl = v,
                defaultBindingMode: Avalonia.Data.BindingMode.OneTime);

        private IInputElement focusControl;

        /// <summary>
        /// Gets or sets the command to execute when send behavior is invoked.
        /// </summary>
        public IInputElement FocusControl
        {
            get => this.focusControl;
            set => this.SetAndRaise(FocusControlProperty, ref this.focusControl, value);
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
            this.FocusControl?.Focus();
        }
    }
}
