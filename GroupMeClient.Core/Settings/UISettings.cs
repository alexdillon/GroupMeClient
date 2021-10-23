using System;
using System.Reactive.Subjects;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings.Themes;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GroupMeClient.Core.Settings
{
    /// <summary>
    /// <see cref="UISettings"/> defines the settings for customizing the user interface.
    /// </summary>
    public class UISettings
    {
        private readonly BehaviorSubject<ThemeOptions> theme = new BehaviorSubject<ThemeOptions>(ThemeOptions.Default);

        private readonly BehaviorSubject<AccessibilityChatFocusOptions> accessibilityChatFocusOption = new BehaviorSubject<AccessibilityChatFocusOptions>(AccessibilityChatFocusOptions.None);

        private readonly BehaviorSubject<AccessibilityMessageFocusOptions> accessibilityMessageFocusOptions = new BehaviorSubject<AccessibilityMessageFocusOptions>(AccessibilityMessageFocusOptions.None);

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
        /// Gets or sets a value indicating that scaling factor that should be applied when displaying messages.
        /// </summary>
        public double ScalingFactorForMessages { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets a value indicating whether strict enforcement of MultiChat limits are enabled. If enabled,
        /// groups and chats will be forcibly closed if needed to not exceed the <see cref="MaximumNumberOfMultiChatsNormal"/> or
        /// <see cref="MaximumNumberOfMultiChatsMinibar"/>. This typically applies when opening a Windows 10 Toast Notification,
        /// which defaults to not closing chats since the notification may have been accidently tapped.
        /// </summary>
        public bool StrictlyEnforceMultiChatLimits { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether interacting with system notifications is permitted. Interactions include
        /// shortcuts for liking messages and quickly replying to a group or chat.
        /// </summary>
        public bool EnableNotificationInteractions { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether non-native (simulated) notifications will be used on pre-Windows 10 platforms.
        /// </summary>
        public bool EnableNonNativeNotifications { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether UWP notifications will be quickly expired to avoid outdated notifications backing up.
        /// This is particularly a problem in Windows 10 when messages are sent faster than the default notification duration.
        /// </summary>
        public bool EnableUWPNotificationQuickExpiration { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether UWP notifications should be grouped together by group and chat.
        /// </summary>
        public bool EnableNotificationGrouping { get; set; } = false;

        /// <summary>
        /// Gets or sets the starting window placement for new MiniChat instances.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public WindowParams.Location MiniChatOpenLocation { get; set; } = WindowParams.Location.BottomRight;

        /// <summary>
        /// Gets or sets the starting window position for MiniChats when <see cref="MiniChatOpenLocation"/>
        /// is set to <see cref="WindowParams.Location.Manual"/>.
        /// </summary>
        public double MiniChatManualX { get; set; } = 0;

        /// <summary>
        /// Gets or sets the starting window position for MiniChats when <see cref="MiniChatOpenLocation"/>
        /// is set to <see cref="WindowParams.Location.Manual"/>.
        public double MiniChatManualY { get; set; } = 0;

        /// <summary>
        /// Gets or sets the user selected theme that should be applied to the entire application UI.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ThemeOptions Theme
        {
            get => this.theme.Value;
            set
            {
                this.theme.OnNext(value);
                var themeService = Ioc.Default.GetService<IThemeService>();
                themeService.UpdateTheme(value);
            }
        }

        /// <summary>
        /// Gets or sets the user selected accessibility option that is applied to focused chats.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AccessibilityChatFocusOptions AccessibilityChatFocusOption
        {
            get => this.accessibilityChatFocusOption.Value;
            set
            {
                this.accessibilityChatFocusOption.OnNext(value);
                var themeService = Ioc.Default.GetService<IThemeService>();
                themeService.UpdateTheme(value);
            }
        }

        /// <summary>
        /// Gets or sets the user selected accessibility option that is applied to selected messages.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public AccessibilityMessageFocusOptions AccessibilityMessageFocusOption
        {
            get => this.accessibilityMessageFocusOptions.Value;
            set
            {
                this.accessibilityMessageFocusOptions.OnNext(value);
                var themeService = Ioc.Default.GetService<IThemeService>();
                themeService.UpdateTheme(value);
            }
        }

        /// <summary>
        /// Gets an observable for the currently selected theme.
        /// </summary>
        [JsonIgnore]
        public IObservable<ThemeOptions> CurrentSelectedTheme => this.theme;
    }
}
