using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.Messaging
{
    /// <summary>
    /// <see cref="ShowChatRequestMessage"/> specifies a message commanding the application to
    /// ensure a specific <see cref="Group"/> or <see cref="Chat"/> is visible in the <see cref="GroupMeClient.Views.ChatsView"/>.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    public class ShowChatRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowChatRequestMessage"/> class.
        /// </summary>
        /// <param name="groupOrChatId">The identifier for the <see cref="Group"/> Or <see cref="Chat"/> that should be displayed.</param>
        /// <param name="startReply">The message to begin a reply to. If <c>null</c> or empty, no reply will be started.</param>
        public ShowChatRequestMessage(string groupOrChatId, MessageControlViewModel startReply = null)
        {
            this.GroupOrChatId = groupOrChatId;
            this.StartReply = startReply;
        }

        /// <summary>
        /// Gets the identifier for the <see cref="Group"/> Or <see cref="Chat"/> that should be displayed.
        /// </summary>
        public string GroupOrChatId { get; }

        /// <summary>
        /// Gets the message to begin a reply to. If <c>null</c>, no reply is being started.
        /// </summary>
        public MessageControlViewModel StartReply { get; }
    }
}
