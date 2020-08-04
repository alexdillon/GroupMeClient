using System;

namespace GroupMeClient.Core.Controls.Documents
{
    /// <summary>
    /// <see cref="Hyperlink"/> represents a clickable link of text.
    /// </summary>
    public class Hyperlink : Span
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Hyperlink"/> class.
        /// </summary>
        /// <param name="inline">The inner displayed contents of this link.</param>
        public Hyperlink(Inline inline)
            : base(inline)
        {
        }

        /// <summary>
        /// Gets or sets the URI this <see cref="Hyperlink"/> navigates to.
        /// </summary>
        public Uri NavigateUri { get; set; }
    }
}
