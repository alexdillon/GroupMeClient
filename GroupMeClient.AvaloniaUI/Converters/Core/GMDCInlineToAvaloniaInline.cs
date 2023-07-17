using System;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Data.Converters;
using GroupMeClient.Core.Controls.Documents;
using Microsoft.EntityFrameworkCore.Update;

namespace GroupMeClient.AvaloniaUI.Converters
{
    /// <summary>
    /// <see cref="GMDCInlineToAvaloniaInline"/> provides conversions between GMDC Core abstract text types and Avalonia inlines.
    /// </summary>
    public class GMDCInlineToAvaloniaInline : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new Avalonia.Controls.Documents.InlineCollection();

            if (value is ObservableCollection<Inline> gmdcInlines)
            {
                foreach (var inline in gmdcInlines)
                {
                    if (inline is Run r)
                    {
                        result.Add(new Avalonia.Controls.Documents.Run(r.Text));
                    }
                    else if (inline is Span s)
                    {
                        var avaloniaSpan = new Avalonia.Controls.Documents.Span()
                        {
                            FontWeight = this.GMDCFontWeightToAvalonia(s.FontWeight),
                        };

                        foreach (var child in s.Children)
                        {
                            if (child is Run r2)
                            {
                                avaloniaSpan.Inlines.Add(new Avalonia.Controls.Documents.Run(r2.Text));
                            }
                        }

                        result.Add(avaloniaSpan);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private Avalonia.Media.FontWeight GMDCFontWeightToAvalonia(Span.FontWeightOptions fontWeightOptions)
        {
            return fontWeightOptions switch
            {
                Span.FontWeightOptions.Regular => Avalonia.Media.FontWeight.Regular,
                Span.FontWeightOptions.SemiBold => Avalonia.Media.FontWeight.SemiBold,
                Span.FontWeightOptions.Bold => Avalonia.Media.FontWeight.Bold,
                _ => Avalonia.Media.FontWeight.Regular,
            };
        }
    }
}
