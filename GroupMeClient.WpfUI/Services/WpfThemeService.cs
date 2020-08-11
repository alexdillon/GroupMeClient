using System;
using System.Windows;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings;
using MahApps.Metro;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfThemeService"/> provides support for changing the GMDC/Wpf theme at runtime.
    /// </summary>
    public class WpfThemeService : IThemeService
    {
        private readonly ResourceDictionary groupMeLightTheme = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Styles/GroupMeLight.xaml"),
        };

        private readonly ResourceDictionary groupMeDarkTheme = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/Styles/GroupMeDark.xaml"),
        };

        private ResourceDictionary currentGroupMeTheme = null;

        private ResourceDictionary CurrentGroupMeTheme
        {
            get
            {
                if (this.currentGroupMeTheme == null)
                {
                    foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
                    {
                        if (dictionary.Source?.ToString().Contains("GroupMe") ?? false)
                        {
                            this.currentGroupMeTheme = dictionary;
                        }
                    }
                }

                return this.currentGroupMeTheme;
            }

            set
            {
                Application.Current.Resources.MergedDictionaries.Remove(this.CurrentGroupMeTheme);
                Application.Current.Resources.MergedDictionaries.Add(value);
                this.currentGroupMeTheme = value;
            }
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            // No initialization needed.
        }

        /// <summary>
        /// Updates the current theme of the application.
        /// </summary>
        /// <param name="theme">The new theme mode to apply.</param>
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

        /// <summary>
        /// Applies the light mode theme.
        /// </summary>
        private void SetLightTheme()
        {
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, "Light.Cyan");
            this.CurrentGroupMeTheme = this.groupMeLightTheme;
        }

        /// <summary>
        /// Applies the dark mode theme.
        /// </summary>
        private void SetDarkTheme()
        {
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Cyan");
            this.CurrentGroupMeTheme = this.groupMeDarkTheme;
        }

        /// <summary>
        /// Applies the system prefered theme.
        /// </summary>
        private void SetSystemTheme()
        {
            if (Native.WindowsThemeUtils.IsAppLightThemePreferred())
            {
                this.SetLightTheme();
            }
            else
            {
                this.SetDarkTheme();
            }

            Native.WindowsThemeUtils.ThemeUpdateHook.Instance.ThemeChangedEvent -= this.Windows_ThemeChangedEvent;
            Native.WindowsThemeUtils.ThemeUpdateHook.Instance.ThemeChangedEvent += this.Windows_ThemeChangedEvent;
        }

        private void Windows_ThemeChangedEvent()
        {
            this.SetSystemTheme();
        }
    }
}
