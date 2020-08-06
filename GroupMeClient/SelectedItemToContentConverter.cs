using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GroupMeClient.WpfUI
{
    /// <summary>
    /// <see cref="SelectedItemToContentConverter"/> provides a converter between Hamburger Menu entries and ViewModels.
    /// </summary>
    public class SelectedItemToContentConverter : IMultiValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // first value is selected menu item, second value is selected option item
            if (values != null && values.Length > 1)
            {
                return values[0] ?? values[1];
            }

            return null;
        }

        /// <inheritdoc/>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return targetTypes.Select(t => Binding.DoNothing).ToArray();
        }
    }
}