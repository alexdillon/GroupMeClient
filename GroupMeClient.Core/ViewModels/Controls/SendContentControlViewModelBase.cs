using System;
using System.IO;
using System.Windows.Input;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="SendContentControlViewModelBase"/> provides a base ViewModel for dialogs that handle sending specialized content.
    /// </summary>
    public abstract class SendContentControlViewModelBase : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        private string typedMessageContents;
        private bool isSending;

        /// <summary>
        /// Gets or sets the command to be performed when the message is ready to send.
        /// </summary>
        public ICommand SendMessage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Group"/> or <see cref="Chat"/> to which this content is being sent.
        /// </summary>
        public IMessageContainer MessageContainer { get; set; }

        /// <summary>
        ///  Gets a value indicating whether this dialog has contents ready to send.
        /// </summary>
        public abstract bool HasContents { get; }

        /// <summary>
        /// Gets or sets the message the user has composed to send.
        /// </summary>
        public string TypedMessageContents
        {
            get => this.typedMessageContents;
            set => this.Set(() => this.TypedMessageContents, ref this.typedMessageContents, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the sending animation should be displayed.
        /// </summary>
        public bool IsSending
        {
            get => this.isSending;
            set => this.Set(() => this.IsSending, ref this.isSending, value);
        }

        /// <inheritdoc/>
        public abstract void Dispose();
    }
}
