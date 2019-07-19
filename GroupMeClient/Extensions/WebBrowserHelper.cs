namespace GroupMeClient.Extensions
{
    public class WebBrowserHelper
    {
        public static void OpenUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
