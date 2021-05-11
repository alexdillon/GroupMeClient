using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using GroupMeClient.Core.Services;
using GroupMeClient.WpfUI.Extensions;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.WpfUI.Converters
{
    /// <summary>
    /// <see cref="GMDCInlineToWPFInline"/> provides conversions between GMDC Core abstract text types,
    /// and WPF TextElements defined in <see cref="System.Windows.Documents"/>.
    /// </summary>
    [ValueConversion(typeof(ObservableCollection<Core.Controls.Documents.Inline>), typeof(ObservableCollection<System.Windows.Documents.Inline>))]
    public class GMDCInlineToWPFInline : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var results = new ObservableCollection<System.Windows.Documents.Inline>();

            if (value is ObservableCollection<Core.Controls.Documents.Inline> inlines)
            {
                foreach (var inline in inlines)
                {
                    results.Add(this.GMDCInlineToWpfInline(inline));
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private System.Windows.Documents.Inline GMDCInlineToWpfInline(Core.Controls.Documents.Inline value)
        {
            if (value is Core.Controls.Documents.Run run)
            {
                return this.GMDCRunToWpfRun(run);
            }
            else if (value is Core.Controls.Documents.Hyperlink hyperlink)
            {
                // Hyperlinks are a special type of Span, so check them first
                return this.GMDCHyperlinkToWpfHyperlink(hyperlink);
            }
            else if (value is Core.Controls.Documents.Span span)
            {
                // Check plain Span last
                return this.GMDCSpanToWpfSpan(span);
            }

            return null;
        }

        private System.Windows.Documents.Run GMDCRunToWpfRun(Core.Controls.Documents.Run run)
        {
            return new System.Windows.Documents.Run(run.Text);
        }

        private System.Windows.Documents.Span GMDCSpanToWpfSpan(Core.Controls.Documents.Span span)
        {
            var result = new System.Windows.Documents.Span();
            foreach (var child in span.Children)
            {
                result.Inlines.Add(this.GMDCInlineToWpfInline(child));
            }

            result.FontWeight = this.GMDCFontWeightToWpfFontWeight(span.FontWeight);

            return result;
        }

        private System.Windows.Documents.Hyperlink GMDCHyperlinkToWpfHyperlink(Core.Controls.Documents.Hyperlink hyperlink)
        {
            var span = this.GMDCSpanToWpfSpan(hyperlink);
            var result = new CopyableHyperlink(span)
            {
                NavigateUri = hyperlink.NavigateUri,
            };

            result.RequestNavigate += (object sender, System.Windows.Navigation.RequestNavigateEventArgs e) =>
            {
                var osService = Ioc.Default.GetService<IOperatingSystemUIService>();
                osService.OpenWebBrowser(hyperlink.NavigateUri.ToString());
            };

            return result;
        }

        private System.Windows.FontWeight GMDCFontWeightToWpfFontWeight(Core.Controls.Documents.Span.FontWeightOptions fontWeight)
        {
            switch (fontWeight)
            {
                case Core.Controls.Documents.Span.FontWeightOptions.Bold:
                    return System.Windows.FontWeights.Bold;

                case Core.Controls.Documents.Span.FontWeightOptions.SemiBold:
                    return System.Windows.FontWeights.SemiBold;

                case Core.Controls.Documents.Span.FontWeightOptions.Regular:
                default:
                    return System.Windows.FontWeights.Regular;
            }
        }
    }
}
