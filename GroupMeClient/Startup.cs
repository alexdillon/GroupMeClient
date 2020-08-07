using GalaSoft.MvvmLight.Ioc;

namespace GroupMeClient.WpfUI
{
    /// <summary>
    /// <see cref="Startup"/> provides support for DI and IoC service registration for the GMDC/WPF Client.
    /// </summary>
    internal class Startup
    {
        /// <summary>
        /// Initializes all the required services for the GMDC/WPF Client.
        /// </summary>
        public static void StartupServices()
        {
            // Register WPF Services
            SimpleIoc.Default.Register<Core.Services.IClipboardService, Services.WpfClipboardService>();
            SimpleIoc.Default.Register<Core.Services.IUserInterfaceDispatchService, Services.WpfDispatcherService>();
            SimpleIoc.Default.Register<Core.Services.IFileDialogService, Services.WpfFileDialogService>();
            SimpleIoc.Default.Register<Core.Services.IMessageBoxService, Services.WpfMessageBoxService>();
            SimpleIoc.Default.Register<Core.Services.IMessageRendererService, Services.WpfMessageRenderer>();
            SimpleIoc.Default.Register<Core.Services.IUserInterfaceDispatchService, Services.WpfDispatcherService>();
            SimpleIoc.Default.Register<Core.Services.IRestoreService, Services.WpfRestoreService>();

            // Setup Themeing
            SimpleIoc.Default.Register<Core.Services.IThemeService, Services.WpfThemeService>();

            // Setup Updates
            SimpleIoc.Default.Register<Core.Services.IUpdateService, Updates.UpdateAssist>();

            // Setup Windows services
            SimpleIoc.Default.Register<Core.Services.IOperatingSystemUIService, Desktop.Services.WinOperatingSystemUIService>();

            // Create Plugin Manager singleton
            SimpleIoc.Default.Register<Core.Services.IPluginManagerService, Plugins.PluginManager>();
        }
    }
}
