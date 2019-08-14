using GalaSoft.MvvmLight.Messaging;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="UnreadRequestMessage"/> specifies a message to update the application-level number of notifications to display.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class UnreadRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnreadRequestMessage"/> class.
        /// </summary>
        /// <param name="count">The unread message/notification count to display.</param>
        public UnreadRequestMessage(int count)
        {
            this.Count = count;
        }

        /// <summary>
        /// Gets the unread message/notification count to display.
        /// </summary>
        public int Count { get; }
    }
}
