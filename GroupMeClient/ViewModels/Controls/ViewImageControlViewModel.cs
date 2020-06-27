using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi;
using Microsoft.Win32;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ViewImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ViewImageControl"/> control.
    /// </summary>
    public class ViewImageControlViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private Stream imageAttachmentStream;
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
            get => this.imageAttachmentStream;
            internal set => this.Set(() => this.ImageStream, ref this.imageAttachmentStream, value);
        }

        /// <summary>
        /// Gets a value indicating whether the loading animation should be displayed.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.Set(() => this.IsLoading, ref this.isLoading, value);
        }

        /// <summary>
        /// Gets a value indicating how far clockwise the image should be rotated, expressed in degrees.
        /// </summary>
        public double RotateAngle
        {
            get => this.rotateAngle;
            private set => this.Set(() => this.RotateAngle, ref this.rotateAngle, value);
        }

        private string ImageUrl { get; }

        private ImageDownloader ImageDownloader { get; }

        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            (this.imageAttachmentStream as IDisposable)?.Dispose();
        }

        private async Task LoadImageAttachment()
        {
            var image = await this.ImageDownloader.DownloadPostImageAsync(this.ImageUrl);

            if (image == null)
            {
                return;
            }

            this.ImageStream = new MemoryStream(image);
            this.IsLoading = false;
        }

        private void SaveImageAction()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            var imageUrlWithoutLongId = this.ImageUrl.Substring(0, this.ImageUrl.LastIndexOf('.'));
            var extension = System.IO.Path.GetExtension(imageUrlWithoutLongId);
            var filter = $"Image (*{extension})|*{extension}";

            saveFileDialog.Filter = filter;

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fs = File.OpenWrite(saveFileDialog.FileName))
                {
                    this.ImageStream.Seek(0, SeekOrigin.Begin);
                    this.ImageStream.CopyTo(fs);
                }
            }
        }

        private void CopyImageAction()
        {
            var ms = new MemoryStream();
            this.ImageStream.Seek(0, SeekOrigin.Begin);
            this.ImageStream.CopyTo(ms);

            var imageClipObject = new System.Windows.DataObject();
            imageClipObject.SetData("PNG", ms);
            imageClipObject.SetData(ms);
            imageClipObject.SetImage(Utilities.ImageUtils.BytesToImageSource(ms.ToArray()) as BitmapSource);

            System.Windows.Clipboard.SetDataObject(imageClipObject, true);

            ms.Dispose();
        }

        private void RotateImageAction()
        {
            this.RotateAngle += 90;
            this.RotateAngle %= 360;
        }
    }
}
