using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Core.Services;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.Core.ViewModels.Controls.Attachments
{
    /// <summary>
    /// <see cref="FileAttachmentControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.Attachments.FileAttachmentControl"/> control.
    /// </summary>
    public class FileAttachmentControlViewModel : ViewModelBase
    {
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentControlViewModel"/> class.
        /// </summary>
        /// <param name="file">The file to display.</param>
        /// <param name="message">The <see cref="Message"/> containing this attachment.</param>
        public FileAttachmentControlViewModel(FileAttachment file, Message message)
        {
            this.FileAttachment = file;
            this.Message = message;

            this.Clicked = new RelayCommand(async () => await this.ClickedAction(), true);
            this.SaveAs = new RelayCommand(async () => await this.SaveAction(), true);

            _ = this.LoadFileInfo();
        }

        /// <summary>
        /// Gets the contents of the Tweet.
        /// </summary>
        public string Text => $"{this.FileData?.FileName} ({BytesToString(this.FileData?.FileSize)})";

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

        private Message Message { get; }

        /// <summary>
        /// Converts a number of bytes to a human-readable size representation.
        /// </summary>
        /// <param name="byteCount">The number of bytes.</param>
        /// <returns>A human-readable size string.</returns>
        /// <remarks>Adapted from https://stackoverflow.com/a/4975942.</remarks>
        private static string BytesToString(string byteCount)
        {
            if (long.TryParse(byteCount, out var bytes))
            {
                return BytesToString(bytes);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Converts a number of bytes to a human-readable size representation.
        /// </summary>
        /// <param name="byteCount">The number of bytes.</param>
        /// <returns>A human-readable size string.</returns>
        /// <remarks>Adapted from https://stackoverflow.com/a/4975942.</remarks>
        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        private async Task LoadFileInfo()
        {
            this.FileData = await this.FileAttachment.GetFileData(this.Message);
            this.RaisePropertyChanged(string.Empty);
        }

        // TODO same as in MessageControl for likers
        // Limit to left click or other legal (aka not right) buttons

        private async Task ClickedAction()
        {
            this.IsLoading = true;
            var data = await this.FileAttachment.DownloadFileAsync(this.Message);

            var tempFile = Utilities.TempFileUtils.GetTempFileName(this.FileData.FileName);
            File.WriteAllBytes(tempFile, data);

            try
            {
                Process.Start(tempFile);
            }
            catch (Exception)
            {
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", tempFile));
            }

            this.IsLoading = false;
        }

        private async Task SaveAction()
        {
            var extension = FileAttachment.GroupMeDocumentMimeTypeMapper.MimeTypeToExtension(this.FileData.MimeType);

            var fileDialogService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IFileDialogService>();
            var filters = new List<FileFilter>
            {
                new FileFilter() { Name = "Document", Extensions = { extension } },
            };

            var filename = fileDialogService.ShowOpenFileDialog("Save Document", filters);
            if (!string.IsNullOrEmpty(filename))
            {
                this.IsLoading = true;
                var data = await this.FileAttachment.DownloadFileAsync(this.Message);

                using (var fs = File.OpenWrite(filename))
                {
                    fs.Write(data, 0, data.Length);
                }

                this.IsLoading = false;
            }
        }
    }
}
