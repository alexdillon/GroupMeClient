﻿using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GroupMeClientApi;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="AvatarControlViewModel"/> provides the ViewModel for a control to display a GroupMe Avatar.
    /// </summary>
    public class AvatarControlViewModel : ViewModelBase
    {
        private ImageSource avatarRound;
        private ImageSource avatarSquare;

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

            _ = this.LoadAvatarAsync();
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
        public ImageSource AvatarRound
        {
            get { return this.avatarRound; }
            private set { this.Set(() => this.AvatarRound, ref this.avatarRound, value); }
        }

        /// <summary>
        /// Gets the image that should be used for square avatars.
        /// If the avatar shouldn't be rectangular, null is returned.
        /// </summary>
        public ImageSource AvatarSquare
        {
            get { return this.avatarSquare; }
            private set { this.Set(() => this.AvatarSquare, ref this.avatarSquare, value); }
        }

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
                image = await this.ImageDownloader.DownloadPostImageAsync(this.AvatarSource.ImageOrAvatarUrl);
            }
            else
            {
                image = await this.ImageDownloader.DownloadAvatarImageAsync(this.AvatarSource.ImageOrAvatarUrl, isGroup);
            }

            var bitmapImage = Utilities.ImageUtils.BytesToImageSource(image);

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
