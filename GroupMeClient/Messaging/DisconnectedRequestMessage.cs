using GalaSoft.MvvmLight.Messaging;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="DisconnectedRequestMessage"/> specifies a message indicating that a component
    /// of the GroupMe Client has either disconnected or resumed connectivity.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class DisconnectedRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedRequestMessage"/> class.
        /// </summary>
        /// <param name="disconnected">
        /// A boolean indicating if a component has disconnected.
        /// See <see cref="Disconnected"/>.
        /// </param>
        public DisconnectedRequestMessage(bool disconnected)
        {
            this.Disconnected = disconnected;
        }

        /// <summary>
        /// Gets a value indicating whether a component has disconnected.
        /// True indicates a disconnected component. False indicates that a component has successfully reconnected.
        /// </summary>
        public bool Disconnected { get; }
    }
}
