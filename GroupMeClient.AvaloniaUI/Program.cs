using System;
using Avalonia;
using Avalonia.ReactiveUI;
using GroupMeClient.AvaloniaUI.ViewModels;
using GroupMeClient.AvaloniaUI.Views;

namespace GroupMeClient.AvaloniaUI
{
    /// <summary>
    /// The entry point for the GroupMe Desktop Client Avalonia application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Gets the main window instance for this application.
        /// </summary>
        public static MainWindow GMDCMainWindow { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">The command line arguments to launch with.</param>
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
           .StartWithClassicDesktopLifetime(args);

        /// <summary>
        /// Configures the Avalonia framework for this application.
        /// </summary>
        /// <returns>A configured <see cref="AppBuilder"/>.</returns>
        /// <remarks>
        /// Avalonia configuration, don't remove; also used by visual designer.
        /// </remarks>
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI();

        /// <summary>
        /// Creates a main window for GMDC. If the main window
        /// has already been created, this will return the existing instance.
        /// </summary>
        /// <returns>The <see cref="MainWindow"/>.</returns>
        public static MainWindow CreateMainWindow()
        {
            if (GMDCMainWindow == null)
            {
                GMDCMainWindow = new MainWindow
                {
                    DataContext = new MainViewModel(),
                };
            }

            return GMDCMainWindow;
        }
    }
}
