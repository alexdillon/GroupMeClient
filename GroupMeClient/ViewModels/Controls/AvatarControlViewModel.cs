using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GroupMeClient.ViewModels.Controls
{
    public class AvatarControlViewModel : ViewModelBase
    {
        public AvatarControlViewModel(Group group)
        {
            this.Group = group;
            _ = LoadAvatar();
        }

        public AvatarControlViewModel(Chat chat)
        {
            this.Chat = chat;
            _ = LoadAvatar();
        }

        public AvatarControlViewModel(Message message)
        {
            this.Message = message;
            _ = LoadAvatar();
        }

        public Group Group { get; }

        public Chat Chat { get; }

        public Message Message { get; }

        private ImageSource avatarRound;
        private ImageSource avatarSquare;

        /// <summary>
        /// Gets the image that should be used for rounded avatars.
        /// If the avatar shouldn't be rounded, null is returned.
        /// </summary>
        public ImageSource AvatarRound
        {
            get { return this.avatarRound; }
            set { Set(() => this.AvatarRound, ref avatarRound, value); }
        }

        /// <summary>
        /// Gets the image that should be used for square avatars.
        /// If the avatar shouldn't be rectangular, null is returned.
        /// </summary>
        public ImageSource AvatarSquare
        {
            get { return this.avatarSquare; }
            set { Set(() => this.AvatarSquare, ref avatarSquare, value); }
        }

        public async Task LoadAvatar()
        {
            bool isSquare;

            byte[] image;
            if (this.Group != null)
            {
                image = await this.Group.DownloadAvatar();
                isSquare = true;
            }
            else if (this.Chat != null)
            {
                image = await this.Chat.DownloadAvatar();
                isSquare = false;
            }
            else if (this.Message != null)
            {
                var downloader = this.Message.ImageDownloader;
                image = await downloader.DownloadAvatarImage(this.Message.AvatarUrl);
                isSquare = false;
            }
            else
            {
                return;
            }

            using (var ms = new System.IO.MemoryStream(image))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                if (isSquare)
                {
                    this.AvatarSquare = bitmapImage;
                }
                else
                {
                    this.AvatarRound = bitmapImage;
                }
            }
        }
    }
}
