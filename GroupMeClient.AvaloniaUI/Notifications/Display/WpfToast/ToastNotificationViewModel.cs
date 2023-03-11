using System.Windows.Input;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi;
using GroupMeClientApi.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.AvaloniaUI.Notifications.Display.WpfToast
{
    /// <summary>
    /// <see cref="GroupMeToastNotification"/> provides a ViewModel for the <see cref="ToastNotificationViewModel"/> control.
    /// </summary>
    public class ToastNotificationViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToastNotificationViewModel"/> class.
        /// </summary>
        /// <param name="message">The notification body.</param>
        /// <param name="avatar">The notification avatar.</param>
        /// <param name="imageDownloader">The downloader to use when displaying the avatar.</param>
        public ToastNotificationViewModel(string message, IAvatarSource avatar, ImageDownloader imageDownloader)
        {
            this.Message = message;
            this.Avatar = new AvatarControlViewModel(avatar, imageDownloader);
        }

        /// <summary>
        /// Gets the avatar to display.
        /// </summary>
        public AvatarControlViewModel Avatar { get; }

        /// <summary>
        /// Gets the message to display.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets the action to perform when the Toast Notification is dismissed.
        /// </summary>
        public ICommand CloseAction { get; set; }
    }
}
