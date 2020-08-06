using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GroupMeClient.Desktop.Native.Windows
{
    /// <summary>
    /// <see cref="RecoveryManager"/> provides support for Windows Error Recovery automatic application restart.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/questions/32520036/restart-a-crashed-program-with-registerapplicationrestart-without-user-prompt.
    /// </remarks>
    public class RecoveryManager
    {
        /// <summary>
        /// Defines a delegate for callbacks after a Windows Recovery event occurs.
        /// </summary>
        /// <param name="parameter">Context information specified for recovery.</param>
        /// <returns>The return value is not used and should be 0.</returns>
        public delegate int RecoveryDelegate(IntPtr parameter);

        /// <summary>
        /// Gets the command line argument that is used to indicate that an expected restart has occured.
        /// </summary>
        public static string RestartCommandLine => "/restart";

        /// <summary>
        /// Enables Windows Error Recovery automatic recovery.
        /// Recovery allows for immediately resuming an application after a crash or hang occurs.
        /// </summary>
        public static void RegisterForRecovery()
        {
            var callback = new RecoveryDelegate(p =>
            {
                Process.Start(Assembly.GetEntryAssembly().Location, RestartCommandLine);
                ApplicationRecoveryFinished(true);
                return 0;
            });

            var interval = 100U;
            var flags = 0U;

            var result = RegisterApplicationRecoveryCallback(callback, IntPtr.Zero, interval, flags);
        }

        /// <summary>
        /// Enables Windows Error Recovery automatic restart.
        /// Marking this application as WER aware also allows for automatic application restart after system updates or reboot.
        /// For more information, see https://docs.microsoft.com/en-us/windows/win32/recovery/registering-for-application-restart.
        /// </summary>
        public static void RegisterForRestart()
        {
            RegisterApplicationRestart(RestartCommandLine, 0);
        }

        [DllImport("kernel32.dll")]
        private static extern int RegisterApplicationRecoveryCallback(
              RecoveryDelegate recoveryCallback,
              IntPtr parameter,
              uint pingInterval,
              uint flags);

        [DllImport("kernel32.dll")]
        private static extern void ApplicationRecoveryFinished(bool success);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int RegisterApplicationRestart(
            [MarshalAs(UnmanagedType.LPWStr)] string commandLineArgs,
            int flags);
    }
}
