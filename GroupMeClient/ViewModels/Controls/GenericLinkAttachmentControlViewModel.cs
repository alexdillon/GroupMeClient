using System;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using LinqToTwitter;
using System.IO;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls
{
    public class GenericLinkAttachmentControlViewModel : LinkAttachmentBaseViewModel
    {
        public GenericLinkAttachmentControlViewModel(string url) :
            base(url)
        {
        }

        protected override void MetadataDownloadCompleted()
        {
            
        }
    }
}
