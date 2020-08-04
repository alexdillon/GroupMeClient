namespace GroupMeClient.Wpf
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
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IClipboardService, Wpf.Services.WpfClipboardService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IUserInterfaceDispatchService, Wpf.Services.WpfDispatcherService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IFileDialogService, Wpf.Services.WpfFileDialogService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IMessageBoxService, Wpf.Services.WpfMessageBoxService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IMessageRendererService, Wpf.Services.WpfMessageRenderer>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IUserInterfaceDispatchService, Wpf.Services.WpfDispatcherService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IOperatingSystemUIService, Wpf.Services.WinOperatingSystemUIService>();

            // Create Plugin Manager singleton
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<Core.Services.IPluginManagerService, Wpf.Plugins.PluginManager>();
        }
    }
}
