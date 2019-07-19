using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public class TwitterAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public TwitterAttachmentControlViewModel(string tweetUrl)
            : base(tweetUrl)
        {
            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        public string Sender => this.LinkInfo?.Name;

        public string Text => this.LinkInfo?.Text;

        public string Handle => this.LinkInfo?.ScreenName;

        public ICommand Clicked { get; }

        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImage(this.LinkInfo.ProfileImageUrl);
            this.RaisePropertyChanged(string.Empty);
        }

        private void ClickedAction()
        {
            Extensions.WebBrowserHelper.OpenUrl(this.Url);
        }
    }
}
