using System;
using System.Windows.Data;

namespace GroupMeClient.WpfUI.Converters
{
    /// <summary>
    /// <see cref="PercentageConverter"/> provides a <see cref="IValueConverter"/> to return a percentage of the provided value.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/717358.
    /// </remarks>
    public class PercentageConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
