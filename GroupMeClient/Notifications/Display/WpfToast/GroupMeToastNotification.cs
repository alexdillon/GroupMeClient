using System.Windows;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi;
using GroupMeClientApi.Models;
using ToastNotifications.Core;
using ToastNotifications.Messages.Core;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    /// <summary>
    /// <see cref="GroupMeToastNotification"/> provides a ViewModel for the GroupMeToastDisplayPart control.
    /// </summary>
    public class GroupMeToastNotification : MessageBase<GroupMeToastDisplayPart>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeToastNotification"/> class.
        /// </summary>
        /// <param name="message">The notification body.</param>
        /// <param name="avatar">The notification avatar.</param>
        /// <param name="imageDownloader">The downloader to use when displaying the avatar.</param>
        public GroupMeToastNotification(string message, IAvatarSource avatar, ImageDownloader imageDownloader)
            : base(
                  message,
                  new MessageOptions() { FreezeOnMouseEnter = false, })
        {
            this.Avatar = new AvatarControlViewModel(avatar, imageDownloader);
        }

        /// <summary>
        /// Gets the avatar to display.
        /// </summary>
        public AvatarControlViewModel Avatar { get; }

        /// <inheritdoc/>
        protected override GroupMeToastDisplayPart CreateDisplayPart()
        {
            return new GroupMeToastDisplayPart(this);
        }

        /// <inheritdoc/>
        protected override void UpdateDisplayOptions(GroupMeToastDisplayPart displayPart, MessageOptions options)
        {
            if (options.FontSize != null)
            {
                displayPart.Text.FontSize = options.FontSize.Value;
            }

            displayPart.CloseButton.Visibility = options.ShowCloseButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
