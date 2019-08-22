using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GroupMeClient.Converters
{
    /// <summary>
    /// <see cref="NullToVisibilityConverter"/> provides a converter to hide null items in XAML.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
