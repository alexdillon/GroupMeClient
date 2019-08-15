using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="SettingsViewModel"/> provides a ViewModel for the <see cref="Views.SettingsView"/>.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        /// <param name="settingsManager">The settings manager to use.</param>
        public SettingsViewModel(Settings.SettingsManager settingsManager)
        {
            this.InstalledPlugins = new ObservableCollection<Plugin>();

            this.LoadPluginInfo();
        }

        /// <summary>
        /// Gets a collection installed plugins.
        /// </summary>
        public ObservableCollection<Plugin> InstalledPlugins { get; } = new ObservableCollection<Plugin>();

        private void LoadPluginInfo()
        {
            // Load Group Chat Plugins
            foreach (var plugin in Plugins.PluginManager.Instance.GroupChatPlugins)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Group Chat Plugin" });
            }

            // Load Message Effect Plugins
            foreach (var plugin in Plugins.PluginManager.Instance.MessageComposePlugins)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Message Effect Plugin" });
            }
        }

        /// <summary>
        /// <see cref="Plugin"/> provides metadata and display information about a GroupMe Desktop Client plugin.
        /// </summary>
        public struct Plugin
        {
            /// <summary>
            /// Gets or sets the plugin name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the plugin version.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Gets or sets the plugin type.
            /// </summary>
            public string Type { get; set; }
        }
    }
}
