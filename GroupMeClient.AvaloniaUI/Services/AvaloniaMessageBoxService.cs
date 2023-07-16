using System.Windows;
using GroupMeClient.Core.Services;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace GroupMeClient.AvaloniaUI.Services
{
    /// <summary>
    /// <see cref="AvaloniaMessageBoxService"/> provides a platform-native dialog service for Windows/WPF.
    /// </summary>
    public class AvaloniaMessageBoxService : IMessageBoxService
    {
        /// <inheritdoc/>
        public void ShowMessageBox(MessageBoxParams parameters)
        {
            var msg = MessageBoxManager.GetMessageBoxStandard(new MsBox.Avalonia.Dto.MessageBoxStandardParams
            {
                ButtonDefinitions = this.GetButtons(parameters.MessageBoxButtons),
                ContentTitle = parameters.Title,
                ContentMessage = parameters.Message,
                Icon = this.GetImage(parameters.MessageBoxIcons),
            });

            msg.ShowAsync().Wait();
        }

        private ButtonEnum GetButtons(MessageBoxParams.Buttons buttons)
        {
            switch (buttons)
            {
                case MessageBoxParams.Buttons.Ok:
                    return ButtonEnum.Ok;

                default:
                    return ButtonEnum.Ok;
            }
        }

        private Icon GetImage(MessageBoxParams.Icon image)
        {
            switch (image)
            {
                case MessageBoxParams.Icon.Error:
                    return Icon.Error;

                case MessageBoxParams.Icon.Success:
                    return Icon.Info;

                case MessageBoxParams.Icon.Warning:
                    return Icon.Warning;

                case MessageBoxParams.Icon.None:
                default:
                    return Icon.None;
            }
        }
    }
}
