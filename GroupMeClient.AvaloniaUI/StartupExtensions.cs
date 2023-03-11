using Microsoft.Extensions.DependencyInjection;

namespace GroupMeClient.AvaloniaUI
{
    /// <summary>
    /// <see cref="Startup"/> provides support for DI and IoC service registration for the GMDC/Avalonia Client.
    /// </summary>
    internal static class StartupExtensions
    {
        /// <summary>
        /// Initializes all the required services for the GMDC/Avalonia Client.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        public static void UseGMDCAvalonia(this IServiceCollection services)
        {
            // Register Avalonia Services
            services.AddSingleton<Core.Services.IClipboardService, Services.AvaloniaClipboardService>();
            services.AddSingleton<Core.Services.IUserInterfaceDispatchService, Services.AvaloniaDispatcherService>();
            services.AddSingleton<Core.Services.IFileDialogService, Services.AvaloniaFileDialogService>();
            services.AddSingleton<Core.Services.IMessageBoxService, Services.AvaloniaMessageBoxService>();
            services.AddSingleton<Core.Services.IUserInterfaceDispatchService, Services.AvaloniaDispatcherService>();
            services.AddSingleton<Core.Services.IRestoreService, Services.AvaloniaRestoreService>();
            services.AddSingleton<Core.Services.IImageService, Services.AvaloniaImageService>();

            // Setup Themeing
            services.AddSingleton<Core.Services.IThemeService, Services.AvaloniaThemeService>();

            // Setup Updates
            services.AddSingleton<Core.Services.IUpdateService, Updates.UpdateAssist>();

            // Register Windows Services
            services.AddSingleton<Core.Services.IOperatingSystemUIService, Desktop.Services.WinOperatingSystemUIService>();

            // Create Plugin Manager singleton
            services.AddSingleton<Core.Services.IPluginManagerService, Plugins.PluginManager>();
        }
    }
}
