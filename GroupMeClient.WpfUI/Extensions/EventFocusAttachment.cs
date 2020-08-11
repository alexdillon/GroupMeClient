using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// <see cref="EventFocusAttachment"/> provides support for assigning focus after a <see cref="ToggleButton"/> is clicked.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/2212927.
    /// </remarks>
    public class EventFocusAttachment
    {
        /// <summary>
        /// Gets a property indicating which element should be focused when the <see cref="ToggleButton"/> is activated.
        /// </summary>
        public static readonly DependencyProperty ElementToFocusProperty =
            DependencyProperty.RegisterAttached(
              "ElementToFocus",
              typeof(Control),
              typeof(EventFocusAttachment),
              new UIPropertyMetadata(null, ElementToFocusPropertyChanged));

        /// <summary>
        /// Gets a value indicating which control will be focused.
        /// </summary>
        /// <param name="button">The dependency object to retreive the property from.</param>
        /// <returns>The control that will receive focus.</returns>
        public static Control GetElementToFocus(ToggleButton button)
        {
            return (Control)button.GetValue(ElementToFocusProperty);
        }

        /// <summary>
        /// Sets a value indicating which control will receive focus.
        /// </summary>
        /// <param name="button">The dependency object to apply the property to.</param>
        /// <param name="value">The control that will receive focus.</param>
        public static void SetElementToFocus(ToggleButton button, Control value)
        {
            button.SetValue(ElementToFocusProperty, value);
        }

        private static void ElementToFocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                button.Click += (s, args) =>
                {
                    Control control = GetElementToFocus(button);
                    if (control != null)
                    {
                        control.Focus();
                    }
                };
            }
        }
    }
}
