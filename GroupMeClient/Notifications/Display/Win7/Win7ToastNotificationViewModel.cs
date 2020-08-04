using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GroupMeClient.Wpf.Utilities;

namespace GroupMeClient.Wpf.Notifications.Display.Win7
{
    /// <summary>
    /// <see cref="Win7ToastNotificationViewModel"/> provides a ViewModel for the <see cref="Win7.Win7ToastNotification"/> control.
    /// </summary>
    public class Win7ToastNotificationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Win7ToastNotificationViewModel"/> class.
        /// </summary>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The body contents of the notification.</param>
        /// <param name="imageData">The bytes of image data for the avatar to display.</param>
        public Win7ToastNotificationViewModel(string title, string message, byte[] imageData)
        {
            this.Title = title;
            this.Message = message;
            this.ImageData = ImageUtils.BytesToImageSource(imageData);
        }

        /// <summary>
        /// Gets the title for the notification.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the message for the notification.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets a <see cref="ImageSource"/> containing the image data for this notification.
        /// </summary>
        public ImageSource ImageData { get; }
    }
}
