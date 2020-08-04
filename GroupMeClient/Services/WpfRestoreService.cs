using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using GroupMeClient.Core.Services;
using GroupMeClient.Wpf.Native;

namespace GroupMeClient.Wpf.Services
{
    /// <summary>
    /// <see cref="WpfRestoreService"/> provides support for managing the state of the application for the GMDC/WPF Client.
    /// </summary>
    public class WpfRestoreService : IRestoreService
    {
        /// <inheritdoc/>
        public bool ShouldRestoreState => Environment.GetCommandLineArgs().Contains(Native.RecoveryManager.RestartCommandLine);

        /// <inheritdoc/>
        public void HardApplicationRestart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Process.GetCurrentProcess().Kill();
        }

        /// <inheritdoc/>
        public void SoftApplicationRestart()
        {
            Process.Start(Application.ResourceAssembly.Location, RecoveryManager.RestartCommandLine);
            Application.Current.Shutdown();
        }
    }
}
