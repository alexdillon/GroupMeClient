using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi;
using GroupMeClientApi.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace GroupMeClient.Core.Plugins.ViewModels
{
    /// <summary>
    /// <see cref="ImageDetailsControlViewModel"/> provides a ViewModel for the <see cref="ImageDetailsControl"/> control.
    /// </summary>
    public class ImageDetailsControlViewModel : ObservableObject, IDisposable
    {
        private bool isLoading;
        private Stream imageData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageDetailsControlViewModel"/> class.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> containing the image to be shown.</param>
        /// <param name="imageIndex">The index of this image attachment to display.</param>
        /// <param name="downloader">The <see cref="GroupMeClientApi.ImageDownloader"/> that should be used to download images.</param>
        /// <param name="gotoContextAction">The <see cref="Action"/> used to display this <see cref="Message"/> in-context in GMDC.</param>
        /// <param name="showPopupAction">The <see cref="Action"/> used to open popups to display the image viewer.</param>
        /// <param name="showNext">The <see cref="Action"/> used to navigate to the next image in the gallery.</param>
        /// <param name="showPrevious">The <see cref="Action"/> used to navigate to the previous image in the gallery.</param>
        public ImageDetailsControlViewModel(Message message, int imageIndex, ImageDownloader downloader, Action gotoContextAction, Action<ObservableObject> showPopupAction, Action showNext, Action showPrevious)
        {
            this.Message = message;
            this.ImageDownloader = downloader;
            this.ImageIndex = imageIndex;
            this.ShowPopupAction = showPopupAction;

            this.Clicked = new RelayCommand(this.ClickedAction);
            this.ShowNextImage = new RelayCommand(showNext);
            this.ShowPreviousImage = new RelayCommand(showPrevious);
            this.GotoContext = new RelayCommand(gotoContextAction);

            this.SenderAvatar = new AvatarControlViewModel(this.Message, this.ImageDownloader);
            this.ImageUrl = ImageGalleryWindowViewModel.GetAttachmentContentUrls(this.Message.Attachments, false)[this.ImageIndex];

            Task.Run(this.LoadImage);
        }

        /// <summary>
        /// Gets the <see cref="Message"/> containing the displayed image.
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// Gets the <see cref="AvatarControlViewModel"/> for the user who sent this image.
        /// </summary>
        public AvatarControlViewModel SenderAvatar { get; }

        /// <summary>
        /// Gets the command to be performed when the image is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <summary>
        /// Gets the command to show the next image in the gallery.
        /// </summary>
        public ICommand ShowNextImage { get; }

        /// <summary>
        /// Gets the command to show the previous image in the gallery.
        /// </summary>
        public ICommand ShowPreviousImage { get; }

        /// <summary>
        /// Gets the command to show the message in context in GroupMe Desktop Client.
        /// </summary>
        public ICommand GotoContext { get; }

        /// <summary>
        /// Gets a stream containing the image data to display.
        /// </summary>
        public Stream ImageData
        {
            get => this.imageData;
            private set => this.SetProperty(ref this.imageData, value);
        }

        /// <summary>
        /// Gets a value indicating whether the image is still loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetProperty(ref this.isLoading, value);
        }

        private ImageDownloader ImageDownloader { get; }

        private int ImageIndex { get; }

        private string ImageUrl { get; }

        private Action<ObservableObject> ShowPopupAction { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            ((IDisposable)this.ImageData)?.Dispose();
        }

        private async Task LoadImage()
        {
            this.IsLoading = true;

            var image = await this.ImageDownloader.DownloadPostImageAsync(this.ImageUrl);

            if (image == null)
            {
                return;
            }

            this.ImageData = new MemoryStream(image);

            this.IsLoading = false;
        }

        private void ClickedAction()
        {
            var vm = new ViewImageControlViewModel(this.ImageUrl, this.ImageDownloader);
            this.ShowPopupAction(vm);
        }
    }
}
