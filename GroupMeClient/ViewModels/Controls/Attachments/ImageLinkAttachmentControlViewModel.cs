using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public class ImageLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public ImageLinkAttachmentControlViewModel(string url)
        {
            this.Url = url;

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
