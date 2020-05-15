using GalaSoft.MvvmLight.Messaging;
using GroupMeClientApi.Models;
using GroupMeClientPlugin.GroupChat;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="RunPluginRequestMessage"/> specifies a message commanding the application
    /// execute a plugin for a specific <see cref="Group"/> or <see cref="Chat"/> with support
    /// for providing cache access to the <see cref="IGroupChatPlugin"/>.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class RunPluginRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunPluginRequestMessage"/> class.
        /// </summary>
        /// <param name="messageContainer">The Group or Chat to execute the plugin with.</param>
        /// <param name="plugin">The plugin to execute upon index completion.</param>
        public RunPluginRequestMessage(IMessageContainer messageContainer, IGroupChatPlugin plugin)
        {
            this.MessageContainer = messageContainer;
            this.Plugin = plugin;
        }

        /// <summary>
        /// Gets the Group Or Chat that should be passed as a parameter to the executed plugin.
        /// </summary>
        public IMessageContainer MessageContainer { get; }

        /// <summary>
        /// Gets the plugin that should be executed upon index completion.
        /// </summary>
        public IGroupChatPlugin Plugin { get; }
    }
}
