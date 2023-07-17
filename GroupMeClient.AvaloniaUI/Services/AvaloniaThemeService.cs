using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings.Themes;

namespace GroupMeClient.AvaloniaUI.Services
{
    /// <summary>
    /// <see cref="AvaloniaThemeService"/> provides support for changing the GroupMe Desktop Client Avalonia theme at runtime.
    /// </summary>
    public class AvaloniaThemeService : IThemeService
    {
        private readonly StyleInclude groupMeLightTheme = new StyleInclude(new Uri("resm:Styles?assembly=GroupMeClient.AvaloniaUI"))
        {
            Source = new Uri("avares://GroupMeClient.AvaloniaUI/GroupMeLight.axaml"),
        };

        private readonly StyleInclude groupMeDarkTheme = new StyleInclude(new Uri("resm:Styles?assembly=GroupMeClient.AvaloniaUI"))
        {
            Source = new Uri("avares://GroupMeClient.AvaloniaUI/GroupMeDark.axaml"),
        };

        /// <summary>
        /// Gets the style dictionary associated with the current GroupMe theme.
        /// </summary>
        public StyleInclude CurrentGroupMeTheme { get; private set; }

        /// <summary>
        /// Gets the theme variant associated with the current Avalonia theme.
        /// </summary>
        public ThemeVariant CurrentAvaloniaTheme { get; private set; }

        private bool IsInitialized { get; set; }

        private bool IsPending { get; set; }

        /// <summary>
        /// Initializes the theme engine. The Main Window must be fully initialized prior to calling this method.
        /// </summary>
        public void Initialize()
        {
            Program.GMDCMainWindow.Styles.Add(this.groupMeLightTheme);
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

        /// <inheritdoc/>
        public List<string> GetAvailableThemeStyles()
        {
            return new List<string>();
        }

        /// <summary>
        /// Applies the light mode theme.
        /// </summary>
        private void SetLightTheme()
        {
            this.CurrentAvaloniaTheme = ThemeVariant.Light;
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
            this.CurrentAvaloniaTheme = ThemeVariant.Dark;
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
            Application.Current.SetCurrentValue(Application.RequestedThemeVariantProperty, this.CurrentAvaloniaTheme);
            Program.GMDCMainWindow.Styles[0] = this.CurrentGroupMeTheme;
        }
    }
}
