using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Utilities;
using GroupMeClientApi;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="GroupMeImageAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.GroupMeImageAttachmentControl"/> control.
    /// </summary>
    public class GroupMeImageAttachmentControlViewModel : ViewModelBase, IDisposable
    {
        private byte[] imageData;
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeImageAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="attachment">The attachment to display.</param>
        /// <param name="groupId">The ID of the Group or Chat this image's parent message is in.</param>
        /// <param name="imageDownloader">The downloader to use for loading the image.</param>
        /// <param name="previewMode">The resolution in which to download and render the image.</param>
        public GroupMeImageAttachmentControlViewModel(ImageAttachment attachment, string groupId, ImageDownloader imageDownloader, GroupMeImageDisplayMode previewMode = GroupMeImageDisplayMode.Large)
        {
            this.ImageAttachment = attachment;
            this.GroupOrChatId = groupId;
            this.ImageDownloader = imageDownloader;
            this.Clicked = new RelayCommand(this.ClickedAction);
            this.PreviewMode = previewMode;

            this.IsLoading = true;
            this.GenerateSizedPlaceholder();
            Task.Run(this.LoadImageAttachment);
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
        public Stream ImageAttachmentStream
        {
            get
            {
                if (this.ImageData == null)
                {
                    return null;
                }
                else
                {
                    return new ReadOnlyByteStream(this.ImageData);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the loading animation should be displayed.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.Set(() => this.IsLoading, ref this.isLoading, value);
        }

        private ImageAttachment ImageAttachment { get; }

        private GroupMeImageDisplayMode PreviewMode { get; }

        private ImageDownloader ImageDownloader { get; }

        private string GroupOrChatId { get; }

        private byte[] ImageData
        {
            get => this.imageData;
            set
            {
                this.imageData = value;
                this.RaisePropertyChanged(nameof(this.ImageAttachmentStream));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // No unmanaged image data anymore
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
            if (image != null)
            {
                this.IsLoading = false;
                this.ImageData = image;
            }
        }

        private void ClickedAction()
        {
            var vm = new ViewImageControlViewModel(this.ImageAttachment.Url, this.ImageDownloader);

            var request = new Messaging.DialogRequestMessage(vm, destination: this.GroupOrChatId);
            Messenger.Default.Send(request);
        }

        private void GenerateSizedPlaceholder()
        {
            // Assign a dummy image of the same size to allow for immediate layout
            // operations to be completed accurately before the full image loads
            var dimensions = this.GetScaledImageDimensions();
            var imageService = SimpleIoc.Default.GetInstance<IImageService>();
            this.ImageData = imageService.CreateTransparentPng(dimensions.Item1, dimensions.Item2);
        }

        private Tuple<int, int> GetScaledImageDimensions()
        {
            if (this.PreviewMode == GroupMeImageDisplayMode.Preview)
            {
                return new Tuple<int, int>(200, 200);
            }

            var choppedUrl = new Uri(this.ImageAttachment.Url).AbsolutePath.Substring(1).Split('.')[0];
            var dimensionsStr = choppedUrl.Split('x');

            int.TryParse(dimensionsStr[0], out var width);
            int.TryParse(dimensionsStr[1], out var height);

            // GroupMe in large mode limits images to 960px in the largest dimensions. Small mode is not documented for the limits.
            const int MaxImageDim = 960;

            var largestSide = Math.Max(width, height);
            var scale = Math.Min(1.0, (double)MaxImageDim / largestSide);

            return new Tuple<int, int>((int)(width * scale), (int)(height * scale));
        }
    }
}
