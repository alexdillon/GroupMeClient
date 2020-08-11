using System;
using Avalonia.Data.Converters;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="InverseBoolConverter"/> provides a converter for boolean logical NOT.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                // Treat null as false.
                value = false;
            }

            return !(bool)value;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
