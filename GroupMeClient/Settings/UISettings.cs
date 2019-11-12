using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Settings
{
    /// <summary>
    /// <see cref="UISettings"/> defines the settings for customizing the user interface.
    /// </summary>
    public class UISettings
    {
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
    }
}
