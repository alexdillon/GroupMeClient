using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GroupMeClientApi;
using GroupMeClientApi.Models.Attachments;
using Microsoft.Toolkit.Mvvm.Input;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="SendFileControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.SendFileControl"/> control.
    /// </summary>
    public class SendFileControlViewModel : SendContentControlViewModelBase
    {
        private int uploadPercentage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendFileControlViewModel"/> class.
        /// </summary>
        public SendFileControlViewModel()
        {
            this.SendButtonClicked = new AsyncRelayCommand(this.Send, () => !this.IsSending);
        }

        /// <summary>
        /// Gets or sets the data stream for the content that is being sent.
        /// </summary>
        public Stream ContentStream { get; set; }

        /// <summary>
        /// Gets the command to be executed when the send button is clicked.
        /// </summary>
        public ICommand SendButtonClicked { get; }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets the file upload progress as an integer percentage value.
        /// </summary>
        public int UploadPercentage
        {
            get => this.uploadPercentage;
            private set => this.SetProperty(ref this.uploadPercentage, value);
        }

        /// <inheritdoc/>
        public override bool HasContents => this.ContentStream != null;

        private CancellationTokenSource UploadCancellationSource { get; set; }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.UploadCancellationSource?.Cancel();
            this.ContentStream?.Close();
            this.ContentStream?.Dispose();
        }

        private async Task Send()
        {
            try
            {
                byte[] file;

                using (var ms = new MemoryStream())
                {
                    this.ContentStream.Seek(0, SeekOrigin.Begin);
                    await this.ContentStream.CopyToAsync(ms);
                    file = ms.ToArray();
                }

                this.IsSending = true;

                this.UploadCancellationSource = new CancellationTokenSource();

                var uploadProgress = new UploadProgress();

                // Only show progress if the file is larger than the block size
                // Otherwise the progress will immediately jump to 100%.
                if (file.Length > FileAttachment.DefaultUploadBlockSize)
                {
                    uploadProgress.BytesUploadedChanged += (e) =>
                    {
                        var percentage = e.BytesUploaded / (double)file.Length;
                        this.UploadPercentage = (int)(percentage * 100);
                    };
                }

                var attachment = await FileAttachment.CreateFileAttachment(
                    this.FileName,
                    file,
                    this.MessageContainer,
                    this.UploadCancellationSource,
                    uploadProgress);

                var attachmentList = new List<Attachment> { attachment };

                if (this.SendMessage.CanExecute(attachmentList))
                {
                    this.SendMessage.Execute(attachmentList);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
