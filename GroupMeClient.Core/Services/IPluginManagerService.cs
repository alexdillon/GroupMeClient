using System.Collections.Generic;
using GroupMeClientPlugin.GroupChat;
using GroupMeClientPlugin.MessageCompose;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IPluginManagerService"/> provides a generic service for accessing plugins.
    public interface IPluginManagerService
    {
        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins.
        /// </summary>
        IEnumerable<IGroupChatPlugin> GroupChatPlugins { get; }

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are built-in.
        /// </summary>
        ICollection<IGroupChatPlugin> GroupChatPluginsBuiltIn { get; }

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are manually installed.
        /// </summary>
        ICollection<IGroupChatPlugin> GroupChatPluginsManuallyInstalled { get; }

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are automatically installed from a repo.
        /// </summary>
        ICollection<IGroupChatPlugin> GroupChatPluginsAutoInstalled { get; }

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        IEnumerable<IMessageComposePlugin> MessageComposePlugins { get; }

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        ICollection<IMessageComposePlugin> MessageComposePluginsBuiltIn { get; }

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        ICollection<IMessageComposePlugin> MessageComposePluginsManuallyInstalled { get; }

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        ICollection<IMessageComposePlugin> MessageComposePluginsAutoInstalled { get; }

        /// <summary>
        /// Loads and registers all available plugins.
        /// </summary>
        /// <param name="pluginsPath">The folder to load plugins from.</param>
        void LoadPlugins(string pluginsPath);
    }
}
