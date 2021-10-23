using System.Collections.Generic;
using GroupMeClient.Core.Settings.Themes;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IThemeService"/> defines a platform-independent service for applying themes to the GMDC Application.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Initializes the Theme Engine after the UI and Backend have started successfully.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates the currently active theme.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        void UpdateTheme(ThemeOptions theme);

        /// <summary>
        /// Updates the currently active styling that is applied on top of the current light or dark theme.
        /// </summary>
        /// <param name="themeStyle">The name of the style to apply.</param>
        void UpdateThemeStyle(string themeStyle);

        /// <summary>
        /// Updates the current accessibility theming for the indicator that is applied to focused chats.
        /// </summary>
        /// <param name="option">The new option to apply.</param>
        void UpdateTheme(AccessibilityChatFocusOptions option);

        /// <summary>
        /// Updates the current accessibility theming for the indicator that is applied to selected messages.
        /// </summary>
        /// <param name="option">The new option to apply.</param>
        void UpdateTheme(AccessibilityMessageFocusOptions option);

        /// <summary>
        /// Gets a list of available theme styles that can be applied on top of the base themes.
        /// </summary>
        /// <returns>A list of theme style names.</returns>
        List<string> GetAvailableThemeStyles();
    }
}
