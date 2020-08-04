// Adapted from https://web.archive.org/web/20130622171857/http://www.garethevans.com/linking-multiple-value-converters-in-wpf-and-silverlight

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace GroupMeClient.Wpf.Converters
{
    /// <summary>
    /// <see cref="ValueConverterGroup"/> allows grouping a sequence of converters together.
    /// </summary>
    public class ValueConverterGroup : List<IValueConverter>, IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return this.Aggregate(
                value,
                (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}