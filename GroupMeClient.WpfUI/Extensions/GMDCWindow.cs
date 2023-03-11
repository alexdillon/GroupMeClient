using System;
using System.Windows;
using System.Windows.Input;

namespace GroupMeClient.WpfUI.Extensions
{
    /// <summary>
    /// Custom window type for GMDC with a MahApps-metro styled titlebar, but using native Windows glow.
    /// </summary>
    public class GMDCWindow : Window
    {
        /// <summary>
        /// Dependency Property for the <see cref="RightSideCommand"/> property.
        /// </summary>
        public static readonly DependencyProperty RightSideCommandsProp = DependencyProperty.Register(
            nameof(RightSideCommands),
            typeof(UIElement),
            typeof(GMDCWindow),
            new UIPropertyMetadata());

        /// <summary>
        /// <see cref="RoutedCommand"/> for closing the window.
        /// </summary>
        public static readonly RoutedCommand CloseWindowCommand = new RoutedCommand();

        /// <summary>
        /// <see cref="RoutedCommand"/> for restoring the window.
        /// </summary>
        public static readonly RoutedCommand RestoreWindowCommand = new RoutedCommand();

        /// <summary>
        /// <see cref="RoutedCommand"/> for maximizing the window.
        /// </summary>
        public static readonly RoutedCommand MaximizeWindowCommand = new RoutedCommand();

        /// <summary>
        /// <see cref="RoutedCommand"/> for minimizing the window.
        /// </summary>
        public static readonly RoutedCommand MinimizeWindowCommand = new RoutedCommand();

        /// <summary>
        /// Initializes a new instance of the <see cref="GMDCWindow"/> class.
        /// </summary>
        public GMDCWindow()
        {
            this.CommandBindings.Add(new CommandBinding(CloseWindowCommand, this.CloseWindowExecuted));
            this.CommandBindings.Add(new CommandBinding(MaximizeWindowCommand, this.MaximizeWindowExecuted));
            this.CommandBindings.Add(new CommandBinding(MinimizeWindowCommand, this.MinimizeWindowExecuted));
            this.CommandBindings.Add(new CommandBinding(RestoreWindowCommand, this.RestoreWindowExecuted));
        }

        /// <summary>
        /// Gets or sets the custom command content shown on the right side of the titlebar.
        /// </summary>
        public UIElement RightSideCommands
        {
            get => (UIElement)this.GetValue(RightSideCommandsProp);
            set => this.SetValue(RightSideCommandsProp, value);
        }

        private void RestoreWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void MinimizeWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaximizeWindowExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void CloseWindowExecuted(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            SystemCommands.CloseWindow(this);
        }
    }
}
