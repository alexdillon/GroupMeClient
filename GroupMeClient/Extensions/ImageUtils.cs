using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GroupMeClient.Extensions
{
    class ImageUtils
    {
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

        public static ImageSource BytesToImageSource(byte[] image)
        {
            using (var ms = new MemoryStream(image))
            {
                return BitmapFrame.Create(
                        ms,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.OnLoad);

                //var bitmapImage = new BitmapImage();
                //bitmapImage.BeginInit();
                //bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                //bitmapImage.StreamSource = ms;
                //bitmapImage.EndInit();

                //return bitmapImage;
            }
        }
    }
}
