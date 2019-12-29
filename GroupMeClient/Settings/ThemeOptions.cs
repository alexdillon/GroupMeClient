using System.ComponentModel;

namespace GroupMeClient.Settings
{
    /// <summary>
    /// <see cref="ThemeOptions"/> specifies the different theme options that can be applied to the UI.
    /// </summary>
    public enum ThemeOptions
    {
        /// <summary>
        /// Use the light color theme (white background).
        /// </summary>
        [Description("Light Theme")]
        Light,

        /// <summary>
        /// Use the dark color theme (black background).
        /// </summary>
        [Description("Dark Theme")]
        Dark,
    }
}
