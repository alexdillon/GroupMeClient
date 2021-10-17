using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="WpfThemeService"/> class.
        /// </summary>
        public WpfThemeService()
        {
            // None of the WPF themeing needs delayed initialization.
            this.Initialize();
        }

        private bool IsLightTheme { get; set; } = true;

        private string GMDCDynamicColorsPrefix => "GMDC.BaseColors";

        private ResourceDictionary GMDCDynamicColors => new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Themes/GMDC.BaseColors.xaml") };

        private string GMDCColorThemePrefix => "GMDC.Colors";

        private Dictionary<ThemeOptions, ResourceDictionary> GMDCColorThemes { get; } = new Dictionary<ThemeOptions, ResourceDictionary>()
        {
            { ThemeOptions.Light, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Themes/GMDC.Colors.Light.xaml") } },
            { ThemeOptions.Dark, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Themes/GMDC.Colors.Dark.xaml") } },
        };

        private string DefaultThemeStyle => "GMDC";

        private string CurrentThemeStyle { get; set; }

        private Dictionary<string, (ResourceDictionary light, ResourceDictionary dark)> ThemeStyles { get; } = new Dictionary<string, (ResourceDictionary light, ResourceDictionary dark)>();

        private string GMDCAccessibilityChatFocusPrefix => "GMDC.Accessibility.ChatFocus";

        private Dictionary<AccessibilityChatFocusOptions, ResourceDictionary> GMDCAccessibilityChatFocus { get; } = new Dictionary<AccessibilityChatFocusOptions, ResourceDictionary>()
        {
            { AccessibilityChatFocusOptions.None, null },
            { AccessibilityChatFocusOptions.Bar, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Accessibility/GMDC.Accessibility.ChatFocus.Bar.xaml") } },
            { AccessibilityChatFocusOptions.Border, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Accessibility/GMDC.Accessibility.ChatFocus.Border.xaml") } },
        };

        private string GMDCAccessibilityMessageFocusPrefix => "GMDC.Accessibility.MessageFocus";

        private Dictionary<AccessibilityMessageFocusOptions, ResourceDictionary> GMDCAccessibilityMessageFocus { get; } = new Dictionary<AccessibilityMessageFocusOptions, ResourceDictionary>()
        {
            { AccessibilityMessageFocusOptions.None, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Accessibility/GMDC.Accessibility.MessageFocus.None.xaml") } },
            { AccessibilityMessageFocusOptions.Border, new ResourceDictionary() { Source = new Uri("pack://application:,,,/Resources/Accessibility/GMDC.Accessibility.MessageFocus.Border.xaml") } },
        };

        /// <inheritdoc/>
        public void Initialize()
        {
            // Load custom themes
            var files = Directory.GetFiles(App.ThemesPath, "*.xaml");
            var themes = files
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(f => f.EndsWith(".Light", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".Dark", StringComparison.OrdinalIgnoreCase))
                .Select(f => f.Substring(0, f.LastIndexOf(".")))
                .Distinct();

            this.ThemeStyles.Add(this.DefaultThemeStyle, (null, null));

            foreach (var theme in themes)
            {
                var lightThemePath = Path.Combine(App.ThemesPath, $"{theme}.Light.xaml");
                var darkThemePath = Path.Combine(App.ThemesPath, $"{theme}.Dark.xaml");
                ResourceDictionary lightDictionary = null;
                ResourceDictionary darkDictionary = null;

                if (File.Exists(lightThemePath))
                {
                    lightDictionary = new ResourceDictionary() { Source = new Uri(lightThemePath) };
                }

                if (File.Exists(darkThemePath))
                {
                    darkDictionary = new ResourceDictionary() { Source = new Uri(darkThemePath) };
                }

                if (!this.ThemeStyles.ContainsKey(theme))
                {
                    this.ThemeStyles.Add(theme, (lightDictionary, darkDictionary));
                }
            }
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

            // Change the user applied styling to match the new base theme
            this.UpdateThemeStyle(this.CurrentThemeStyle);
        }

        /// <inheritdoc/>
        public void UpdateThemeStyle(string themeStyle)
        {
            if (string.IsNullOrEmpty(themeStyle) || !this.ThemeStyles.ContainsKey(themeStyle))
            {
                this.CurrentThemeStyle = this.DefaultThemeStyle;
                this.ChangeStyle(this.ThemeStyles[this.DefaultThemeStyle]);
            }
            else
            {
                this.CurrentThemeStyle = themeStyle;
                this.ChangeStyle(this.ThemeStyles[themeStyle]);
            }
        }

        /// <summary>
        /// Updates the current accessibility theming for the indicator that is applied to focused chats.
        /// </summary>
        /// <param name="option">The new option to apply.</param>
        public void UpdateTheme(AccessibilityChatFocusOptions option)
        {
            this.ChangeTheme(this.GMDCAccessibilityChatFocusPrefix, this.GMDCAccessibilityChatFocus[option]);
        }

        /// <summary>
        /// Updates the current accessibility theming for the indicator that is applied to selected messages.
        /// </summary>
        /// <param name="option">The new option to apply.</param>
        public void UpdateTheme(AccessibilityMessageFocusOptions option)
        {
            this.ChangeTheme(this.GMDCAccessibilityMessageFocusPrefix, this.GMDCAccessibilityMessageFocus[option]);
        }

        /// <inheritdoc/>
        public List<string> GetAvailableThemeStyles()
        {
            return this.ThemeStyles.Keys.ToList();
        }

        /// <summary>
        /// Applies the light mode theme.
        /// </summary>
        private void SetLightTheme()
        {
            this.IsLightTheme = true;
            ControlzEx.Theming.ThemeManager.Current.ChangeTheme(Application.Current, "Light.Cyan");
            this.ChangeTheme(this.GMDCColorThemePrefix, this.GMDCColorThemes[ThemeOptions.Light]);
        }

        /// <summary>
        /// Applies the dark mode theme.
        /// </summary>
        private void SetDarkTheme()
        {
            this.IsLightTheme = false;
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

            // Change the user applied styling to match the new base theme
            this.UpdateThemeStyle(this.CurrentThemeStyle);
        }

        private void ChangeTheme(string themePrefix, ResourceDictionary newTheme)
        {
            ResourceDictionary currentTheme = null;

            foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source?.ToString().Contains(themePrefix) ?? false)
                {
                    currentTheme = dictionary;
                    break;
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

        private void ChangeStyle((ResourceDictionary lightTheme, ResourceDictionary darkTheme) themeStyle)
        {
            // Remove the existing custom style
            foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Contains("IsCustomStyle"))
                {
                    Application.Current.Resources.MergedDictionaries.Remove(dictionary);
                    break;
                }
            }

            // Apply a new custom style if available for the current base theme
            var targetStyle = this.IsLightTheme ? themeStyle.lightTheme : themeStyle.darkTheme;
            if (targetStyle != null)
            {
                Application.Current.Resources.MergedDictionaries.Add(targetStyle);
            }

            this.ResetDynamicBaseColors();
        }

        private void ResetDynamicBaseColors()
        {
            // Remove the existing custom style
            foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
            {
                if (dictionary.Source?.ToString().Contains(this.GMDCDynamicColorsPrefix) ?? false)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(dictionary);
                    break;
                }
            }

            Application.Current.Resources.MergedDictionaries.Add(this.GMDCDynamicColors);
        }
    }
}
