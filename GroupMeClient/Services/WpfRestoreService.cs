using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using GroupMeClient.Core.Services;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfRestoreService"/> provides support for managing the state of the application for the GMDC/WPF Client.
    /// </summary>
    public class WpfRestoreService : IRestoreService
    {
        /// <inheritdoc/>
        public bool ShouldRestoreState => Environment.GetCommandLineArgs().Contains(Desktop.Native.Windows.RecoveryManager.RestartCommandLine);

        /// <inheritdoc/>
        public void HardApplicationRestart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Process.GetCurrentProcess().Kill();
        }

        /// <inheritdoc/>
        public void SoftApplicationRestart()
        {
            Process.Start(Application.ResourceAssembly.Location, Desktop.Native.Windows.RecoveryManager.RestartCommandLine);
            Application.Current.Shutdown();
        }
    }
}
