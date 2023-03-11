using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using GroupMeClient.AvaloniaUI.ViewModels;
using GroupMeClient.AvaloniaUI.Views;
using GroupMeClient.Core.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace GroupMeClient.AvaloniaUI
{
    /// <summary>
    /// The entry point for the GroupMe Desktop Client Avalonia application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments to launch with.</param>
        public static void Main(string[] args)
        {
            // Initialization code. Don't use any Avalonia, third-party APIs or any
            // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
            // yet and stuff might break.
            BuildAvaloniaApp().Start(AppMain, args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        public static MainWindow GroupMeMainWindow;

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            GroupMeMainWindow = new MainWindow
            {
                DataContext = new MainViewModel(),
            };

            // Initialize the theme engine now that the UI has been defined
            var themeService = Ioc.Default.GetRequiredService<IThemeService>();
            themeService.Initialize();

            app.Run(GroupMeMainWindow);
        }
    }
}
