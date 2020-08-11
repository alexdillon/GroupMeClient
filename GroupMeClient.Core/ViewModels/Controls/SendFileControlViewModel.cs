using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="SendFileControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.SendFileControl"/> control.
    /// </summary>
    public class SendFileControlViewModel : SendContentControlViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendFileControlViewModel"/> class.
        /// </summary>
        public SendFileControlViewModel()
        {
            this.SendButtonClicked = new RelayCommand(async () => await this.Send(), () => !this.IsSending);
        }

        /// <summary>
        /// Gets the command to be executed when the send button is clicked.
        /// </summary>
        public ICommand SendButtonClicked { get; }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        public string FileName { get; set; }

        private CancellationTokenSource UploadCancellationSource { get; set; }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.UploadCancellationSource?.Cancel();
            base.Dispose();
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

                var attachment = await GroupMeClientApi.Models.Attachments.FileAttachment.CreateFileAttachment(
                    this.FileName,
                    file,
                    this.MessageContainer,
                    this.UploadCancellationSource);

                if (this.SendMessage.CanExecute(attachment))
                {
                    this.SendMessage.Execute(attachment);
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
