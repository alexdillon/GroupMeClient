using System;
using System.Globalization;
using System.Windows.Data;

namespace GroupMeClient.Extensions
{
    public class IsZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valStr = value.ToString();
            return valStr == "0" || string.IsNullOrEmpty(valStr);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsZeroConverter can only be used OneWay.");
        }
    }
}
