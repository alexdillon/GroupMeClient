using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using Microsoft.Win32;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="FileAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.FileAttachmentControl"/> control.
    /// </summary>
    public class FileAttachmentControlViewModel : AttachmentViewModelBase
    {
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="file">The file to display.</param>
        /// <param name="messageContainer">The group that the file is contained within.</param>
        /// <param name="message">The <see cref="Message"/> containing this attachment.</param>
        public FileAttachmentControlViewModel(FileAttachment file, IMessageContainer messageContainer, Message message)
        {
            this.FileAttachment = file;
            this.MessageContainer = messageContainer;
            this.Message = message;

            this.Clicked = new RelayCommand<MouseButtonEventArgs>(async (x) => await this.ClickedAction(x), true);
            this.SaveAs = new RelayCommand(async () => await this.SaveAction(), true);

            _ = this.LoadFileInfo();
        }

        /// <summary>
        /// Gets the contents of the Tweet.
        /// </summary>
        public string Text => this.FileData?.FileName;

        /// <summary>
        /// Gets or sets a value indicating whether the file is currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.Set(() => this.IsLoading, ref this.isLoading, value);
        }

        /// <summary>
        /// Gets the command to be performed when the document is clicked.
        /// </summary>
        public ICommand Clicked { get; }

        /// <summary>
        /// Gets the command to be performed to save the document.
        /// </summary>
        public ICommand SaveAs { get; }

        private FileAttachment FileAttachment { get; }

        private FileAttachment.FileData FileData { get; set; }

        private IMessageContainer MessageContainer { get; set; }

        private Message Message { get; }

        private async Task LoadFileInfo()
        {
            this.FileData = await this.FileAttachment.GetFileData(this.Message);
            this.RaisePropertyChanged(string.Empty);
        }

        private async Task ClickedAction(MouseButtonEventArgs e)
        {
            if (e == null || e.LeftButton == MouseButtonState.Pressed)
            {
                this.IsLoading = true;
                var data = await this.FileAttachment.DownloadFileAsync(this.MessageContainer.Messages.First());

                var tempFile = Utilities.TempFileUtils.GetTempFileName(this.FileData.FileName);
                File.WriteAllBytes(tempFile, data);
                System.Diagnostics.Process.Start(tempFile);
                this.IsLoading = false;
            }
        }

        private async Task SaveAction()
        {
            var extension = FileAttachment.GroupMeDocumentMimeTypeMapper.MimeTypeToExtension(this.FileData.MimeType);

            var saveFileDialog = new SaveFileDialog();
            var filter = $"Document (*{extension})|*{extension}";
            saveFileDialog.FileName = this.FileData.FileName;
            saveFileDialog.Filter = filter;

            if (saveFileDialog.ShowDialog() == true)
            {
                this.IsLoading = true;
                var data = await this.FileAttachment.DownloadFileAsync(this.MessageContainer.Messages.First());

                using (var fs = File.OpenWrite(saveFileDialog.FileName))
                {
                    fs.Write(data, 0, data.Length);
                }

                this.IsLoading = false;
            }
        }
    }
}
