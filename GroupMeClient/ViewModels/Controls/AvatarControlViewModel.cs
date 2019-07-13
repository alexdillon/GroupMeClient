using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GroupMeClientApi.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GroupMeClientApi;

namespace GroupMeClient.ViewModels.Controls
{
    public class AvatarControlViewModel : ViewModelBase
    {
        public AvatarControlViewModel(IMessageContainer messageContainer)
        {
            this.MessageContainer = messageContainer;
            _ = LoadAvatar();
        }

        public AvatarControlViewModel(Message message)
        {
            this.Message = message;
            _ = LoadAvatar();
        }

        public AvatarControlViewModel(Member member, ImageDownloader imageDownloader)
        {
            this.Member = member;
            this.ImageDownloader = imageDownloader;
            _ = LoadAvatar();
        }

        public IMessageContainer MessageContainer { get; }
        public Message Message { get; }
        public Member Member { get; }

        public ImageDownloader ImageDownloader { get; }

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
            bool isSquare = false;

            byte[] image;
            if (this.MessageContainer != null)
            {
                image = await this.MessageContainer.DownloadAvatar();

                if (this.MessageContainer is Group)
                {
                    isSquare = true;
                }
                else if (this.MessageContainer is Chat)
                {
                    isSquare = false;
                }
            }
            else if (this.Message != null)
            {
                var downloader = this.Message.ImageDownloader;
                image = await downloader.DownloadAvatarImage(this.Message.AvatarUrl);
                isSquare = false;
            }
            else if (this.Member != null && this.ImageDownloader != null)
            {
                image = await this.ImageDownloader.DownloadAvatarImage(this.Member.ImageOrAvatarUrl);
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
