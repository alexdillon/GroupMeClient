using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.Core.Messaging
{
    /// <summary>
    /// <see cref="DialogRequestMessage"/> specifies a message to open a new Window-level dialog.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    public class DialogRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogRequestMessage"/> class.
        /// </summary>
        /// <param name="dialog">The dialog to be displayed.</param>
        /// <param name="topMost">A value indicating whether this dialog should be displayed on top of any first-level dialogs.</param>
        /// <param name="destination">A optional tag value indicating which dialog manager(s) should respond to this request.</param>
        public DialogRequestMessage(ObservableObject dialog, bool topMost = false, string destination = "")
        {
            this.Dialog = dialog;
            this.TopMost = topMost;
            this.Destination = destination;

            this.DialogId = Guid.NewGuid();
        }

        /// <summary>
        /// Gets the dialog to be displayed.
        /// </summary>
        public ObservableObject Dialog { get; }

        /// <summary>
        /// Gets a value indicating whether this dialog should be displayed on top of any first-level dialogs.
        /// </summary>
        public bool TopMost { get; }

        /// <summary>
        /// Gets a string identifying which dialog manager(s) should respond to this popup request.
        /// </summary>
        public string Destination { get; }

        /// <summary>
        /// Gets a GUID that uniquely identifies this dialog.
        /// </summary>
        public Guid DialogId { get; }
    }
}
