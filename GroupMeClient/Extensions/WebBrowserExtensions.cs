using System;
using System.Windows;
using System.Windows.Controls;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="WebBrowserExtensions"/> provides extension method for MVVM binding to <see cref="WebBrowser"/>.
    /// </summary>
    public static class WebBrowserExtensions
    {
        /// <summary>
        /// Gets or sets a dependency property that bound to the Web Browser Source property.
        /// </summary>
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserExtensions), new UIPropertyMetadata(null, BindableSourcePropertyChanged));

        /// <summary>
        /// Gets the Dependency Property binding for the Source.
        /// </summary>
        /// <param name="obj">Object to retreive binding from.</param>
        /// <returns>The Web Browser Source.</returns>
        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        /// <summary>
        /// Sets the bound source Dependency Property.
        /// </summary>
        /// <param name="obj">The Web Browser to apply the binding to.</param>
        /// <param name="value">The bound source.</param>
        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        private static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is WebBrowser browser)
            {
                string uri = e.NewValue as string;
                browser.Source = !string.IsNullOrEmpty(uri) ? new Uri(uri) : null;
            }
        }
    }
}
