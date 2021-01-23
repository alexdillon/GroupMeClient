using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using GroupMeClient.Core.Services;
using SkiaSharp;

namespace GroupMeClient.AvaloniaUI.Services
{
    /// <summary>
    /// <see cref="AvaloniaImageService"/> provides image generators using Wpf Imaging APIs.
    /// </summary>
    public class AvaloniaImageService : IImageService
    {
        /// <inheritdoc/>
        public byte[] CreateTransparentPng(int width, int height)
        {

            using (var bitmap = new SKBitmap(width, height))
            {
                bitmap.Erase(SKColors.Transparent);

                using (var ms = new MemoryStream())
                {
                    var data = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
                    data.SaveTo(ms);

                    return ms.ToArray();
                }
            }
        }
    }
}
