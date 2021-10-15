using System;
using System.Collections.Generic;
using System.Windows;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings.Themes;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfThemeService"/> provides support for changing the GMDC/Wpf theme at runtime.
    /// </summary>
    public class WpfThemeService : IThemeService
    {
        private string GMDCColorThemePrefix => "GMDC.Colors";

        private Dictionary<ThemeOptions, ResourceDictionary> GMDCColorThemes { get; } = new Dictionary<ThemeOptions, ResourceDictionary>()
        {
            { ThemeOptions.Light, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Themes/GMDC.Colors.Light.xaml") } },
            { ThemeOptions.Dark, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Themes/GMDC.Colors.Dark.xaml") } },
        };


        {

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
            this.ChangeTheme(this.GMDCColorThemePrefix, this.GMDCColorThemes[ThemeOptions.Light]);
        }

        /// <summary>
        /// Applies the dark mode theme.
        /// </summary>
        private void SetDarkTheme()
        {
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Cyan");
            this.ChangeTheme(this.GMDCColorThemePrefix, this.GMDCColorThemes[ThemeOptions.Dark]);
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

        private void ChangeTheme(string themePrefix, ResourceDictionary newTheme)
        {
            ResourceDictionary currentTheme = null;

            foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source?.ToString().Contains(themePrefix) ?? false)
                {
                    currentTheme = dictionary;
                }
            }

            if (currentTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(currentTheme);
            }

            if (newTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(newTheme);
            }
        }
    }
}
