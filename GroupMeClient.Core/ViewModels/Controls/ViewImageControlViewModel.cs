using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Utilities;
using GroupMeClientApi;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ViewImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ViewImageControl"/> control.
    /// </summary>
    public class ViewImageControlViewModel : ObservableObject, IDisposable
    {
        private byte[] imageData;
        private bool isLoading;
        private double rotateAngle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewImageControlViewModel"/> class.
        /// </summary>
        /// <param name="imageUrl">The url of the image to display.</param>
        /// <param name="downloader">The downloader to use.</param>
        public ViewImageControlViewModel(string imageUrl, ImageDownloader downloader)
        {
            this.ImageUrl = imageUrl;
            this.ImageDownloader = downloader;

            this.SaveImage = new RelayCommand(this.SaveImageAction);
            this.CopyImage = new RelayCommand(this.CopyImageAction);
            this.RotateImage = new RelayCommand(this.RotateImageAction);

            this.IsLoading = true;
            _ = this.LoadImageAttachment();
        }

        /// <summary>
        /// Gets the action to be performed when the save image button is clicked.
        /// </summary>
        public ICommand SaveImage { get; }

        /// <summary>
        /// Gets the action to be performed when the copy image button is clicked.
        /// </summary>
        public ICommand CopyImage { get; }

        /// <summary>
        /// Gets the action to be performed when the roate image button is clicked.
        /// </summary>
        public ICommand RotateImage { get; }

        /// <summary>
        /// Gets the attached image.
        /// </summary>
        public Stream ImageStream
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
            private set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets a value indicating how far clockwise the image should be rotated, expressed in degrees.
        /// </summary>
        public double RotateAngle
        {
            get => this.rotateAngle;
            private set => this.SetProperty(ref this.rotateAngle, value);
        }

        private byte[] ImageData
        {
            get => this.imageData;
            set
            {
                this.imageData = value;
                this.OnPropertyChanged(nameof(this.ImageStream));
            }
        }

        private string ImageUrl { get; }

        private ImageDownloader ImageDownloader { get; }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            // No unmanaged image resources anymore
        }

        private async Task LoadImageAttachment()
        {
            var image = await this.ImageDownloader.DownloadPostImageAsync(this.ImageUrl);

            if (image == null)
            {
                return;
            }

            this.ImageData = image;
            this.IsLoading = false;
        }

        private void SaveImageAction()
        {
            var imageUrlWithoutLongId = this.ImageUrl.Substring(0, this.ImageUrl.LastIndexOf('.'));
            var extension = Path.GetExtension(imageUrlWithoutLongId);

            var fileDialogService = Ioc.Default.GetService<IFileDialogService>();
            var filters = new List<FileFilter>
            {
                new FileFilter() { Name = "Image", Extensions = { extension } },
            };

            var filename = fileDialogService.ShowSaveFileDialog("Save Attachment", filters);
            if (!string.IsNullOrEmpty(filename))
            {
                using (var fs = File.OpenWrite(filename))
                {
                    this.ImageStream.Seek(0, SeekOrigin.Begin);
                    this.ImageStream.CopyTo(fs);
                }
            }
        }

        private void CopyImageAction()
        {
            var clipboardService = Ioc.Default.GetService<IClipboardService>();

            var rawData = new MemoryStream();
            this.ImageStream.Seek(0, SeekOrigin.Begin);
            this.ImageStream.CopyTo(rawData);

            clipboardService.CopyImage(new Core.Controls.Media.GenericImageSource(rawData.ToArray()));
        }

        private void RotateImageAction()
        {
            this.RotateAngle += 90;
            this.RotateAngle %= 360;
        }
    }
}
