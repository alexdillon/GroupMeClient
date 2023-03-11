using System.Net;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Plugins;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Tasks;
using GroupMeClient.Core.ViewModels;
using GroupMeClientPlugin.GroupChat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.Core
{
    /// <summary>
    /// <see cref="StartupExtensions"/> provides support for initializing the GMDC Core Engine.
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Initializes core services for the GMDC Core Engine.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="startupParameters">The startup parameters to use.</param>
        public static void UseGMDCCoreServices(this IServiceCollection services, StartupParameters startupParameters)
        {
            services.AddSingleton<TaskManager>();
            services.AddSingleton((s) => startupParameters.ClientIdentity);
            services.AddSingleton((s) => new CacheManager(startupParameters.CacheFilePath, Ioc.Default.GetService<TaskManager>(), Ioc.Default.GetService<SettingsManager>()));
            services.AddSingleton((s) => new PersistManager(startupParameters.PersistFilePath));
            services.AddSingleton((s) => new SettingsManager(startupParameters.SettingsFilePath));
            services.AddSingleton((s) => new PluginInstaller(startupParameters.PluginPath));
            services.AddSingleton<PluginHost>();

            services.AddSingleton<GroupMeClientApi.GroupMeClient, GroupMeClientService>();

            AdditionalStartupConfig();
        }

        /// <summary>
        /// Registers all top-level ViewModels with the service system.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        public static void UseGMDCCoreViewModels(this IServiceCollection services)
        {
            services.AddSingleton<ChatsViewModel>();
            services.AddSingleton<SearchViewModel>();
            services.AddSingleton<StarsViewModel>();
            services.AddSingleton<SettingsViewModel>();

            // UI integration is provided via the Seach page for the show-in-context feature.
            services.AddSingleton<IPluginUIIntegration>(
                (s) => Ioc.Default.GetService<SearchViewModel>());
        }

        private static void AdditionalStartupConfig()
        {
            // Windows 7 and prior will not have TLS1.2 enabled by default in all cases.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
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
