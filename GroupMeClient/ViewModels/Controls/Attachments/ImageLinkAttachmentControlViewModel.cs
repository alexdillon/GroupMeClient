using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls.Attachments
{
    public class ImageLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public ImageLinkAttachmentControlViewModel(string url)
        {
            this.Url = url;

            this.Clicked = new RelayCommand(ClickedAction);
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
