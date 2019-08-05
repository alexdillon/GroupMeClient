using System;
using System.IO;
using System.Threading.Tasks;
using GroupMeClientApi;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ViewImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ViewImageControl"/> control.
    /// </summary>
    public class ViewImageControlViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private Stream imageAttachmentStream;
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageControlViewModel"/> class.
        /// </summary>
        /// <param name="attachment">The attachment display.</param>
        /// <param name="downloader">The downloader to use.</param>
        public ViewImageControlViewModel(ImageAttachment attachment, ImageDownloader downloader)
        {
            this.ImageAttachment = attachment;
            this.ImageDownloader = downloader;

            this.IsLoading = true;
            _ = this.LoadImageAttachment();
        }

        /// <summary>
        /// Gets the attached image.
        /// </summary>
        public Stream ImageStream
        {
            get { return this.imageAttachmentStream; }
            internal set { this.Set(() => this.ImageStream, ref this.imageAttachmentStream, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the loading animation should be displayed.
        /// </summary>
        public bool IsLoading
        {
            get { return this.isLoading; }
            private set { this.Set(() => this.IsLoading, ref this.isLoading, value); }
        }

        private ImageAttachment ImageAttachment { get; }

        private ImageDownloader ImageDownloader { get; }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            ((IDisposable)this.imageAttachmentStream).Dispose();
        }

        private async Task LoadImageAttachment()
        {
            byte[] image = null;
            image = await this.ImageDownloader.DownloadPostImageAsync($"{this.ImageAttachment.Url}");

            if (image == null)
            {
                return;
            }

            this.ImageStream = new MemoryStream(image);
            this.IsLoading = false;
        }
    }
}
