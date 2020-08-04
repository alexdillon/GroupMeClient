using System.Windows;
using System.Windows.Documents;
using GroupMeClient.Core.Services;

namespace GroupMeClient.Wpf.Extensions
{
    /// <summary>
    /// <see cref="WebHyperlinkExtensions"/> provides extension methods to easily make HyperLinks that launch
    /// a webpage in the user's preferred browser.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/11433814.
    /// </remarks>
    public static class WebHyperlinkExtensions
    {
        /// <summary>
        /// A dependency property for to set whether a hyperlink is a web link.
        /// </summary>
        public static readonly DependencyProperty IsWebLinkProperty =
           DependencyProperty.RegisterAttached(
               "IsWebLink",
               typeof(bool),
               typeof(WebHyperlinkExtensions),
               new UIPropertyMetadata(false, OnIsExternalChanged));

        /// <summary>
        /// Gets a value indicating whether web link handling is enabled for a <see cref="Hyperlink"/>.
        /// </summary>
        /// <param name="obj">The dependency object to retreive the property from.</param>
        /// <returns>A value indicating whether web link handling is enabled.</returns>
        public static bool GetIsWebLink(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsWebLinkProperty);
        }

        /// <summary>
        /// Sets a value indicating whether web link handling is enabled for a <see cref="Hyperlink"/>.
        /// </summary>
        /// <param name="obj">The dependency object to apply the property to.</param>
        /// <param name="value">The value to apply.</param>
        public static void SetIsWebLink(DependencyObject obj, bool value)
        {
            obj.SetValue(IsWebLinkProperty, value);
        }

        private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var hyperlink = sender as Hyperlink;

            if ((bool)args.NewValue)
            {
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            }
            else
            {
                hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
            }
        }

        private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var osService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IOperatingSystemUIService>();
            osService.OpenWebBrowser(e.Uri.ToString());

            e.Handled = true;
        }
    }
}
