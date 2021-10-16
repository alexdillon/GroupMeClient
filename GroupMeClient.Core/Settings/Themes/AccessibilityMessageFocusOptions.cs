using System;

namespace GroupMeClient.Core.Settings.Themes
{
    /// <summary>
    /// Accessibility settings that can be applied to add visual focus to the currently selected message
    /// in the GMDC UI.
    /// </summary>
    public enum AccessibilityMessageFocusOptions
    {
        /// <summary>
        /// No added focus to the selected message.
        /// </summary>
        None,

        /// <summary>
        /// A full border will be applied to the selected message.
        /// </summary>
        Border,
    }
}
