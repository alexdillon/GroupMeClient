using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GroupMeClient.Wpf.Converters
{
    /// <summary>
    /// <see cref="NullToVisibilityConverter"/> provides a converter to hide null items in XAML.
    /// </summary>
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collapseOrHide = Visibility.Hidden;
            if (parameter is bool collapseParam && collapseParam)
            {
                collapseOrHide = Visibility.Collapsed;
            }

            return value == null ? collapseOrHide : Visibility.Visible;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
