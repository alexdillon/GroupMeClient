using System.Windows;
using GroupMeClient.Core.Services;

namespace GroupMeClient.Wpf.Services
{
    /// <summary>
    /// <see cref="WpfMessageBoxService"/> provides a platform-native dialog service for Windows/WPF.
    /// </summary>
    public class WpfMessageBoxService : IMessageBoxService
    {
        /// <inheritdoc/>
        public void ShowMessageBox(MessageBoxParams parameters)
        {
            MessageBox.Show(
                parameters.Message,
                parameters.Title,
                this.GetButtons(parameters.MessageBoxButtons),
                this.GetImage(parameters.MessageBoxIcons));
        }

        private MessageBoxButton GetButtons(MessageBoxParams.Buttons buttons)
        {
            switch (buttons)
            {
                case MessageBoxParams.Buttons.Ok:
                    return MessageBoxButton.OK;

                default:
                    return MessageBoxButton.OK;
            }
        }

        private MessageBoxImage GetImage(MessageBoxParams.Icon image)
        {
            switch (image)
            {
                case MessageBoxParams.Icon.Error:
                    return MessageBoxImage.Error;

                case MessageBoxParams.Icon.Success:
                    return MessageBoxImage.Information;

                case MessageBoxParams.Icon.Warning:
                    return MessageBoxImage.Warning;

                case MessageBoxParams.Icon.None:
                default:
                    return MessageBoxImage.None;
            }
        }
    }
}
