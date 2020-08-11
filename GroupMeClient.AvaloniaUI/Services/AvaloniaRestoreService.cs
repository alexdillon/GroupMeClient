using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Avalonia;
using GroupMeClient.Core.Services;

namespace GroupMeClient.AvaloniaUI.Services
{
    //TODO

    /// <summary>
    /// <see cref="AvaloniaRestoreService"/> provides support for managing the state of the application for the GMDC/WPF Client.
    /// </summary>
    public class AvaloniaRestoreService : IRestoreService
    {
        /// <inheritdoc/>
        public bool ShouldRestoreState => false; // Environment.GetCommandLineArgs().Contains(Native.RecoveryManager.RestartCommandLine);

        /// <inheritdoc/>
        public void HardApplicationRestart()
        {
            //Process.Start(Application.Current..Location);
            //Process.GetCurrentProcess().Kill();
        }

        /// <inheritdoc/>
        public void SoftApplicationRestart()
        {
            //Process.Start(Application.ResourceAssembly.Location, RecoveryManager.RestartCommandLine);
            //Application.Current.Shutdown();
        }
    }
}
