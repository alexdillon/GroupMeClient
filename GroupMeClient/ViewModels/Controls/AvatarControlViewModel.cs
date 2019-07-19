using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GroupMeClientApi;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    public class AvatarControlViewModel : ViewModelBase
    {
        public AvatarControlViewModel(IAvatarSource avatarSource, ImageDownloader imageDownloader)
        {
            this.AvatarSource = avatarSource;
            this.ImageDownloader = imageDownloader;
            _ = this.LoadAvatar();
        }

        public IAvatarSource AvatarSource { get; }
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
            set { this.Set(() => this.AvatarRound, ref this.avatarRound, value); }
        }

        /// <summary>
        /// Gets the image that should be used for square avatars.
        /// If the avatar shouldn't be rectangular, null is returned.
        /// </summary>
        public ImageSource AvatarSquare
        {
            get { return this.avatarSquare; }
            set { this.Set(() => this.AvatarSquare, ref this.avatarSquare, value); }
        }

        public async Task LoadAvatar()
        {
            byte[] image = await this.ImageDownloader.DownloadAvatarImage(this.AvatarSource.ImageOrAvatarUrl);

            var bitmapImage = Extensions.ImageUtils.BytesToImageSource(image);

            if (this.AvatarSource.IsRoundedAvatar)
            {
                this.AvatarRound = bitmapImage;
            }
            else
            {
                this.AvatarSquare = bitmapImage;
            }
        }
    }
}
