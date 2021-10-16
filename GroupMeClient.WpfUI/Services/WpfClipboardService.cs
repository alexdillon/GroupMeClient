using System.Diagnostics;
using System.Windows.Media.Imaging;
using GroupMeClient.Core.Controls.Media;
using GroupMeClient.Core.Services;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfClipboardService"/> implements clipboard services on the Wpf/Windows platform.
    /// </summary>
    public class WpfClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public void CopyImage(GenericImageSource imageSource)
        {
            try
            {
                this.CopyImageInternal(imageSource, copyData: true);
            }
            catch (System.Exception)
            {
                // If the clipboard is locked, clipboard access with copy will sometimes fail
                // Try using SetDataObject with the "copy" parameter set to false will
                // work instead (but won't persist data copied after GMDC closes).
                try
                {
                    this.CopyImageInternal(imageSource, copyData: false);
                }
                catch (System.Exception)
                {
                    Debug.WriteLine("Failed to set clipboard image data");
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public void CopyText(string text)
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
            }
            catch (System.Exception)
            {
                // If the clipboard is locked, regular SetText will sometimes fail
                // Try using SetDataObject with the "copy" parameter set to false will
                // work instead (but won't persist data copied after GMDC closes).
                try
                {
                    System.Windows.Clipboard.SetDataObject(text, false);
                }
                catch (System.Exception)
                {
                    Debug.WriteLine("Failed to set clipboard data");
                    return;
                }
            }
        }

        private void CopyImageInternal(GenericImageSource imageSource, bool copyData)
        {
            var imageClipObject = new System.Windows.DataObject();
            imageClipObject.SetData("PNG", imageSource.RawImageData);
            imageClipObject.SetData(imageSource.RawImageData);
            imageClipObject.SetImage(Utilities.ImageUtils.BytesToImageSource(imageSource.RawImageData) as BitmapSource);

            System.Windows.Clipboard.SetDataObject(imageClipObject, copyData);
        }
    }
}
