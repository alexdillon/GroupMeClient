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
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IClipboardService, Services.WpfClipboardService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IUserInterfaceDispatchService, Services.WpfDispatcherService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IFileDialogService, Services.WpfFileDialogService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IMessageBoxService, Services.WpfMessageBoxService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IMessageRendererService, Services.WpfMessageRenderer>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IUserInterfaceDispatchService, Services.WpfDispatcherService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IRestoreService, Services.WpfRestoreService>();

            // Setup Themeing
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IThemeService, Services.WpfThemeService>();

            // Setup Windows services
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IOperatingSystemUIService, Desktop.Services.WinOperatingSystemUIService>();

            // Create Plugin Manager singleton
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IPluginManagerService, Plugins.PluginManager>();
        }
    }
}
