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
    }
}
