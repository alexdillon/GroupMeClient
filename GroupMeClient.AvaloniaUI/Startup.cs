namespace GroupMeClient.AvaloniaUI
{
    /// <summary>
    /// <see cref="Startup"/> provides support for DI and IoC service registration for the GMDC/Avalonia Client.
    /// </summary>
    internal class Startup
    {
        /// <summary>
        /// Initializes all the required services for the GMDC/Avalonia Client.
        /// </summary>
        public static void StartupServices()
        {
            // Register Avalonia Services
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IClipboardService, GroupMeClient.AvaloniaUI.Services.AvaloniaClipboardService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IUserInterfaceDispatchService, GroupMeClient.AvaloniaUI.Services.AvaloniaDispatcherService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IFileDialogService, GroupMeClient.AvaloniaUI.Services.AvaloniaFileDialogService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IMessageBoxService, GroupMeClient.AvaloniaUI.Services.AvaloniaMessageBoxService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IMessageRendererService, GroupMeClient.AvaloniaUI.Services.AvaloniaMessageRenderer>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IUserInterfaceDispatchService, GroupMeClient.AvaloniaUI.Services.AvaloniaDispatcherService>();
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IRestoreService, GroupMeClient.AvaloniaUI.Services.AvaloniaRestoreService>();

            // Register Windows Services
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IOperatingSystemUIService, GroupMeClient.Desktop.Services.WinOperatingSystemUIService>();

            // Setup Themeing
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IThemeService, GroupMeClient.AvaloniaUI.Services.AvaloniaThemeService>();

            // Create Plugin Manager singleton
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<GroupMeClient.Core.Services.IPluginManagerService, GroupMeClient.AvaloniaUI.Plugins.PluginManager>();
        }
    }
}
