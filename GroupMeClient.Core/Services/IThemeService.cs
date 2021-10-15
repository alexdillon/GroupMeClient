﻿using GroupMeClient.Core.Settings;
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
        /// Updates the current accessibility theming for the indicator that is applied to focused chats.
        /// </summary>
        /// <param name="option">The new option to apply.</param>
        void UpdateTheme(AccessibilityChatFocusOptions option);
    }
}
