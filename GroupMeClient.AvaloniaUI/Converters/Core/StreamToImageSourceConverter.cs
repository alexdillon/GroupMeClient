using System;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using GroupMeClient.Core.Controls.Media;

namespace GroupMeClient.AvaloniaUI.Converters
{
    public class StreamToImageSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MemoryStream ms)
            {
                return this.MemoryStreamToImage(ms);
            }
            else if (value is Stream s)
            {
                var memStream = new MemoryStream();
                s.Seek(0, SeekOrigin.Begin);
                s.CopyTo(memStream);
                return this.MemoryStreamToImage(memStream);
            }

            return null;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private IBitmap MemoryStreamToImage(MemoryStream ms)
        {
            var bytes = ms.ToArray();
            return Utilities.ImageUtils.BytesToImageSource(bytes);
        }
    }
}
