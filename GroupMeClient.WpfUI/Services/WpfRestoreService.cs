using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
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
            var updateService = SimpleIoc.Default.GetInstance<IUpdateService>();

            if (updateService.IsInstalled)
            {
                // Do a reboot through Squirrel to allow for installing updates if required
                var assembly = Assembly.GetEntryAssembly();
                var updateDotExe = Path.Combine(
                    Path.GetDirectoryName(assembly.Location),
                    "..",
                    "Update.exe");

                var gmdcClientName = Path.GetFileName(Application.ResourceAssembly.Location);

                var psi = new ProcessStartInfo
                {
                    FileName = updateDotExe,
                    Arguments = $"-processStart {gmdcClientName} --process-start-args={Desktop.Native.Windows.RecoveryManager.RestartCommandLine}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process.Start(psi);
            }
            else
            {
                // If not installed just reboot normally
                Process.Start(Application.ResourceAssembly.Location, Desktop.Native.Windows.RecoveryManager.RestartCommandLine);
            }

            // Kill the current instance
            Application.Current.Shutdown();
        }
    }
}
