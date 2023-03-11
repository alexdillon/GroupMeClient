using System;
using System.Windows;
using System.Windows.Interop;
using GroupMeClient.Core.Services;
using GroupMeClient.WpfUI.ViewModels;
using GroupMeClient.WpfUI.Views;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfWindowService"/> provides support for creating new windows using WPF.
    /// </summary>
    public class WpfWindowService : IWindowService
    {
        /// <inheritdoc/>
        public void ShowWindow(WindowParams windowParams)
        {
            this.CreateWindow(windowParams).Show();
        }

        /// <inheritdoc/>
        public void ShowDialog(WindowParams windowParams)
        {
            this.CreateWindow(windowParams).ShowDialog();
        }

        private Window CreateWindow(WindowParams windowParams)
        {
            var hostVm = new WindowHostViewModel(windowParams.Content, windowParams.Tag);
            var host = new WindowHost()
            {
                DataContext = hostVm,
            };

            var window = new Window()
            {
                Title = windowParams.Title,
                Content = host,
                DataContext = hostVm,
                Topmost = windowParams.TopMost,
                Tag = windowParams.Tag,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
            };

            if (windowParams.Width > 0)
            {
                window.Width = windowParams.Width;
            }

            if (windowParams.Height > 0)
            {
                window.Height = windowParams.Height;
            }

            var screen = SystemParameters.WorkArea;
            var windowOffset = 7;

            switch (windowParams.StartingLocation)
            {
                case WindowParams.Location.Default:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    break;

                case WindowParams.Location.Manual:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = windowParams.StartingX;
                    window.Top = windowParams.StartingY;
                    break;

                case WindowParams.Location.CenterScreen:
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    break;

                case WindowParams.Location.BottomRight:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = screen.Right - window.Width + windowOffset;
                    window.Top = screen.Bottom - window.Height + windowOffset;
                    break;

                case WindowParams.Location.BottomLeft:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = screen.Left;
                    window.Top = screen.Bottom - window.Height + windowOffset;
                    break;

                case WindowParams.Location.TopLeft:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = screen.Left;
                    window.Top = screen.Top;
                    break;

                case WindowParams.Location.TopRight:
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                    window.Left = screen.Right - window.Width + windowOffset;
                    window.Top = screen.Top;
                    break;
            }

            window.Closing += (s, e) => windowParams.CloseCallback?.Invoke();

            return window;
        }
    }
}
