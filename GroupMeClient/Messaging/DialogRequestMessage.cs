using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="DialogRequestMessage"/> specifies a message to open a new Window-level dialog.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class DialogRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogRequestMessage"/> class.
        /// </summary>
        /// <param name="dialog">The dialog to be displayed.</param>
        public DialogRequestMessage(ViewModelBase dialog)
        {
            this.Dialog = dialog;
        }

        /// <summary>
        /// Gets the dialog to be displayed.
        /// </summary>
        public ViewModelBase Dialog { get; }
    }
}
