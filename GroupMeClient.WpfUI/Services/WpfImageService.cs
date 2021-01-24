using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GroupMeClient.Core.Services;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfImageService"/> provides image generators using Wpf Imaging APIs.
    /// </summary>
    public class WpfImageService : IImageService
    {
        /// <inheritdoc/>
        public byte[] CreateTransparentPng(int width, int height)
        {
            var tinyImg = BitmapSource.Create(
                pixelWidth: 1,
                pixelHeight: 1,
                96,
                96,
                PixelFormats.Bgr24,
                new BitmapPalette(new List<Color> { Colors.Transparent }),
                new byte[] { 0, 0, 0 },
                3);

            var scaledImg = new TransformedBitmap(tinyImg, new ScaleTransform(width, height));

            return Utilities.ImageUtils.BitmapSourceToBytes(scaledImg);
        }
    }
}
