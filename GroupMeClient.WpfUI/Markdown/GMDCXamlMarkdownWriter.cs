using System;
using System.Xaml;
using Neo.Markdig.Xaml;
using Neo.Markdig.Xaml.Renderers;

namespace GroupMeClient.WpfUI.Markdown
{
    /// <summary>
    /// An implementation of <see cref="XamlMarkdownWriter"/> that uses GMDC styling.
    /// </summary>
    internal class GMDCXamlMarkdownWriter : XamlMarkdownWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GMDCXamlMarkdownWriter"/> class.
        /// </summary>
        /// <param name="writer">The XAML writer to emit the rendered document to.</param>
        public GMDCXamlMarkdownWriter(XamlWriter writer)
            : base(writer)
        {
        }

        /// <inheritdoc/>
        public override object GetDefaultStyle(MarkdownXamlStyle style)
        {
            switch (style)
            {
                case MarkdownXamlStyle.Document:
                    return GMDCMarkdownStyle.DocumentStyleKey;
                case MarkdownXamlStyle.Code:
                    return GMDCMarkdownStyle.CodeStyleKey;
                case MarkdownXamlStyle.CodeBlock:
                    return GMDCMarkdownStyle.CodeBlockStyleKey;
                case MarkdownXamlStyle.Heading1:
                    return GMDCMarkdownStyle.Heading1StyleKey;
                case MarkdownXamlStyle.Heading2:
                    return GMDCMarkdownStyle.Heading2StyleKey;
                case MarkdownXamlStyle.Heading3:
                    return GMDCMarkdownStyle.Heading3StyleKey;
                case MarkdownXamlStyle.Heading4:
                    return GMDCMarkdownStyle.Heading4StyleKey;
                case MarkdownXamlStyle.Heading5:
                    return GMDCMarkdownStyle.Heading5StyleKey;
                case MarkdownXamlStyle.Heading6:
                    return GMDCMarkdownStyle.Heading6StyleKey;
                case MarkdownXamlStyle.Image:
                    return GMDCMarkdownStyle.ImageStyleKey;
                case MarkdownXamlStyle.Inserted:
                    return GMDCMarkdownStyle.InsertedStyleKey;
                case MarkdownXamlStyle.Marked:
                    return GMDCMarkdownStyle.MarkedStyleKey;
                case MarkdownXamlStyle.QuoteBlock:
                    return GMDCMarkdownStyle.QuoteBlockStyleKey;
                case MarkdownXamlStyle.StrikeThrough:
                    return GMDCMarkdownStyle.StrikeThroughStyleKey;
                case MarkdownXamlStyle.Subscript:
                    return GMDCMarkdownStyle.SubscriptStyleKey;
                case MarkdownXamlStyle.Superscript:
                    return GMDCMarkdownStyle.SuperscriptStyleKey;
                case MarkdownXamlStyle.Table:
                    return GMDCMarkdownStyle.TableStyleKey;
                case MarkdownXamlStyle.TableCell:
                    return GMDCMarkdownStyle.TableCellStyleKey;
                case MarkdownXamlStyle.TableHeader:
                    return GMDCMarkdownStyle.TableHeaderStyleKey;
                case MarkdownXamlStyle.TaskList:
                    return GMDCMarkdownStyle.TaskListStyleKey;
                case MarkdownXamlStyle.ThematicBreak:
                    return GMDCMarkdownStyle.ThematicBreakStyleKey;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style));
            }
        }
    }
}
