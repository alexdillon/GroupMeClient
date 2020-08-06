using System.Diagnostics;
using GroupMeClient.Core.Services;

namespace GroupMeClient.Desktop.Services
{
    /// <summary>
    /// <see cref="WinOperatingSystemUIService"/> provides an implementation of the <see cref="IOperatingSystemUIService"/> service
    /// for the Windows platform running on .NET Core or .NET Framework.
    /// </summary>
    public class WinOperatingSystemUIService : IOperatingSystemUIService
    {
        /// <inheritdoc/>
        public void OpenWebBrowser(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
    }
}
