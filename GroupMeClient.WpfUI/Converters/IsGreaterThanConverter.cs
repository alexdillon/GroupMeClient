using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace GroupMeClient.WpfUI.Converters
{
    /// <summary>
    /// <see cref="IsGreaterThanConverter"/> provides a converter indicating whether are value is greater than a specified value.
    /// </summary>
    public class IsGreaterThanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterCorrect = parameter;

            if (parameter.GetType() != value.GetType())
            {
                parameterCorrect = System.Convert.ChangeType(parameter, value.GetType());
            }

            var comparer = new Comparer(culture);
            return comparer.Compare(value, parameterCorrect) > 0;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsGreaterThanConverter can only be used OneWay.");
        }
    }
}
