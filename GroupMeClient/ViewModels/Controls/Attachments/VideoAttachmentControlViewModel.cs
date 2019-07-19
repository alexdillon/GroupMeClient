using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public class VideoAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public VideoAttachmentControlViewModel(string url)
        {
            this.Url = url;

            if (this.Uri != null && this.Uri.Host == "v.groupme.com")
            {
                var newUri = this.Uri.AbsoluteUri;
                newUri = newUri.Replace(".mp4", ".jpg");

                _ = this.DownloadImage(newUri);
            }

            this.Clicked = new RelayCommand(this.ClickedAction);
        }

        public ICommand Clicked { get; }

        protected override void MetadataDownloadCompleted()
        {
        }

        private void ClickedAction()
        {
            Extensions.WebBrowserHelper.OpenUrl(this.Url);
        }
    }
}
