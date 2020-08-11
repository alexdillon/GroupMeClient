using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="IsZeroConverter"/> provides a converter between Zero, Null, and Empty Strings (true), and any other value (false).
    /// </summary>
    public class IsZeroConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return true;
            }

            var valStr = value.ToString();
            return valStr == "0" || string.IsNullOrEmpty(valStr);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsZeroConverter can only be used OneWay.");
        }
    }
}
