using System;
using System.IO;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls
{
    public class SendImageControlViewModel : GalaSoft.MvvmLight.ViewModelBase, IDisposable
    {
        public SendImageControlViewModel()
        {
        }

        private string typedMessageContents;

        private bool isSending;

        public ICommand SendMessage { get; set; }

        public Stream ImageStream { get; set; }

        public string TypedMessageContents
        {
            get { return this.typedMessageContents; }
            set { this.Set(() => this.TypedMessageContents, ref this.typedMessageContents, value); }
        }

        public bool IsSending
        {
            get { return this.isSending; }
            set { this.Set(() => this.IsSending, ref this.isSending, value); }
        }

        void IDisposable.Dispose()
        {
            this.ImageStream.Close();
            this.ImageStream.Dispose();
        }
    }
}
