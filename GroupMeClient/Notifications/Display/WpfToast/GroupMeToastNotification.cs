using System.Windows;
using GroupMeClient.ViewModels.Controls;
using GroupMeClientApi;
using GroupMeClientApi.Models;
using ToastNotifications.Core;
using ToastNotifications.Messages.Core;

namespace GroupMeClient.Notifications.Display.WpfToast
{
    public class GroupMeToastNotification : MessageBase<GroupMeToastDisplayPart>
    {
        public GroupMeToastNotification(string message, IAvatarSource avatar, ImageDownloader imageDownloader)
            : base(message, new MessageOptions())
        {
            this.Avatar = new AvatarControlViewModel(avatar, imageDownloader);
        }

        public AvatarControlViewModel Avatar { get; }

        protected override GroupMeToastDisplayPart CreateDisplayPart()
        {
            return new GroupMeToastDisplayPart(this);
        }

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
