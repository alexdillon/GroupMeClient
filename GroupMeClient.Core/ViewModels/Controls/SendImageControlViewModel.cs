using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="SendImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.SendImageControl"/> control.
    /// </summary>
    public class SendImageControlViewModel : SendContentControlViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendImageControlViewModel"/> class.
        /// </summary>
        public SendImageControlViewModel()
        {
            this.SendButtonClicked = new RelayCommand(async () => await this.Send(), () => !this.IsSending);
        }

        /// <summary>
        /// Gets the command to be executed when the send button is clicked.
        /// </summary>
        public ICommand SendButtonClicked { get; }

        private async Task Send()
        {
            byte[] image;

            using (var ms = new MemoryStream())
            {
                this.ContentStream.Seek(0, SeekOrigin.Begin);
                await this.ContentStream.CopyToAsync(ms);
                image = ms.ToArray();
            }

            this.IsSending = true;

            var attachment = await GroupMeClientApi.Models.Attachments.ImageAttachment.CreateImageAttachment(image, this.MessageContainer);

            if (this.SendMessage.CanExecute(attachment))
            {
                this.SendMessage.Execute(attachment);
            }
        }
    }
}
