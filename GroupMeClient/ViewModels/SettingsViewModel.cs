using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.ViewModels.Controls;
using MahApps.Metro.Controls.Dialogs;

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
            this.SettingsManager = settingsManager;

            this.ManageReposCommand = new RelayCommand(this.ManageRepos);
            this.ManageUpdatesCommand = new RelayCommand(this.ManageUpdates);

            this.DialogManager = new PopupViewModel()
            {
                PopupDialog = null,
                EasyClosePopup = null,
                ClosePopup = new RelayCommand(this.ClosePopup),
            };

            this.LoadPluginInfo();
        }

        /// <summary>
        /// Gets a collection installed plugins.
        /// </summary>
        public ObservableCollection<Plugin> InstalledPlugins { get; } = new ObservableCollection<Plugin>();

        /// <summary>
        /// Gets a string displaying the friendly version number for the application.
        /// </summary>
        public string ApplicationVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Gets a string describing the git commit the application has built from.
        /// </summary>
        public string ApplicationCommit => $"{ThisAssembly.Git.Commit}-{ThisAssembly.Git.Branch}{(ThisAssembly.Git.IsDirty ? "-dirty" : string.Empty)}";

        /// <summary>
        /// Gets the <see cref="PopupViewModel"/> for the Settings view.
        /// </summary>
        public PopupViewModel DialogManager { get; }

        /// <summary>
        /// Gets the command used to open a popup to manage plugin repositories.
        /// </summary>
        public ICommand ManageReposCommand { get; }

        /// <summary>
        /// Gets the command used to open a popup to manage plugin updates.
        /// </summary>
        public ICommand ManageUpdatesCommand { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the UI Setting for only showing previews when multiple images are attached to a single message is enabled.
        /// </summary>
        public bool ShowPreviewsForMultiImages
        {
            get
            {
                return this.SettingsManager.UISettings.ShowPreviewsForMultiImages;
            }

            set
            {
                this.SettingsManager.UISettings.ShowPreviewsForMultiImages = value;
                this.RaisePropertyChanged(nameof(this.ShowPreviewsForMultiImages));
                this.SettingsManager.SaveSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how many Multi-Chats are allowed in regular mode.
        /// </summary>
        public int MaximumNumberOfMultiChats
        {
            get
            {
                return this.SettingsManager.UISettings.MaximumNumberOfMultiChatsNormal;
            }

            set
            {
                this.SettingsManager.UISettings.MaximumNumberOfMultiChatsNormal = value;
                this.RaisePropertyChanged(nameof(this.MaximumNumberOfMultiChats));
                this.SettingsManager.SaveSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how many Multi-Chats are allowed in MiniBar mode.
        /// </summary>
        public int MaximumNumberOfMultiChatsMiniBar
        {
            get
            {
                return this.SettingsManager.UISettings.MaximumNumberOfMultiChatsMinibar;
            }

            set
            {
                this.SettingsManager.UISettings.MaximumNumberOfMultiChatsMinibar = value;
                this.RaisePropertyChanged(nameof(this.MaximumNumberOfMultiChatsMiniBar));
                this.SettingsManager.SaveSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether interactions with system notifications are allowed.
        /// </summary>
        public bool EnableNotificationInteractions
        {
            get
            {
                return this.SettingsManager.UISettings.EnableNotificationInteractions;
            }

            set
            {
                this.SettingsManager.UISettings.EnableNotificationInteractions = value;
                this.RaisePropertyChanged(nameof(this.EnableNotificationInteractions));
                this.SettingsManager.SaveSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the scaling factor used when rendering messages.
        /// </summary>
        public double ScalingFactorForMessages
        {
            get
            {
                return this.SettingsManager.UISettings.ScalingFactorForMessages;
            }

            set
            {
                this.SettingsManager.UISettings.ScalingFactorForMessages = value;
                this.RaisePropertyChanged(nameof(this.ScalingFactorForMessages));
                this.SettingsManager.SaveSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating which UI theme should be applied.
        /// </summary>
        public Settings.ThemeOptions Theme
        {
            get
            {
                return this.SettingsManager.UISettings.Theme;
            }

            set
            {
                this.SettingsManager.UISettings.Theme = value;
                this.RaisePropertyChanged(nameof(this.Theme));
                this.SettingsManager.SaveSettings();
            }
        }

        private Settings.SettingsManager SettingsManager { get; }

        private void LoadPluginInfo()
        {
            // Load Group Chat Plugins
            foreach (var plugin in Plugins.PluginManager.Instance.GroupChatPluginsBuiltIn)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Group Chat Plugins", Source = "Built-In" });
            }

            foreach (var plugin in Plugins.PluginManager.Instance.GroupChatPluginsAutoInstalled)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Group Chat Plugins", Source = "Auto Installed" });
            }

            foreach (var plugin in Plugins.PluginManager.Instance.GroupChatPluginsManuallyInstalled)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Group Chat Plugins", Source = "Manually Installed" });
            }

            // Load Message Effect Plugins
            foreach (var plugin in Plugins.PluginManager.Instance.MessageComposePluginsBuiltIn)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Message Effect Plugins", Source = "Built-In" });
            }

            foreach (var plugin in Plugins.PluginManager.Instance.MessageComposePluginsAutoInstalled)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Message Effect Plugins", Source = "Auto Installed" });
            }

            foreach (var plugin in Plugins.PluginManager.Instance.MessageComposePluginsManuallyInstalled)
            {
                var pluginBase = plugin as GroupMeClientPlugin.PluginBase;
                this.InstalledPlugins.Add(new Plugin() { Name = pluginBase.PluginDisplayName, Version = pluginBase.PluginVersion, Type = "Message Effect Plugins", Source = "Manually Installed" });
            }
        }

        private void ManageRepos()
        {
            var repoManager = new ManageReposViewModel();
            this.DialogManager.PopupDialog = repoManager;
        }

        private void ManageUpdates()
        {
            var updateManager = new UpdatePluginsViewModel();
            this.DialogManager.PopupDialog = updateManager;
        }

        private void ClosePopup()
        {
            this.DialogManager.PopupDialog = null;
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

            /// <summary>
            /// Gets or sets the source of the plugin.
            /// </summary>
            public string Source { get; set; }
        }
    }
}
