using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.ViewModels.Controls
{
    public class MessageControlViewModel : ViewModelBase
    {
        public MessageControlViewModel(Message message)
        {
            this.Message = message;
            this.Avatar = new AvatarControlViewModel(this.Message);
            _ = LoadImageAttachment();
        }

        private Message message;
        private AvatarControlViewModel avatar;

        public Message Message
        {
            get { return this.message; }
            set { Set(() => this.Message, ref message, value); }
        }

        public AvatarControlViewModel Avatar
        {
            get { return this.avatar; }
            set { Set(() => this.Avatar, ref avatar, value); }
        }

        public string Id => this.Message.Id;

        public string Text => this.Message.Text;

        public string Sender => this.Message.Name;

        public Brush GroupMeRedBrush { get; } = new SolidColorBrush(Color.FromRgb(247, 112, 112));
        public Brush GroupMeLightBlueBrush { get; } = new SolidColorBrush(Color.FromRgb(219, 244, 253));

        public Brush MessageColor
        {
            get
            {
                var me = this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI();

                if (this.Message.UserId == me.UserId)
                {
                    return this.GroupMeLightBlueBrush;
                }
                else
                {
                    return Brushes.White;
                }
            }
        }

        private System.IO.Stream imageAttachmentStream;

        /// <summary>
        /// Gets the attached image if present.
        /// </summary>
        public System.IO.Stream ImageAttachmentStream
        {
            get { return imageAttachmentStream; }

            set { Set(() => this.ImageAttachmentStream, ref imageAttachmentStream, value); }
        }

        public MahApps.Metro.IconPacks.PackIconMaterialKind LikeStatus
        {
            get
            {
                if (this.Message.FavoritedBy.Count > 0)
                {
                    return MahApps.Metro.IconPacks.PackIconMaterialKind.Heart;
                }
                else
                {
                    return MahApps.Metro.IconPacks.PackIconMaterialKind.HeartOutline;
                }
            }
        }

        public Brush LikeColor
        {
            get
            {
                var me = this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI();

                if (this.Message.FavoritedBy.Contains(me.Id))
                {
                    return this.GroupMeRedBrush;
                }
                else
                {
                    return Brushes.Gray;
                }
            }
        }

        public string LikeCount
        {
            get
            {
                if (this.message.FavoritedBy.Count == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return this.Message.FavoritedBy.Count.ToString();
                }
            }
        }

        public async Task LoadImageAttachment()
        {
            byte[] image = null;
            foreach (var attachment in this.Message.Attachments)
            {
                if (attachment.GetType() == typeof(ImageAttachment))
                {
                    var imgAttach = attachment as ImageAttachment;
                    var downloader = this.Message.ImageDownloader;

                    image = await downloader.DownloadPostImage(imgAttach.Url);
                }
            }

            if (image == null)
            {
                return;
            }

            this.ImageAttachmentStream = new System.IO.MemoryStream(image);
        }
    }
}
