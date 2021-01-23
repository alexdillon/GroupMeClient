using System.Windows;
using GroupMeClient.Core.Services;

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
            //var msg = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(new MessageBox.Avalonia.DTO.MessageBoxStandardParams
            //{
            //    ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.Ok,
            //    ContentTitle = parameters.Title,
            //    ContentMessage = parameters.Message,
            //    Icon = MessageBox.Avalonia.Enums.Icon.Error,
            //});
            //msg.ShowDialog(GroupMeClient.AvaloniaUI.Program.GroupMeMainWindow).Wait();

            //TODO10
        }

        private MessageBox.Avalonia.Enums.ButtonEnum GetButtons(MessageBoxParams.Buttons buttons)
        {
            switch (buttons)
            {
                case MessageBoxParams.Buttons.Ok:
                    return MessageBox.Avalonia.Enums.ButtonEnum.Ok;

                default:
                    return MessageBox.Avalonia.Enums.ButtonEnum.Ok;
            }
        }

        private MessageBox.Avalonia.Enums.Icon GetImage(MessageBoxParams.Icon image)
        {
            switch (image)
            {
                case MessageBoxParams.Icon.Error:
                    return MessageBox.Avalonia.Enums.Icon.Error;

                case MessageBoxParams.Icon.Success:
                    return MessageBox.Avalonia.Enums.Icon.Info;

                case MessageBoxParams.Icon.Warning:
                    return MessageBox.Avalonia.Enums.Icon.Warning;

                case MessageBoxParams.Icon.None:
                default:
                    return MessageBox.Avalonia.Enums.Icon.None;
            }
        }
    }
}
