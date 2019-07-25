using System.Threading.Tasks;

namespace GroupMeClientPlugin.MessageCompose
{
    /// <summary>
    /// <see cref="IMessageComposePlugin"/> defines a plugin that can provide Message Composition Effects in the
    /// GroupMe Desktop Client. Message Composition Effects can include suggesting images or reformatting text.
    /// </summary>
    public interface IMessageComposePlugin
    {
        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        string EffectPluginName { get; }

        /// <summary>
        /// Generates possible message effects off of the existing text a user has composed.
        /// </summary>
        /// <param name="typedMessage">The original message composed by the user.</param>
        /// <returns>A listing of possible text and image effects.</returns>
        Task<MessageSuggestions> ProvideOptions(string typedMessage);
    }
}
