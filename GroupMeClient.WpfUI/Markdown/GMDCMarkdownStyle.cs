using System.ComponentModel;
using System.Windows;

namespace GroupMeClient.WpfUI.Markdown
{
    /// <summary>
    /// Resource keys with GMDC styling for markdown elements.
    /// </summary>
    internal static class GMDCMarkdownStyle
    {
        /// <summary>Gets a resource Key for the DocumentStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey DocumentStyleKey { get; } = CreateResourceKey(nameof(DocumentStyleKey));

        /// <summary>Gets a resource Key for the CodeStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey CodeStyleKey { get; } = CreateResourceKey(nameof(CodeStyleKey));

        /// <summary>Gets a resource Key for the CodeBlockStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey CodeBlockStyleKey { get; } = CreateResourceKey(nameof(CodeBlockStyleKey));

        /// <summary>Gets a resource Key for the Heading1Style.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey Heading1StyleKey { get; } = CreateResourceKey(nameof(Heading1StyleKey));

        /// <summary>Gets a resource Key for the Heading2Style.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey Heading2StyleKey { get; } = CreateResourceKey(nameof(Heading2StyleKey));

        /// <summary>Gets a resource Key for the Heading3Style.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey Heading3StyleKey { get; } = CreateResourceKey(nameof(Heading3StyleKey));

        /// <summary>Gets a resource Key for the Heading4Style.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey Heading4StyleKey { get; } = CreateResourceKey(nameof(Heading4StyleKey));

        /// <summary>Gets a resource Key for the Heading5Style.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey Heading5StyleKey { get; } = CreateResourceKey(nameof(Heading5StyleKey));

        /// <summary>Gets a resource Key for the Heading6Style.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey Heading6StyleKey { get; } = CreateResourceKey(nameof(Heading6StyleKey));

        /// <summary>Gets a resource Key for the ImageStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey ImageStyleKey { get; } = CreateResourceKey(nameof(ImageStyleKey));

        /// <summary>Gets a resource Key for the InsertedStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey InsertedStyleKey { get; } = CreateResourceKey(nameof(InsertedStyleKey));

        /// <summary>Gets a resource Key for the MarkedStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey MarkedStyleKey { get; } = CreateResourceKey(nameof(MarkedStyleKey));

        /// <summary>Gets a resource Key for the QuoteBlockStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey QuoteBlockStyleKey { get; } = CreateResourceKey(nameof(QuoteBlockStyleKey));

        /// <summary>Gets a resource Key for the StrikeThroughStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey StrikeThroughStyleKey { get; } = CreateResourceKey(nameof(StrikeThroughStyleKey));

        /// <summary>Gets a resource Key for the SubscriptStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey SubscriptStyleKey { get; } = CreateResourceKey(nameof(SubscriptStyleKey));

        /// <summary>Gets a resource Key for the SuperscriptStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey SuperscriptStyleKey { get; } = CreateResourceKey(nameof(SuperscriptStyleKey));

        /// <summary>Gets a resource Key for the TableStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey TableStyleKey { get; } = CreateResourceKey(nameof(TableStyleKey));

        /// <summary>Gets a resource Key for the TableCellStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey TableCellStyleKey { get; } = CreateResourceKey(nameof(TableCellStyleKey));

        /// <summary>Gets a resource Key for the TableHeaderStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey TableHeaderStyleKey { get; } = CreateResourceKey(nameof(TableHeaderStyleKey));

        /// <summary>Gets a resource Key for the TaskListStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey TaskListStyleKey { get; } = CreateResourceKey(nameof(TaskListStyleKey));

        /// <summary>Gets a resource Key for the ThematicBreakStyle.</summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey ThematicBreakStyleKey { get; } = CreateResourceKey(nameof(ThematicBreakStyleKey));

        /// <summary>Gets a resource Key for the HyperlinkStyleKey</summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceKey HyperlinkStyleKey { get; } = CreateResourceKey(nameof(HyperlinkStyleKey));

        private static ComponentResourceKey CreateResourceKey(string caller = null)
        {
            return new ComponentResourceKey(typeof(GMDCMarkdownStyle), caller);
        }
    }
}