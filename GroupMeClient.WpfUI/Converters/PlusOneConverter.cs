using System;
using System.Windows.Data;

namespace GroupMeClient.WpfUI.Converters
{
    /// <summary>
    /// <see cref="PlusOneConverter"/> provides a converter to increment a value by 1.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class PlusOneConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int i)
            {
                return i + 1;
            }
            else if (value is double d)
            {
                return d + 1;
            }
            else if (value is float f)
            {
                return f + 1;
            }
            else
            {
                return 1;
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
