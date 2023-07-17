using System.Threading.Tasks;
using GroupMeClient.Core.Controls.Media;
using GroupMeClientApi;
using GroupMeClientApi.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="AvatarControlViewModel"/> provides the ViewModel for a control to display a GroupMe Avatar.
    /// </summary>
    public class AvatarControlViewModel : ObservableObject
    {
        private GenericImageSource avatarImage;
        private bool isRound;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarControlViewModel"/> class.
        /// </summary>
        /// <param name="avatarSource">The avatar that should be displayed.</param>
        /// <param name="imageDownloader">The downloader used to retreive the avatar.</param>
        /// <param name="fullQuality">Whether the full resolution avatar should be downloaded and rendered at full quality.</param>
        public AvatarControlViewModel(IAvatarSource avatarSource, ImageDownloader imageDownloader, bool fullQuality = false)
        {
            this.OriginalSource = avatarSource;
            this.ImageDownloader = imageDownloader;
            this.IsFullQuality = fullQuality;

            Task.Run(this.LoadAvatarAsync);
        }

        /// <summary>
        /// Gets the <see cref="IAvatarSource"/> this control is displaying.
        /// </summary>
        public IAvatarSource OriginalSource { get; }

        /// <summary>
        /// Gets the <see cref="ImageDownloader"/> that should be used to retreive avatars.
        /// </summary>
        public ImageDownloader ImageDownloader { get; }

        /// <summary>
        /// Gets a value indicating whether this avatar is rendered at full quality.
        /// </summary>
        public bool IsFullQuality { get; } = false;

        /// <summary>
        /// Gets the avatar image.
        /// </summary>
        public GenericImageSource AvatarImage
        {
            get => this.avatarImage;
            private set => this.SetProperty(ref this.avatarImage, value);
        }

        /// <summary>
        /// Gets a value indicating whether this avatar is round.
        /// </summary>
        public bool IsRound
        {
            get => this.isRound;
            private set => this.SetProperty(ref this.isRound, value);
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
            var isGroup = !this.OriginalSource.IsRoundedAvatar;
            byte[] image;

            if (this.IsFullQuality)
            {
                if (string.IsNullOrEmpty(this.OriginalSource.ImageOrAvatarUrl))
                {
                    image = isGroup ?
                        this.ImageDownloader.GetDefaultGroupAvatar() :
                        this.ImageDownloader.GetDefaultPersonAvatar();
                }
                else
                {
                    image = await this.ImageDownloader.DownloadPostImageAsync(this.OriginalSource.ImageOrAvatarUrl);
                }
            }
            else
            {
                 image = await this.ImageDownloader.DownloadAvatarImageAsync(this.OriginalSource.ImageOrAvatarUrl, isGroup);
            }

            this.CurrentlyRenderedUrl = this.OriginalSource.ImageOrAvatarUrl;
            this.AvatarImage = new GenericImageSource(image);
            this.IsRound = this.OriginalSource.IsRoundedAvatar;
        }
    }
}
