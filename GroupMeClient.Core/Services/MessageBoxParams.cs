namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="MessageBoxParams"/> defines the appearance and content of a Message Box.
    /// </summary>
    public class MessageBoxParams
    {
        /// <summary>
        /// The buttons that should be shown.
        /// </summary>
        public enum Buttons
        {
            /// <summary>
            /// The Okay button
            /// </summary>
            Ok,
        }

        /// <summary>
        /// The icons that should be shown.
        /// </summary>
        public enum Icon
        {
            /// <summary>
            /// No icon.
            /// </summary>
            None,

            /// <summary>
            /// The success icon.
            /// </summary>
            Success,

            /// <summary>
            /// The error icon.
            /// </summary>
            Error,

            /// <summary>
            /// The warning icon.
            /// </summary>
            Warning,
        }

        /// <summary>
        /// Gets or sets the title or caption for the message box.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message contents.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the buttons to include in the dialog.
        /// </summary>
        public Buttons MessageBoxButtons { get; set; }

        /// <summary>
        /// Gets or sets the icon to show in the dialog.
        /// </summary>
        public Icon MessageBoxIcons { get; set; }
    }
}
