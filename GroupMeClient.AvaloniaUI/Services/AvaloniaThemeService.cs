using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Markup.Xaml.Styling;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings.Themes;

namespace GroupMeClient.AvaloniaUI.Services
{
    /// <summary>
    /// <see cref="AvaloniaThemeService"/> provides support for changing the GroupMe Desktop Client Avalonia theme at runtime.
    /// </summary>
    public class AvaloniaThemeService : IThemeService
    {
        private readonly StyleInclude avaloniaLightTheme = new StyleInclude(new Uri("resm:Styles?assembly=GroupMeClient.AvaloniaUI"))
        {
            Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseLight.xaml"),
        };

        private readonly StyleInclude avaloniaDarkTheme = new StyleInclude(new Uri("resm:Styles?assembly=GroupMeClient.AvaloniaUI"))
        {
            Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml"),
        };

        private readonly StyleInclude groupMeLightTheme = new StyleInclude(new Uri("resm:Styles?assembly=GroupMeClient.AvaloniaUI"))
        {
            Source = new Uri("avares://GroupMeClient.AvaloniaUI/GroupMeLight.xaml"),
        };

        private readonly StyleInclude groupMeDarkTheme = new StyleInclude(new Uri("resm:Styles?assembly=GroupMeClient.AvaloniaUI"))
        {
            Source = new Uri("avares://GroupMeClient.AvaloniaUI/GroupMeDark.xaml"),
        };

        /// <summary>
        /// Gets the style dictionary associated with the current base Avalonia theme.
        /// </summary>
        public StyleInclude CurrentAvaloniaTheme { get; private set; }

        /// <summary>
        /// Gets the style dictionary associated with the current GroupMe theme.
        /// </summary>
        public StyleInclude CurrentGroupMeTheme { get; private set; }

        private bool IsInitialized { get; set; }

        private bool IsPending { get; set; }

        /// <summary>
        /// Initializes the theme engine. The Main Window must be fully initialized prior to calling this method.
        /// </summary>
        public void Initialize()
        {
            Program.GroupMeMainWindow.Styles.Add(this.avaloniaLightTheme);
            Program.GroupMeMainWindow.Styles.Add(this.groupMeLightTheme);
            this.IsInitialized = true;

            if (this.IsPending)
            {
                // If any theme change requests were submitted prior to initialization,
                // apply them now.
                this.ApplyTheme();
                this.IsPending = false;
            }
        }

        /// <inheritdoc/>
        public void UpdateTheme(ThemeOptions theme)
        {
            switch (theme)
            {
                case ThemeOptions.Dark:
                    this.SetDarkTheme();
                    break;

                case ThemeOptions.Light:
                    this.SetLightTheme();
                    break;

                case ThemeOptions.Default:
                default:
                    this.SetSystemTheme();
                    break;
            }
        }

        /// <inheritdoc/>
        public void UpdateThemeStyle(string themeStyle)
        {
        }

        /// <inheritdoc/>
        public void UpdateTheme(AccessibilityChatFocusOptions option)
        {
        }

        /// <inheritdoc/>
        public void UpdateTheme(AccessibilityMessageFocusOptions option)
        {
        }

        /// <summary>
        /// Applies the light mode theme.
        /// </summary>
        private void SetLightTheme()
        {
            this.CurrentAvaloniaTheme = this.avaloniaLightTheme;
            this.CurrentGroupMeTheme = this.groupMeLightTheme;

            if (this.IsInitialized)
            {
                this.ApplyTheme();
            }
            else
            {
                this.IsPending = true;
            }
        }

        /// <summary>
        /// Applies the dark mode theme.
        /// </summary>
        private void SetDarkTheme()
        {
            this.CurrentAvaloniaTheme = this.avaloniaDarkTheme;
            this.CurrentGroupMeTheme = this.groupMeDarkTheme;

            if (this.IsInitialized)
            {
                this.ApplyTheme();
            }
            else
            {
                this.IsPending = true;
            }
        }

        /// <summary>
        /// Applies the system prefered theme.
        /// </summary>
        private void SetSystemTheme()
        {
            var useDarkTheme = false;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                useDarkTheme = !Native.Windows.WindowsUtils.IsAppLightThemePreferred();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                useDarkTheme = Native.MacOS.MacUtils.IsDarkModeEnabled();
            }

            if (useDarkTheme)
            {
                this.SetDarkTheme();
            }
            else
            {
                this.SetLightTheme();
            }
        }

        private void ApplyTheme()
        {
            Program.GroupMeMainWindow.Styles[0] = this.CurrentAvaloniaTheme;
            Program.GroupMeMainWindow.Styles[1] = this.CurrentGroupMeTheme;
        }

        /// <inheritdoc/>
        public List<string> GetAvailableThemeStyles()
        {
            return new List<string>();
        }
    }
}
