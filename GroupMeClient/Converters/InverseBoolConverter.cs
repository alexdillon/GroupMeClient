using System;
using System.Windows.Data;

namespace GroupMeClient.Wpf.Converters
{
    /// <summary>
    /// <see cref="InverseBoolConverter"/> provides a converter for boolean logical NOT.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
