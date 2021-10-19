using System;
using System.IO;
using System.Windows;
using GroupMeClient.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.WpfUI
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost host;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            Directory.CreateDirectory(DataRoot);

            var clientIdentity = new Core.Services.KnownClients.GMDC();
            StartupParams = new Core.StartupExtensions.StartupParameters()
            {
                ClientIdentity = clientIdentity,
                CacheFilePath = CachePath,
                PersistFilePath = PersistPath,
                SettingsFilePath = SettingsPath,
                PluginPath = PluginsPath,
            };

            this.host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.UseGMDCCoreServices(StartupParams);
                    services.UseGMDCCoreViewModels();
                    services.UseGMDCWpf();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                .Build();

            Ioc.Default.ConfigureServices(this.host.Services);
        }

        /// <summary>
        /// Gets the AUMID identifier used for Windows.
        /// </summary>
        public static string ApplicationId => "com.squirrel.GroupMeDesktopClient.GroupMeClient";

        /// <summary>
        /// Gets the data root for the GMDC/WPF Application.
        /// </summary>
        public static string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MicroCube", "GroupMe Desktop Client");

        /// <summary>
        /// Gets the name of the settings file for the GMDC/WPF application.
        /// </summary>
        public static string SettingsPath => Path.Combine(DataRoot, "settings.json");

        /// <summary>
        /// Gets the cache database file path for the GMDC/WPF application.
        /// </summary>
        public static string CachePath => Path.Combine(DataRoot, "cache.db");

        /// <summary>
        /// Gets the persist database file path for the GMDC/WPF application.
        /// </summary>
        public static string PersistPath => Path.Combine(DataRoot, "persist.db");

        /// <summary>
        /// Gets the image caching folder path for the GMDC/WPF application.
        /// </summary>
        public static string ImageCachePath => Path.Combine(DataRoot, "ImageCache");

        /// <summary>
        /// Gets the plugin directory for the GMDC/WPF application.
        /// </summary>
        public static string PluginsPath => Path.Combine(DataRoot, "Plugins");

        public static Core.StartupExtensions.StartupParameters StartupParams { get; set; }
    }
}