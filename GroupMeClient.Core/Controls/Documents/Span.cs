using System.Collections.Generic;

namespace GroupMeClient.Core.Controls.Documents
{
    /// <summary>
    /// <see cref="Span"/> represents a segment of formatted text.
    /// </summary>
    public class Span : Inline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Span"/> class.
        /// </summary>
        /// <param name="inlines">A collection of <see cref="Inline"/>s to include in this span.</param>
        public Span(ICollection<Inline> inlines)
        {
            this.Children = inlines;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Span"/> class.
        /// </summary>
        /// <param name="inline">A single <see cref="Inline"/> to be contained within this <see cref="Span"/>.</param>
        public Span(Inline inline)
        {
            this.Children = new List<Inline>() { inline };
        }

        /// <summary>
        /// <see cref="FontWeightOptions"/> define different weights for which a <see cref="Span"/>'s text is rendered.
        /// </summary>
        public enum FontWeightOptions
        {
            /// <summary>
            /// Bold
            /// </summary>
            Bold,

            /// <summary>
            /// Semibold
            /// </summary>
            SemiBold,

            /// <summary>
            /// Regular weight
            /// </summary>
            Regular,
        }

        /// <summary>
        /// Gets a collection of <see cref="Inline"/>s contained within this <see cref="Span"/>.
        /// </summary>
        public ICollection<Inline> Children { get; }

        /// <summary>
        /// Gets or sets the font weight at which this <see cref="Span"/> should be rendered.
        /// </summary>
        public FontWeightOptions FontWeight { get; set; } = FontWeightOptions.Regular;
    }
}
