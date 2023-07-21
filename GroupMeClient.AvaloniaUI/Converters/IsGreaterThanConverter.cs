using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="IsGreaterThanConverter"/> provides a converter indicating whether a value is greater than another specified value.
    /// </summary>
    public class IsGreaterThanConverter : IValueConverter
    {
        /// <summary>
        /// A static instance of the <see cref="IsGreaterThanConverter"/> converter.
        /// </summary>
        public static readonly IsGreaterThanConverter Instance = new IsGreaterThanConverter();

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
            throw new InvalidOperationException($"{nameof(IsGreaterThanConverter)} can only be used OneWay.");
        }
    }
}
