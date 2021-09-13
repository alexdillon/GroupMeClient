using System;
using System.Windows;
using System.Windows.Input;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// Attached behavior that allows for propagating input bindings from a non-focused control to the root window.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/23432365.
    /// </remarks>
    public class InputBindingBehavior
    {
        /// <summary>
        /// Attached Property for enabling <see cref="InputBinding"/> propagation on a control.
        /// </summary>
        public static readonly DependencyProperty PropagateInputBindingsToWindowProperty = DependencyProperty.RegisterAttached(
             "PropagateInputBindingsToWindow",
             typeof(bool),
             typeof(InputBindingBehavior),
             new PropertyMetadata(false, OnPropagateInputBindingsToWindowChanged));

        /// <summary>
        /// Gets a value indicating whether input binding propagation is enabled for a given <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="obj">The element to retrive the property from.</param>
        /// <returns>A value indicating whether propagation is enabled.</returns>
        public static bool GetPropagateInputBindingsToWindow(FrameworkElement obj)
        {
            return (bool)obj.GetValue(PropagateInputBindingsToWindowProperty);
        }

        /// <summary>
        /// Sets the input binding propagation property on a given <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="obj">The element to set the property on.</param>
        /// <param name="value">The value to assign.</param>
        public static void SetPropagateInputBindingsToWindow(FrameworkElement obj, bool value)
        {
            obj.SetValue(PropagateInputBindingsToWindowProperty, value);
        }

        private static void OnPropagateInputBindingsToWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FrameworkElement).Loaded += FrameworkElement_Loaded;
        }

        private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            frameworkElement.Loaded -= FrameworkElement_Loaded;

            var window = Window.GetWindow(frameworkElement);
            if (window == null)
            {
                return;
            }

            // Move input bindings from the FrameworkElement to the window.
            for (int i = frameworkElement.InputBindings.Count - 1; i >= 0; i--)
            {
                var inputBinding = (InputBinding)frameworkElement.InputBindings[i];
                window.InputBindings.Add(inputBinding);
                frameworkElement.InputBindings.Remove(inputBinding);
            }
        }
    }
}
