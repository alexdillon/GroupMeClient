using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GroupMeClient.Settings
{
    /// <summary>
    /// <see cref="UISettings"/> defines the settings for customizing the user interface.
    /// </summary>
    public class UISettings
    {
        private ThemeOptions theme;

        /// <summary>
        /// Gets or sets a value indicating whether messages containing mutliple images are shown as previews.
        /// GroupMe UWP and Web display small-resolution preview versions when multiple images are contained in a single message.
        /// If enabled, preview versions will be shown. If disabled, full resolution versions will be shown instead.
        /// </summary>
        public bool ShowPreviewsForMultiImages { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating the maximum number of chats that can be opened at any given time
        /// when the program is in regular mode (with the full left sidebar displayed).
        /// </summary>
        public int MaximumNumberOfMultiChatsNormal { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value indicating the maximum number of chats that can be opened at any given time
        /// when the left sidebar is collapsed (minibar mode).
        /// </summary>
        public int MaximumNumberOfMultiChatsMinibar { get; set; } = 4;

        /// <summary>
        /// Gets or sets a value indicating whether interacting with system notifications is permitted. Interactions include
        /// shortcuts for liking messages and quickly replying to a group or chat.
        /// </summary>
        public bool EnableNotificationInteractions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating that scaling factor that should be applied when displaying messages.
        /// </summary>
        public double ScalingFactorForMessages { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the user selected theme that should be applied to the entire application UI.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ThemeOptions Theme
        {
            get => this.theme;

            set
            {
                this.theme = value;
                switch (this.Theme)
                {
                    case ThemeOptions.Light:
                        Themes.ThemeManager.SetLightTheme();
                        break;
                    case ThemeOptions.Dark:
                        Themes.ThemeManager.SetDarkTheme();
                        break;
                    case ThemeOptions.Default:
                        Themes.ThemeManager.SetSystemTheme();
                        break;
                }
            }
        }
    }
}
