using GalaSoft.MvvmLight.Messaging;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="RebootRequestMessage"/> specifies a message indicating that a component
    /// of the GroupMe Client has requested that the application be rebooted.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class RebootRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RebootRequestMessage"/> class.
        /// </summary>
        /// <param name="reason">The reason for the reboot request.</param>
        public RebootRequestMessage(string reason)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Gets a value indicating the reason for the reboot request.
        /// </summary>
        public string Reason { get; }
    }
}
