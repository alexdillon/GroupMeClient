﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GroupMeClient.Core.Controls.Media;
using GroupMeClient.Core.Services;
using GroupMeClientApi;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="GenericLinkAttachmentControlViewModel"/> provides a ViewModel for controls to display a generic webpage attachment.
    /// </summary>
    public class GenericLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        private GenericImageSource faviconImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericLinkAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="url">The url of the attached website.</param>
        /// <param name="imageDownloader">The downloader to use when retreiving data.</param>
        public GenericLinkAttachmentControlViewModel(string url, ImageDownloader imageDownloader)
            : base(url, imageDownloader)
        {
            this.Clicked = new RelayCommand(this.ClickedAction);
            this.CopyLink = new RelayCommand(this.CopyLinkAction);
        }

        /// <summary>
        /// Gets the website title.
        /// </summary>
        public string Title => this.LinkInfo?.Title;

        /// <summary>
        /// Gets the website short URL name.
        /// </summary>
        public string Site => this.Uri?.Host;

        /// <summary>
        /// Gets the action to occur when the website is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <summary>
        /// Gets the action to occur to copy the website link URL.
        /// </summary>
        public ICommand CopyLink { get; }

        /// <summary>
        /// Gets the favicon image.
        /// </summary>
        public GenericImageSource FaviconImage
        {
            get => this.faviconImage;
            private set => this.SetProperty(ref this.faviconImage, value);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            // Not needed - no unmanaged resources
        }

        /// <inheritdoc/>
        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImageAsync(this.LinkInfo.AnyPreviewPictureUrl, 350, 300);
            _ = this.DownloadFaviconImage(this.LinkInfo.Favicon);
            this.OnPropertyChanged(string.Empty);
        }

        private async Task DownloadFaviconImage(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var result = await this.ImageDownloader.DownloadByteDataAsync(url);
                    this.FaviconImage = new GenericImageSource(result);
                }
            }
            catch (Exception)
            {
            }
        }

        private void ClickedAction()
        {
            var osService = Ioc.Default.GetService<IOperatingSystemUIService>();
            osService.OpenWebBrowser(this.Url);
        }

        private void CopyLinkAction()
        {
            var clipboardService = Ioc.Default.GetService<IClipboardService>();
            clipboardService.CopyText(this.Url);
        }
    }
}
