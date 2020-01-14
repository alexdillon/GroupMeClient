using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GroupMeClient.Native
{
    /// <summary>
    /// <see cref="WindowsThemeUtils"/> provides helper methods to invoke native operating system themeing functionality when running on Windows.
    /// </summary>
    public class WindowsThemeUtils
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Native Win32 API Name")]
        private const int WM_SETTINGCHANGE = 0x001A;

        private const string ImmersiveColorSetParameter = "ImmersiveColorSet";

        /// <summary>
        /// Gets a value indicating whether Windows prefers for user apps to be in a light color theme.
        /// </summary>
        /// <returns>True if light theme is preferred, false otherwise.</returns>
        /// <remarks>
        /// Adapted from https://stackoverflow.com/questions/44713412/how-can-i-get-whether-windows-10-anniversary-update-or-later-is-using-its-light.
        /// </remarks>
        public static bool IsAppLightThemePreferred()
        {
            bool result = true;
            try
            {
                var v = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");
                if (v != null && v.ToString() == "0")
                {
                    result = false;
                }
            }
            catch
            {
            }

            return result;
        }

        /// <summary>
        /// <see cref="ThemeUpdateHook"/> provides a singleton for installing a Windows System Hook for monitoring for theme changes.
        /// </summary>
        public class ThemeUpdateHook
        {
            private static readonly Lazy<ThemeUpdateHook> LazyPluginManager = new Lazy<ThemeUpdateHook>(() => new ThemeUpdateHook());

            private ThemeUpdateHook()
            {
                if (new WindowInteropHelper(Application.Current.MainWindow).Handle != IntPtr.Zero)
                {
                    // A non-zero Window Handle exists, meaning initialization has already completed.
                    // Install the hook now
                    this.RegisterHook();
                }
                else
                {
                    // Wait for the window to initialize, then install the hook
                    PresentationSource.AddSourceChangedHandler(Application.Current.MainWindow, this.HandleChanged);
                }
            }

            /// <summary>
            /// A <see cref="Delegate"/> to respond to a change in Windows system theme.
            /// </summary>
            public delegate void ThemeChangedEventDelegate();

            /// <summary>
            /// <see cref="ThemeChangedEvent"/> is raised when the Windows system-level color theme changes.
            /// </summary>
            public event ThemeChangedEventDelegate ThemeChangedEvent;

            /// <summary>
            /// Gets the instance of the <see cref="PluginManager"/> for the current application.
            /// </summary>
            public static ThemeUpdateHook Instance => LazyPluginManager.Value;

            private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                switch (msg)
                {
                    case WM_SETTINGCHANGE:
                        var lParamString = Marshal.PtrToStringUni(lParam);
                        if (lParamString == ImmersiveColorSetParameter)
                        {
                            ThemeUpdateHook.Instance.ThemeChangedEvent?.Invoke();
                            handled = true;
                        }

                        break;
                }

                return IntPtr.Zero;
            }

            private void HandleChanged(object sender, SourceChangedEventArgs e)
            {
                if (e.NewSource is HwndSource)
                {
                    this.RegisterHook();
                }
            }

            private void RegisterHook()
            {
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            }
        }
    }
}
