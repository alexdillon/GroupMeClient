using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GroupMeClient.Extensions
{
    /// <summary>
    /// Helper class to provide common Wpf image operations.
    /// </summary>
    public class ImageUtils
    {
        /// <summary>
        /// Converts a <see cref="BitmapSource"/> to encoded raw image bytes.
        /// </summary>
        /// <param name="image">The source image.</param>
        /// <returns>A png encoded array of raw bytes.</returns>
        public static byte[] BitmapSourceToBytes(BitmapSource image)
        {
            var encoder = new PngBitmapEncoder();
            var frame = BitmapFrame.Create(image);
            encoder.Frames.Add(frame);
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts raw image data into an <see cref="ImageSource"/>.
        /// </summary>
        /// <param name="image">The raw image data.</param>
        /// <returns>A Wpf <see cref="ImageSource"/>.</returns>
        public static ImageSource BytesToImageSource(byte[] image)
        {
            using (var ms = new MemoryStream(image))
            {
                return BitmapFrame.Create(
                        ms,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad);

                /*var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                return bitmapImage;*/
            }
        }
    }
}
