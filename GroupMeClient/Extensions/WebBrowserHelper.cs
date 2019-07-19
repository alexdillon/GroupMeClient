namespace GroupMeClient.Extensions
{
    /// <summary>
    /// <see cref="WebBrowserHelper"/> provides helper method for interacting with the system browser.
    /// </summary>
    public class WebBrowserHelper
    {
        /// <summary>
        /// Opens a Url in the user's default browser.
        /// </summary>
        /// <param name="url">The url to open.</param>
        public static void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
