using System;
using System.IO;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="SendImageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.SendImageControl"/> control.
    /// </summary>
    public class SendImageControlViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private string typedMessageContents;
        private bool isSending;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendImageControlViewModel"/> class.
        /// </summary>
        public SendImageControlViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the command to be performed when the message is ready to send.
        /// </summary>
        public ICommand SendMessage { get; set; }

        /// <summary>
        /// Gets or sets the image stream to preview the image.
        /// </summary>
        public Stream ImageStream { get; set; }

        /// <summary>
        /// Gets or sets the message the user has composed to send.
        /// </summary>
        public string TypedMessageContents
        {
            get { return this.typedMessageContents; }
            set { this.Set(() => this.TypedMessageContents, ref this.typedMessageContents, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sending animation should be displayed.
        /// </summary>
        public bool IsSending
        {
            get { return this.isSending; }
            set { this.Set(() => this.IsSending, ref this.isSending, value); }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            this.ImageStream.Close();
            this.ImageStream.Dispose();
        }
    }
}
