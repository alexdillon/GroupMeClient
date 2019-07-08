using System;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using LinqToTwitter;
using System.IO;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls
{
    public class TwitterAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public TwitterAttachmentControlViewModel(string tweetUrl) :
            base(tweetUrl)
        {
        }

        public string Sender => this.LinkInfo?.Name;

        public string Text => this.LinkInfo?.Text;

        public string Handle => this.LinkInfo?.ScreenName;

        protected override void MetadataDownloadCompleted()
        {
            _ = this.DownloadImage(this.LinkInfo.ProfileImageUrl);
            RaisePropertyChanged("");
        }
    }
}
