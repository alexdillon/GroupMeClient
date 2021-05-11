using System;
using Microsoft.Extensions.DependencyInjection;

namespace GroupMeClient.WpfUI
{
    /// <summary>
    /// <see cref="Startup"/> provides support for DI and IoC service registration for the GMDC/WPF Client.
    /// </summary>
    internal static class StartupExtensions
    {
        /// <summary>
        /// Initializes all the required services for the GMDC/WPF Client.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        public static void UseGMDCWpf(this IServiceCollection services)
        {
            // Register WPF Services
            services.AddSingleton<Core.Services.IClipboardService, Services.WpfClipboardService>();
            services.AddSingleton<Core.Services.IUserInterfaceDispatchService, Services.WpfDispatcherService>();
            services.AddSingleton<Core.Services.IFileDialogService, Services.WpfFileDialogService>();
            services.AddSingleton<Core.Services.IMessageBoxService, Services.WpfMessageBoxService>();
            services.AddSingleton<Core.Services.IMessageRendererService, Services.WpfMessageRenderer>();
            services.AddSingleton<Core.Services.IUserInterfaceDispatchService, Services.WpfDispatcherService>();
            services.AddSingleton<Core.Services.IRestoreService, Services.WpfRestoreService>();
            services.AddSingleton<Core.Services.IImageService, Services.WpfImageService>();
            services.AddSingleton<Core.Services.IWindowService, Services.WpfWindowService>();

            // Setup Themeing
            services.AddSingleton<Core.Services.IThemeService, Services.WpfThemeService>();

            // Setup Updates
            services.AddSingleton<Core.Services.IUpdateService, Updates.UpdateAssist>();

            // Setup Windows services
            services.AddSingleton<Core.Services.IOperatingSystemUIService, Desktop.Services.WinOperatingSystemUIService>();

            // Create Plugin Manager singleton
            services.AddSingleton<Core.Services.IPluginManagerService, Plugins.PluginManager>();

            // Register the app AUMID for taskbar grouping
            Desktop.Native.Windows.TaskBar.SetCurrentProcessExplicitAppUserModelID(Notifications.Display.Win10.Win10ToastNotificationsProvider.ApplicationId);
        }
    }
}
