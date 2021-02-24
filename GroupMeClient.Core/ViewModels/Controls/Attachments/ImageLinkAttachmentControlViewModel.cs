using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Utilities;
using GroupMeClientApi;

namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="ImageLinkAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.ImageLinkAttachmentControl"/> control.
    /// </summary>
    public class ImageLinkAttachmentControlViewModel : ViewModelBase, IHidesTextAttachment, IDisposable
    {
        private byte[] imageData;
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageLinkAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="url">The URL of the image to display.</param>
        /// <param name="imageDownloader">The downloader to use when retreiving data.</param>
        /// <param name="navigateToUrl">The URL of the image to open in a web browser when the user clicks on it.</param>
        public ImageLinkAttachmentControlViewModel(string url, ImageDownloader imageDownloader, string navigateToUrl = null)
        {
            this.Url = url;
            this.NavigateToUrl = navigateToUrl;
            this.ImageDownloader = imageDownloader;

            this.Clicked = new RelayCommand(this.ClickedAction);
            this.CopyLink = new RelayCommand(this.CopyLinkAction);

            if (Uri.TryCreate(this.Url, UriKind.Absolute, out var uri))
            {
                // Hide the portion of the message if it really is a well-formed URL.
                this.HiddenText = uri.ToString();
            }

            this.IsLoading = true;
            Task.Run(this.LoadImageAttachment);
        }

        /// <summary>
        /// Gets the raw Url this control is displaying.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the command to be performed when the image is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <summary>
        /// Gets the action to occur to copy the website link URL.
        /// </summary>
        public ICommand CopyLink { get; }

        /// <summary>
        /// Gets a value indicating whether the loading animation should be displayed.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.Set(() => this.IsLoading, ref this.isLoading, value);
        }

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

        /// <inheritdoc/>
        public string HiddenText { get; private set; } = string.Empty;

        private string NavigateToUrl { get; }

        private ImageDownloader ImageDownloader { get; }

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
            this.ImageAttachmentStream?.Dispose();
        }

        private async Task LoadImageAttachment()
        {
            var image = await this.ImageDownloader.DownloadPostImageAsync($"{this.Url}");
            if (image != null)
            {
                this.ImageData = image;
                this.IsLoading = false;
            }
        }

        private void ClickedAction()
        {
            var osService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IOperatingSystemUIService>();
            var navigateUrl = !string.IsNullOrEmpty(this.NavigateToUrl) ? this.NavigateToUrl : this.Url;
            osService.OpenWebBrowser(navigateUrl);
        }

        private void CopyLinkAction()
        {
            var clipboardService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IClipboardService>();
            var navigateUrl = !string.IsNullOrEmpty(this.NavigateToUrl) ? this.NavigateToUrl : this.Url;
            clipboardService.CopyText(navigateUrl);
        }
    }
}
