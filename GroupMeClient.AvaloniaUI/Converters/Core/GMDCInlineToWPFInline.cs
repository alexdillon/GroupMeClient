using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Data.Converters;
using GroupMeClient.Core.Controls.Documents;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="GMDCInlineToString"/> provides conversions between GMDC Core abstract text types and plain text.
    /// </summary>
    public class GMDCInlineToWPFInline : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new StringBuilder();

            if (value is ObservableCollection<Inline> inlines)
            {
                foreach (var inline in inlines)
                {
                    if (inline is Run r)
                    {
                        result.Append(r.Text);
                    }
                    else if (inline is Span s)
                    {
                        foreach (var child in s.Children)
                        {
                            if (child is Run r2)
                            {
                                result.Append(r2.Text);
                            }
                        }
                    }
                }
            }

            return result.ToString();
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
