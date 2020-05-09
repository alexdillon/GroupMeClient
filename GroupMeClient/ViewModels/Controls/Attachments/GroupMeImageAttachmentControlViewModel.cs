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
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeImageAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="attachment">The attachment to display.</param>
        /// <param name="imageDownloader">The downloader to use for loading the image.</param>
        /// <param name="previewMode">The resolution in which to download and render the image.</param>
        public GroupMeImageAttachmentControlViewModel(ImageAttachment attachment, ImageDownloader imageDownloader, GroupMeImageDisplayMode previewMode = GroupMeImageDisplayMode.Large)
            : base(imageDownloader)
        {
            this.ImageAttachment = attachment;
            this.Clicked = new RelayCommand(this.ClickedAction);
            this.PreviewMode = previewMode;

            this.IsLoading = true;
            _ = this.LoadImageAttachment();
        }

        /// <summary>
        /// Different resolution options in which GroupMe Attached Images can be rendered.
        /// </summary>
        public enum GroupMeImageDisplayMode
        {
            /// <summary>
            /// Large resolution image
            /// </summary>
            Large,

            /// <summary>
            /// Small resolution image. Original image is scaled down, but
            /// aspect ratio is preserved.
            /// </summary>
            Small,

            /// <summary>
            /// Small, square image. The original photo will be cropped to fit.
            /// </summary>
            Preview,
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

        /// <summary>
        /// Gets a value indicating whether the loading animation should be displayed.
        /// </summary>
        public bool IsLoading
        {
            get { return this.isLoading; }
            private set { this.Set(() => this.IsLoading, ref this.isLoading, value); }
        }

        private ImageAttachment ImageAttachment { get; }

        private GroupMeImageDisplayMode PreviewMode { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            (this.imageAttachmentStream as IDisposable)?.Dispose();
        }

        /// <inheritdoc/>
        protected override void MetadataDownloadCompleted()
        {
            // not really a link class, but still need this method.
        }

        private static string GetGroupMeImageDisplayModeString(GroupMeImageDisplayMode mode)
        {
            switch (mode)
            {
                case GroupMeImageDisplayMode.Large:
                    return "large";

                case GroupMeImageDisplayMode.Small:
                    return "small";

                case GroupMeImageDisplayMode.Preview:
                    return "preview";

                default:
                    return "large";
            }
        }

        private async Task LoadImageAttachment()
        {
            var resolution = GetGroupMeImageDisplayModeString(this.PreviewMode);

            var image = await this.ImageDownloader.DownloadPostImageAsync($"{this.ImageAttachment.Url}.{resolution}");

            if (image == null)
            {
                return;
            }

            this.ImageAttachmentStream = new System.IO.MemoryStream(image);
            this.IsLoading = false;
        }

        private void ClickedAction()
        {
            var vm = new ViewImageControlViewModel(this.ImageAttachment.Url, this.ImageDownloader);

            var request = new Messaging.DialogRequestMessage(vm);
            Messenger.Default.Send(request);
        }
    }
}
