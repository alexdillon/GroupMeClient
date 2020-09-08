using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Plugins;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Tasks;
using GroupMeClient.Core.ViewModels;
using GroupMeClientPlugin.GroupChat;

namespace GroupMeClient.Core
{
    /// <summary>
    /// <see cref="Startup"/> provides support for initializing the GMDC Core Engine.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes core services for the GMDC Core Engine.
        /// </summary>
        /// <param name="startupParameters">The startup parameters to use.</param>
        public static void StartupCoreServices(StartupParameters startupParameters)
        {
            SimpleIoc.Default.Register<TaskManager>();
            SimpleIoc.Default.Register(() => startupParameters.ClientIdentity);
            SimpleIoc.Default.Register(() => new CacheManager(startupParameters.CacheFilePath, SimpleIoc.Default.GetInstance<TaskManager>()));
            SimpleIoc.Default.Register(() => new PersistManager(startupParameters.PersistFilePath));
            SimpleIoc.Default.Register(() => new SettingsManager(startupParameters.SettingsFilePath));
            SimpleIoc.Default.Register(() => new PluginInstaller(startupParameters.PluginPath));
            SimpleIoc.Default.Register<PluginHost>();
        }

        /// <summary>
        /// Registers all top-level ViewModels with the service system.
        /// </summary>
        public static void RegisterTopLevelViewModels()
        {
            SimpleIoc.Default.Register<ChatsViewModel>(createInstanceImmediately: false);
            SimpleIoc.Default.Register<SearchViewModel>(createInstanceImmediately: false);
            SimpleIoc.Default.Register<StarsViewModel>(createInstanceImmediately: false);
            SimpleIoc.Default.Register<SettingsViewModel>(createInstanceImmediately: false);

            // UI integration is provided via the Seach page for the show-in-context feature.
            SimpleIoc.Default.Register<IPluginUIIntegration>(
                () => SimpleIoc.Default.GetInstance<SearchViewModel>(),
                createInstanceImmediately: false);
        }

        /// <summary>
        /// <see cref="StartupParameters"/> defines the required configuration items that must be provided from a GMDC UI implementation
        /// to the GMDC Core Engine in order to complete initialization.
        /// </summary>
        public class StartupParameters
        {
            /// <summary>
            /// Gets or sets the identity of the client that is initializing GMDC Core.
            /// </summary>
            public IClientIdentityService ClientIdentity { get; set; }

            /// <summary>
            /// Gets or sets the location of the caching database file.
            /// </summary>
            public string CacheFilePath { get; set; }

            /// <summary>
            /// Gets or sets the location of the persistant storage file.
            /// </summary>
            public string PersistFilePath { get; set; }

            /// <summary>
            /// Gets or sets the location of the application settings file.
            /// </summary>
            public string SettingsFilePath { get; set; }

            /// <summary>
            /// Gets or sets the location to load plugins from.
            /// </summary>
            public string PluginPath { get; set; }
        }
    }
}
