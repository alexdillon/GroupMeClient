using System;

namespace GroupMeClient.Core.Settings.Themes
{
    /// <summary>
    /// Accessibility settings that can be applied to add visual focus to the currently selected chat
    /// in the GMDC UI.
    /// </summary>
    public enum AccessibilityChatFocusOptions
    {
        /// <summary>
        /// No added focus to the active chat.
        /// </summary>
        None,

        /// <summary>
        /// An indicator bar will be applied to the active chat.
        /// </summary>
        Bar,

        /// <summary>
        /// A full border will be applied to the active chat.
        /// </summary>
        Border,
    }
}
