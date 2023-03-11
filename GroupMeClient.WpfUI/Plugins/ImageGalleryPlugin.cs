using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupMeClient.Core.Plugins.ViewModels;
using GroupMeClient.Core.Services;
using GroupMeClient.WpfUI.Plugins.Views;
using GroupMeClientApi.Models;
using GroupMeClientPlugin;
using GroupMeClientPlugin.GroupChat;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.WpfUI.Plugins
{
    /// <summary>
    /// <see cref="ImageGalleryPlugin"/> defines a GroupMe Desktop Client Plugin that can be used
    /// to display an image gallery for a specific group or chat.
    /// </summary>
    public class ImageGalleryPlugin : PluginBase, IGroupChatPlugin
    {
        /// <inheritdoc/>
        public string PluginName => "Image Gallery";

        /// <inheritdoc/>
        public override string PluginDisplayName => this.PluginName;

        /// <inheritdoc/>
        public override string PluginVersion => Core.GlobalAssemblyInfo.SimpleVersion;

        /// <inheritdoc/>
        public override Version ApiVersion => new Version(2, 0, 0);

        /// <inheritdoc/>
        public Task Activated(IMessageContainer groupOrChat, CacheSession cacheSession, IPluginUIIntegration integration, Action<CacheSession> cleanup)
        {
            var dataContext = new ImageGalleryWindowViewModel(groupOrChat, cacheSession, integration);
            var window = new ImageGalleryWindow
            {
                DataContext = dataContext,
            };

            window.Closing += (s, e) =>
            {
                cleanup(cacheSession);
            };

            var uiDispatcher = Ioc.Default.GetService<IUserInterfaceDispatchService>();
            uiDispatcher.Invoke(() =>
            {
                window.Show();
            });

            return Task.CompletedTask;
        }
    }
}
