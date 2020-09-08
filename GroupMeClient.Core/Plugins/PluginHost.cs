using System;
using GroupMeClient.Core.Caching;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using Microsoft.EntityFrameworkCore;

namespace GroupMeClient.Core.Plugins
{
    /// <summary>
    /// <see cref="PluginHost"/> allows for launching and managing GMDC interactive plugins.
    /// </summary>
    public class PluginHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginHost"/> class.
        /// </summary>
        /// <param name="cacheManager">The cache manager instance to use for plugin activation.</param>
        /// <param name="integrationProvider">
        /// A UI component that provides integration services to allow plugins
        /// to interact with the GMDC User Interface.</param>
        public PluginHost(CacheManager cacheManager, IPluginUIIntegration integrationProvider)
        {
            this.CacheManager = cacheManager;
            this.IntegrationProvider = integrationProvider;
        }

        private CacheManager CacheManager { get; }

        private IPluginUIIntegration IntegrationProvider { get; }

        /// <summary>
        /// Begins execution of a GroupMe plugin (<see cref="IGroupChatPlugin"/>) for a specific <see cref="IMessageContainer"/>.
        /// </summary>
        /// <param name="group">The <see cref="IMessageContainer"/> to execute the plugin on.</param>
        /// <param name="plugin">The plugin that should be executed.</param>
        public void RunPlugin(IMessageContainer group, IGroupChatPlugin plugin)
        {
            var cacheContext = this.CacheManager.OpenNewContext();

            var cacheSession = new CacheSession(
                 CacheManager.GetMessagesForGroup(group, cacheContext),
                 cacheContext.Messages.AsNoTracking(),
                 cacheContext);

            _ = plugin.Activated(group, cacheSession, this.IntegrationProvider, this.CleanupPlugin);
        }

        /// <summary>
        /// Provides cleanup services after a plugin terminates with a specific <see cref="CacheSession"/>.
        /// </summary>
        /// <param name="cacheSession">The session used by the terminated plugin.</param>
        private void CleanupPlugin(CacheSession cacheSession)
        {
            (cacheSession.Tag as Caching.CacheManager.CacheContext).Dispose();
        }
    }
}
