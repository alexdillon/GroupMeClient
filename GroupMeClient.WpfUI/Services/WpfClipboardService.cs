using System.IO;
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
            var ms = new MemoryStream();
            ms.Write(imageSource.RawImageData, 0, imageSource.RawImageData.Length);

            var imageClipObject = new System.Windows.DataObject();
            imageClipObject.SetData("PNG", ms);
            imageClipObject.SetData(ms);
            imageClipObject.SetImage(Utilities.ImageUtils.BytesToImageSource(imageSource.RawImageData) as BitmapSource);

            System.Windows.Clipboard.SetDataObject(imageClipObject, true);

            ms.Dispose();
        }

        /// <inheritdoc/>
        public void CopyText(string text)
        {
            System.Windows.Clipboard.SetText(text);
        }
    }
}
