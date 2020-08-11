using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GroupMeClient.Core.Plugins;
using GroupMeClient.Core.Services;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using GroupMeClientPlugin.MessageCompose;

namespace GroupMeClient.AvaloniaUI.Plugins
{
    /// <summary>
    /// <see cref="PluginManager"/> provides functionality to dynamically load <see cref="IPluginBase"/>-based
    /// plugins and register them to extend Client functionality.
    /// </summary>
    /// <remarks>
    /// Based on https://code.msdn.microsoft.com/windowsdesktop/Creating-a-simple-plugin-b6174b62.
    /// </remarks>
    public sealed class PluginManager : IPluginManagerService
    {
        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins.
        /// </summary>
        public IEnumerable<IGroupChatPlugin> GroupChatPlugins
        {
            get
            {
                return Enumerable.Concat(this.GroupChatPluginsAutoInstalled, this.GroupChatPluginsBuiltIn).Concat(this.GroupChatPluginsManuallyInstalled);
            }
        }

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are built-in.
        /// </summary>
        public ICollection<IGroupChatPlugin> GroupChatPluginsBuiltIn { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are manually installed.
        /// </summary>
        public ICollection<IGroupChatPlugin> GroupChatPluginsManuallyInstalled { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IGroupChatPlugin"/> plugins that are automatically installed from a repo.
        /// </summary>
        public ICollection<IGroupChatPlugin> GroupChatPluginsAutoInstalled { get; } = new List<IGroupChatPlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public IEnumerable<IMessageComposePlugin> MessageComposePlugins
        {
            get
            {
                return Enumerable.Concat(this.MessageComposePluginsAutoInstalled, this.MessageComposePluginsBuiltIn).Concat(this.MessageComposePluginsManuallyInstalled);
            }
        }

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePluginsBuiltIn { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePluginsManuallyInstalled { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Gets the available <see cref="IMessageComposePlugin"/> plugins.
        /// </summary>
        public ICollection<IMessageComposePlugin> MessageComposePluginsAutoInstalled { get; } = new List<IMessageComposePlugin>();

        /// <summary>
        /// Loads and registers all available plugins.
        /// </summary>
        /// <param name="pluginsPath">The folder to load plugins from.</param>
        public void LoadPlugins(string pluginsPath)
        {
            this.GroupChatPluginsAutoInstalled.Clear();
            this.GroupChatPluginsBuiltIn.Clear();
            this.GroupChatPluginsManuallyInstalled.Clear();

            this.MessageComposePluginsAutoInstalled.Clear();
            this.MessageComposePluginsBuiltIn.Clear();
            this.MessageComposePluginsManuallyInstalled.Clear();
        }
    }
}
