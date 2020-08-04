namespace GroupMeClient.Core.Controls.Documents
{
    /// <summary>
    /// <see cref="Run"/> represents an unformatted section of text.
    /// </summary>
    public class Run : Inline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Run"/> class.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public Run(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Gets or sets the text displayed in this <see cref="Run"/>.
        /// </summary>
        public string Text { get; set; }
    }
}
