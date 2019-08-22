using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GroupMeClient.Converters
{
    /// <summary>
    /// <see cref="BoolToVisibilityConverter"/> provides a converter to hide false items in XAML.
    /// </summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = System.Convert.ToBoolean(value, culture);
            return visible == false ? Visibility.Hidden : Visibility.Visible;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
