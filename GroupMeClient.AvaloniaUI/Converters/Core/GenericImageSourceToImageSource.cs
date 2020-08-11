using System;
using Avalonia.Data.Converters;
using GroupMeClient.Core.Controls.Media;

namespace GroupMeClient.AvaloniaUI.Converters
{
    public class GenericImageSourceToImageSource : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is GenericImageSource genericImageSource)
            {
                if (genericImageSource.RenderHeight > 0 || genericImageSource.RenderWidth > 0)
                {
                    return Utilities.ImageUtils.BytesToImageSource(
                        genericImageSource.RawImageData,
                        genericImageSource.RenderWidth,
                        genericImageSource.RenderHeight);
                }
                else
                {
                    return Utilities.ImageUtils.BytesToImageSource(genericImageSource.RawImageData);
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
