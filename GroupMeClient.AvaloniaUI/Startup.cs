using GalaSoft.MvvmLight.Ioc;

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
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IClipboardService, GroupMeClient.AvaloniaUI.Services.AvaloniaClipboardService>();
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IUserInterfaceDispatchService, GroupMeClient.AvaloniaUI.Services.AvaloniaDispatcherService>();
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IFileDialogService, GroupMeClient.AvaloniaUI.Services.AvaloniaFileDialogService>();
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IMessageBoxService, GroupMeClient.AvaloniaUI.Services.AvaloniaMessageBoxService>();
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IMessageRendererService, GroupMeClient.AvaloniaUI.Services.AvaloniaMessageRenderer>();
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IUserInterfaceDispatchService, GroupMeClient.AvaloniaUI.Services.AvaloniaDispatcherService>();
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IRestoreService, GroupMeClient.AvaloniaUI.Services.AvaloniaRestoreService>();

            // Setup Themeing
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IThemeService, GroupMeClient.AvaloniaUI.Services.AvaloniaThemeService>();

            // Setup Updates
            SimpleIoc.Default.Register<Core.Services.IUpdateService, Updates.UpdateAssist>();

            // Register Windows Services
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IOperatingSystemUIService, GroupMeClient.Desktop.Services.WinOperatingSystemUIService>();

            // Create Plugin Manager singleton
            SimpleIoc.Default.Register<GroupMeClient.Core.Services.IPluginManagerService, GroupMeClient.AvaloniaUI.Plugins.PluginManager>();
        }
    }
}
