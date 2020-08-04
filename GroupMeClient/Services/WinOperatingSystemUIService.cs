using GroupMeClient.Core.Services;

namespace GroupMeClient.Wpf.Services
{
    /// <summary>
    /// <see cref="WinOperatingSystemUIService"/> provides an implementation of the <see cref="IOperatingSystemUIService"/> service
    /// for the Windows platform.
    /// </summary>
    public class WinOperatingSystemUIService : IOperatingSystemUIService
    {
        /// <inheritdoc/>
        public void OpenWebBrowser(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
