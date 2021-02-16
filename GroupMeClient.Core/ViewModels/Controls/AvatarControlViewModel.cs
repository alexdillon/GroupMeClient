using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GroupMeClient.Core.Controls.Media;
using GroupMeClientApi;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="AvatarControlViewModel"/> provides the ViewModel for a control to display a GroupMe Avatar.
    /// </summary>
    public class AvatarControlViewModel : ViewModelBase
    {
        private GenericImageSource avatarRound;
        private GenericImageSource avatarSquare;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarControlViewModel"/> class.
        /// </summary>
        /// <param name="avatarSource">The avatar that should be displayed.</param>
        /// <param name="imageDownloader">The downloader used to retreive the avatar.</param>
        /// <param name="fullQuality">Whether the full resolution avatar should be downloaded and rendered at full quality.</param>
        public AvatarControlViewModel(IAvatarSource avatarSource, ImageDownloader imageDownloader, bool fullQuality = false)
        {
            this.AvatarSource = avatarSource;
            this.ImageDownloader = imageDownloader;
            this.IsFullQuality = fullQuality;

            Task.Run(this.LoadAvatarAsync);
        }

        /// <summary>
        /// Gets the <see cref="IAvatarSource"/> this control is displaying.
        /// </summary>
        public IAvatarSource AvatarSource { get; }

        /// <summary>
        /// Gets the <see cref="ImageDownloader"/> that should be used to retreive avatars.
        /// </summary>
        public ImageDownloader ImageDownloader { get; }

        /// <summary>
        /// Gets a value indicating whether this avatar is rendered at full quality.
        /// </summary>
        public bool IsFullQuality { get; } = false;

        /// <summary>
        /// Gets the image that should be used for rounded avatars.
        /// If the avatar shouldn't be rounded, null is returned.
        /// </summary>
        public GenericImageSource AvatarRound
        {
            get => this.avatarRound;
            private set => this.Set(() => this.AvatarRound, ref this.avatarRound, value);
        }

        /// <summary>
        /// Gets the image that should be used for square avatars.
        /// If the avatar shouldn't be rectangular, null is returned.
        /// </summary>
        public GenericImageSource AvatarSquare
        {
            get => this.avatarSquare;
            private set => this.Set(() => this.AvatarSquare, ref this.avatarSquare, value);
        }

        /// <summary>
        /// Gets the URL that is currently rendering in the <see cref="AvatarControlViewModel"/>.
        /// </summary>
        public string CurrentlyRenderedUrl { get; private set; }

        /// <summary>
        /// Asychronously downloads the avatar image from GroupMe.
        /// </summary>
        /// <returns>A <see cref="Task"/> with the download status.</returns>
        public async Task LoadAvatarAsync()
        {
            var isGroup = !this.AvatarSource.IsRoundedAvatar;
            byte[] image;

            if (this.IsFullQuality)
            {
                if (string.IsNullOrEmpty(this.AvatarSource.ImageOrAvatarUrl))
                {
                    image = isGroup ?
                        this.ImageDownloader.GetDefaultGroupAvatar() :
                        this.ImageDownloader.GetDefaultPersonAvatar();
                }
                else
                {
                    image = await this.ImageDownloader.DownloadPostImageAsync(this.AvatarSource.ImageOrAvatarUrl);
                }
            }
            else
            {
                 image = await this.ImageDownloader.DownloadAvatarImageAsync(this.AvatarSource.ImageOrAvatarUrl, isGroup);
            }

            this.CurrentlyRenderedUrl = this.AvatarSource.ImageOrAvatarUrl;

            var bitmapImage = new GenericImageSource(image);

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
