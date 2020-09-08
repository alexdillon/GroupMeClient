using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core.Controls;
using GroupMeClient.Core.Services;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="SendImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.SendImageControl"/> control.
    /// </summary>
    public class SendImageControlViewModel : SendContentControlViewModelBase, IDragDropPasteTarget
    {
        private SendableImage selectedImage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendImageControlViewModel"/> class.
        /// </summary>
        public SendImageControlViewModel()
        {
            this.SendButtonClicked = new RelayCommand(async () => await this.Send(), () => !this.IsSending);

            this.ImagesCollection = new ObservableCollection<SendableImage>();
            this.AddImage = new RelayCommand(this.AddBlankImage);
            this.DeleteImage = new RelayCommand(this.DeleteSelectedImage);
        }

        /// <summary>
        /// Gets the collection of images that are being sent.
        /// </summary>
        public ObservableCollection<SendableImage> ImagesCollection { get; }

        /// <summary>
        /// Gets the command to be executed when the send button is clicked.
        /// </summary>
        public ICommand SendButtonClicked { get; }

        /// <summary>
        /// Gets the command to be executed to attach another image.
        /// </summary>
        public ICommand AddImage { get; }

        /// <summary>
        /// Gets the command to be executed to remove the selected image.
        /// </summary>
        public ICommand DeleteImage { get; }

        /// <summary>
        /// Gets or sets the image that is currently selected.
        /// </summary>
        public SendableImage SelectedImage
        {
            get => this.selectedImage;
            set => this.Set(() => this.SelectedImage, ref this.selectedImage, value);
        }

        /// <inheritdoc/>
        public override bool HasContents => this.ImagesCollection.Count > 0;

        /// <inheritdoc />
        public override void Dispose()
        {
            foreach (var image in this.ImagesCollection)
            {
                image.ImageStream?.Close();
                image.ImageStream?.Dispose();
            }
        }

        /// <inheritdoc/>
        void IDragDropPasteTarget.OnFileDrop(string[] filepaths)
        {
        }

        /// <inheritdoc/>
        void IDragDropPasteTarget.OnImageDrop(byte[] image)
        {
            if (this.SelectedImage.ImageStream == null)
            {
                this.SelectedImage.LoadFromStream(new MemoryStream(image));
            }
        }

        private async Task Send()
        {
            var attachments = new List<Attachment>();

            foreach (var sendableImage in this.ImagesCollection)
            {
                byte[] imageBytes;

                using (var ms = new MemoryStream())
                {
                    sendableImage.ImageStream.Seek(0, SeekOrigin.Begin);
                    await sendableImage.ImageStream.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }

                this.IsSending = true;

                var attachment = await ImageAttachment.CreateImageAttachment(imageBytes, this.MessageContainer);
                attachments.Add(attachment);
            }

            if (this.SendMessage.CanExecute(attachments))
            {
                this.SendMessage.Execute(attachments);
            }
        }

        private void AddBlankImage()
        {
            this.ImagesCollection.Add(new SendableImage());
            this.SelectedImage = this.ImagesCollection.Last();
        }

        private void DeleteSelectedImage()
        {
            int removeIndex = this.ImagesCollection.IndexOf(this.SelectedImage);
            this.ImagesCollection.RemoveAt(removeIndex);

            if (removeIndex > 0)
            {
                this.SelectedImage = this.ImagesCollection[removeIndex - 1];
            }
            else
            {
                this.SelectedImage = this.ImagesCollection[0];
            }
        }

        /// <summary>
        /// <see cref="SendableImage"/> represents an image that is being prepared to be attached to a message and sent.
        /// </summary>
        public class SendableImage : ViewModelBase, IDisposable
        {
            private Stream imageStream;

            /// <summary>
            /// Initializes a new instance of the <see cref="SendableImage"/> class.
            /// </summary>
            /// <param name="data">The image data to send.</param>
            public SendableImage(Stream data)
                : base()
            {
                this.ImageStream = data;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SendableImage"/> class.
            /// </summary>
            public SendableImage()
            {
                this.SelectImageFile = new RelayCommand(this.LoadFromFile);
            }

            /// <summary>
            /// Gets the raw image data.
            /// </summary>
            public Stream ImageStream
            {
                get => this.imageStream;
                private set => this.Set(() => this.ImageStream, ref this.imageStream, value);
            }

            /// <summary>
            /// Gets the command to execute to load image data from a file.
            /// </summary>
            public ICommand SelectImageFile { get; }

            /// <inheritdoc/>
            public void Dispose()
            {
                ((IDisposable)this.ImageStream).Dispose();
            }

            /// <summary>
            /// Loads image data from an existing stream.
            /// </summary>
            /// <param name="stream">The stream to load image data from.</param>
            public void LoadFromStream(Stream stream)
            {
                this.ImageStream = stream;
            }

            private void LoadFromFile()
            {
                var supportedImages = ImageAttachment.SupportedExtensions.ToList();

                var fileDialogService = SimpleIoc.Default.GetInstance<IFileDialogService>();
                var filters = new List<FileFilter>
                {
                    new FileFilter() { Name = "Images", Extensions = supportedImages },
                };

                var filename = fileDialogService.ShowOpenFileDialog("Select Image", filters);
                if (!string.IsNullOrEmpty(filename))
                {
                    var extension = Path.GetExtension(filename);
                    if (supportedImages.Contains(extension))
                    {
                        this.ImageStream = File.OpenRead(filename);
                    }
                }
            }
        }
    }
}
