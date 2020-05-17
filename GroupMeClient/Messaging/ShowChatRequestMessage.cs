using GalaSoft.MvvmLight.Messaging;
using GroupMeClientApi.Models;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="ShowChatRequestMessage"/> specifies a message commanding the application to
    /// ensure a specific <see cref="Group"/> or <see cref="Chat"/> is visible in the <see cref="GroupMeClient.Views.ChatsView"/>.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class ShowChatRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowChatRequestMessage"/> class.
        /// </summary>
        /// <param name="groupOrChatId">The identifier for the <see cref="Group"/> Or <see cref="Chat"/> that should be displayed.</param>
        public ShowChatRequestMessage(string groupOrChatId)
        {
            this.GroupOrChatId = groupOrChatId;
        }

        /// <summary>
        /// Gets the identifier for the <see cref="Group"/> Or <see cref="Chat"/> that should be displayed.
        /// </summary>
        public string GroupOrChatId { get; }
    }
}
