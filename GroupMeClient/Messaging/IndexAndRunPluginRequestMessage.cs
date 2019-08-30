using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClientApi.Models;
using GroupMeClientPlugin.GroupChat;

namespace GroupMeClient.Messaging
{
    /// <summary>
    /// <see cref="IndexAndRunPluginRequestMessage"/> specifies a message commanding the application to switch to the Search Tab,
    /// begin indexing, and then execute a plugin.
    /// This request can be sent through <see cref="Messenger"/>.
    /// </summary>
    internal class IndexAndRunPluginRequestMessage : MessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexAndRunPluginRequestMessage"/> class.
        /// </summary>
        /// <param name="messageContainer">The Group or Chat to execute the plugin with.</param>
        /// <param name="plugin">The plugin to execute upon index completion.</param>
        public IndexAndRunPluginRequestMessage(IMessageContainer messageContainer, IGroupChatCachePlugin plugin)
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
        public IGroupChatCachePlugin Plugin { get; }
    }
}
