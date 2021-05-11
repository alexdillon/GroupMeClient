using System;

namespace GroupMeClient.Core.Messaging
{
    /// <summary>
    /// <see cref="DialogDismissMessage"/> specifies a message to close an existing Window-level dialog.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    public class DialogDismissMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogDismissMessage"/> class.
        /// </summary>
        /// <param name="dialogId">The identifier of the dialog to dismiss.</param>
        public DialogDismissMessage(Guid dialogId)
        {
            this.DialogId = dialogId;
        }

        /// <summary>
        /// Gets a GUID that uniquely identifies the dialog being dismissed.
        /// </summary>
        public Guid DialogId { get; }
    }
}
