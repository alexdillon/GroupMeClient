using System;
using System.Threading.Tasks;
using GroupMeClientApi;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="GroupMeImageAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.GroupMeImageAttachmentControl"/> control.
    /// </summary>
    public class GroupMeImageAttachmentControlViewModel : LinkAttachmentBaseViewModel, IDisposable
    {
        private System.IO.Stream imageAttachmentStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeImageAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="attachment">The attachment to display.</param>
        /// <param name="downloader">The downloader to use for loading the image.</param>
        public GroupMeImageAttachmentControlViewModel(ImageAttachment attachment, ImageDownloader downloader)
        {
            this.ImageAttachment = attachment;
            this.ImageDownloader = downloader;

            _ = this.LoadImageAttachment();
        }

        /// <summary>
        /// Gets or sets the attached image.
        /// </summary>
        public System.IO.Stream ImageAttachmentStream
        {
            get { return this.imageAttachmentStream; }
            set { this.Set(() => this.ImageAttachmentStream, ref this.imageAttachmentStream, value); }
        }

        private ImageAttachment ImageAttachment { get; }

        private ImageDownloader ImageDownloader { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            ((IDisposable)this.imageAttachmentStream).Dispose();
        }

        /// <inheritdoc/>
        protected override void MetadataDownloadCompleted()
        {
            // not really a link class, but still need this method.
        }

        private async Task LoadImageAttachment()
        {
            byte[] image = null;
            image = await this.ImageDownloader.DownloadPostImage($"{this.ImageAttachment.Url}.large");

            if (image == null)
            {
                return;
            }

            this.ImageAttachmentStream = new System.IO.MemoryStream(image);
        }
    }
}
