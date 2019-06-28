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
        public MessageControlViewModel()
        {
        }

        public MessageControlViewModel(Message message)
        {
            this.message = message;
            _ = LoadImageAttachment();
            _ = LoadAvatar();
        }

        private Message message;

        public Message Message
        {
            get { return this.message; }
            set { Set(() => this.Message, ref message, value); }
        }

        public string Id => this.Message.Id;

        public string Text => this.Message.Text;

        public string Sender => this.Message.Name;

        public Color Color
        {
            get
            {
                if (this.Message.Group != null)
                {
                    // TODO decide who sent the message
                    return Colors.White;
                }
                else if (this.Message.Chat != null)
                {
                    return Colors.White;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        private ImageSource imageAttachment;

        /// <summary>
        /// Gets the attached image if present.
        /// </summary>
        public ImageSource ImageAttachment
        {
            get { return imageAttachment; }

            set { Set(() => this.ImageAttachment, ref imageAttachment, value); }
        }

        private ImageSource avatar;

        /// <summary>
        /// Gets the image that should be used for rounded avatars.
        /// </summary>
        public ImageSource AvatarRound
        {
            get { return avatar; }

            set { Set(() => this.AvatarRound, ref avatar, value); }
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

        public Brush GroupMeRedBrush { get; } = new SolidColorBrush(Color.FromRgb(247, 112, 112));

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
            System.Drawing.Image image = null;
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

            using (var ms = new System.IO.MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, System.IO.SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                this.ImageAttachment = bitmapImage;
            }
        }

        public async Task LoadAvatar()
        {
            var downloader = this.Message.ImageDownloader;
            var image = await downloader.DownloadAvatarImage(this.Message.AvatarUrl);

            using (var ms = new System.IO.MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                ms.Seek(0, System.IO.SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();

                this.AvatarRound = bitmapImage;
            }
        }
    }
}
