using System;
using System.IO;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;

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
            set { Set(() => this.TypedMessageContents, ref typedMessageContents, value); }
        }

        public bool IsSending
        {
            get { return this.isSending; }
            set { Set(() => this.IsSending, ref isSending, value); }
        }

        void IDisposable.Dispose()
        {
            this.ImageStream.Close();
            this.ImageStream.Dispose();
        }
    }
}
