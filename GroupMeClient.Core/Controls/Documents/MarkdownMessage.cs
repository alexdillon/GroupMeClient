using System;

namespace GroupMeClient.Core.Controls.Documents
{
    /// <summary>
    /// A section of text that should be processed as Markdown.
    /// </summary>
    public class MarkdownMessage : Inline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownMessage"/> class.
        /// </summary>
        /// <param name="content">The markdown contents.</param>
        public MarkdownMessage(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Gets the markdown formatted contents.
        /// </summary>
        public string Content { get; }
    }
}
