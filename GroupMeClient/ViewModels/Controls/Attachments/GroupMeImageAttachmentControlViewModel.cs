using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
        /// <param name="imageDownloader">The downloader to use for loading the image.</param>
        /// <param name="lowQualityPreview">Low quality preview lowers the resolution of attachments but increases performance.</param>
        public GroupMeImageAttachmentControlViewModel(ImageAttachment attachment, ImageDownloader imageDownloader, bool lowQualityPreview = false)
            : base(imageDownloader)
        {
            this.ImageAttachment = attachment;

            this.Clicked = new RelayCommand(this.ClickedAction);

            this.LowQualityPreview = lowQualityPreview;

            _ = this.LoadImageAttachment();
        }

        /// <summary>
        /// Gets the command to be performed when the image is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <summary>
        /// Gets the attached image.
        /// </summary>
        public System.IO.Stream ImageAttachmentStream
        {
            get { return this.imageAttachmentStream; }
            internal set { this.Set(() => this.ImageAttachmentStream, ref this.imageAttachmentStream, value); }
        }

        private ImageAttachment ImageAttachment { get; }

        private bool LowQualityPreview { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            ((IDisposable)this.imageAttachmentStream)?.Dispose();
        }

        /// <inheritdoc/>
        protected override void MetadataDownloadCompleted()
        {
            // not really a link class, but still need this method.
        }

        private async Task LoadImageAttachment()
        {
            byte[] image = null;

            var resolution = this.LowQualityPreview ? "small" : "large";

            image = await this.ImageDownloader.DownloadPostImageAsync($"{this.ImageAttachment.Url}.{resolution}");

            if (image == null)
            {
                return;
            }

            this.ImageAttachmentStream = new System.IO.MemoryStream(image);
        }

        private void ClickedAction()
        {
            var vm = new ViewImageControlViewModel(this.ImageAttachment, this.ImageDownloader);

            var request = new Messaging.DialogRequestMessage(vm);
            Messenger.Default.Send(request);
        }
    }
}
