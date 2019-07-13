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
        public AvatarControlViewModel(IAvatarSource avatarSource, ImageDownloader imageDownloader)
        {
            this.AvatarSource = avatarSource;
            this.ImageDownloader = imageDownloader;
            _ = LoadAvatar();
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
            byte[] image = await this.ImageDownloader.DownloadAvatarImage(this.AvatarSource.ImageOrAvatarUrl);

            using (var ms = new System.IO.MemoryStream(image))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

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
}
