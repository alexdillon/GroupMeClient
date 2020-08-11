using GalaSoft.MvvmLight.Messaging;
using GroupMeClientApi.Models;
using GroupMeClientPlugin.GroupChat;

namespace GroupMeClient.Core.Messaging
{
    /// <summary>
    /// <see cref="SwitchToPageRequestMessage"/> specifies a message commanding the application
    /// execute a plugin for a specific <see cref="Group"/> or <see cref="Chat"/> with support
    /// for providing cache access to the <see cref="IGroupChatPlugin"/>.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    public class SwitchToPageRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchToPageRequestMessage"/> class.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> to switch to.</param>
        public SwitchToPageRequestMessage(Page page)
        {
            this.SelectedPage = page;
        }

        /// <summary>
        /// <see cref="Page"/> defines different top-level tabs that can be displayed in GroupMe Desktop Client.
        /// </summary>
        public enum Page
        {
            /// <summary>
            /// The Chats Page
            /// </summary>
            Chats,

            /// <summary>
            /// The Search Page
            /// </summary>
            Search,

            /// <summary>
            /// The Settings Page
            /// </summary>
            Settings,
        }

        /// <summary>
        /// Gets the <see cref="Page"/> the application should switch to.
        /// </summary>
        public Page SelectedPage { get; }
    }
}
