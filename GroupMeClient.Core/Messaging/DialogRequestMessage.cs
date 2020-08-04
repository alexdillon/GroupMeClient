using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

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
        public DialogRequestMessage(ViewModelBase dialog, bool topMost = false)
        {
            this.Dialog = dialog;
            this.TopMost = topMost;
        }

        /// <summary>
        /// Gets the dialog to be displayed.
        /// </summary>
        public ViewModelBase Dialog { get; }

        /// <summary>
        /// Gets a value indicating whether this dialog should be displayed on top of any first-level dialogs.
        /// </summary>
        public bool TopMost { get; }
    }
}
