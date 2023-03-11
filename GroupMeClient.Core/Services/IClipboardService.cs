using System;
using GroupMeClient.Core.Controls.Media;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// Interface for a cross-platform service for accessing data on the Clipboard.
    /// </summary>
    public interface IClipboardService
    {
        /// <summary>
        /// Copies an image to the clipboard.
        /// </summary>
        /// <param name="imageSource">The image data to copy.</param>
        void CopyImage(GenericImageSource imageSource);

        /// <summary>
        /// Copies plain text to the clipboard.
        /// </summary>
        /// <param name="text">The text to copy.</param>
        void CopyText(string text);
    }
}
