using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
            get
            {
                return this.message;
            }

            set
            {
                if (this.message == value)
                {
                    return;
                }

                this.message = value;
                RaisePropertyChanged("Message");
            }
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
            get
            {
                return imageAttachment;
            }

            set
            {
                if (value == imageAttachment)
                {
                    return;
                }

                imageAttachment = value;
                RaisePropertyChanged("ImageAttachment");
            }
        }

        private ImageSource avatar;

        /// <summary>
        /// Gets the image that should be used for rounded avatars.
        /// </summary>
        public ImageSource AvatarRound
        {
            get
            {
                return avatar;
            }

            set
            {
                if (value == avatar)
                {
                    return;
                }

                avatar = value;
                RaisePropertyChanged("AvatarRound");
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
