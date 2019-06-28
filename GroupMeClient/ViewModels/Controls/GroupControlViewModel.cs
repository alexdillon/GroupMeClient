using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    public class GroupControlViewModel : ViewModelBase
    {
        public GroupControlViewModel()
        {
        }

        public GroupControlViewModel(Group group)
        {
            this.group = group;

            _ = LoadAvatar();
        }

        public GroupControlViewModel(Chat chat)
        {
            this.chat = chat;

            _ = LoadAvatar();
        }

        public ICommand GroupSelected { get; set; }

        private Group group;

        public Group Group
        {
            get
            {
                return this.group;
            }

            set
            {
                if (this.group == value)
                {
                    return;
                }

                this.group = value;
                RaisePropertyChanged("Group");
            }
        }

        private Chat chat;

        public Chat Chat
        {
            get
            {
                return this.chat;
            }

            set
            {
                if (this.chat == value)
                {
                    return;
                }

                this.chat = value;
                RaisePropertyChanged("Chat");
            }
        }

        public string LastUpdatedFriendlyTime
        {
            get
            {
                var updatedAtTime = this.LastUpdated;

                var elapsedTime = DateTime.Now.Subtract(updatedAtTime).Duration();
                if (elapsedTime < TimeSpan.FromDays(1))
                {
                    return updatedAtTime.ToShortTimeString();
                }
                else
                {
                    return updatedAtTime.ToString("MMM d");
                }
            }
        }

        public string QuickPreview
        {
            get
            {
                var sender = this.Group?.MsgPreview.Preview.Nickname ?? this.Chat?.LatestMessage.Name;
                var attachments = this.Group?.MsgPreview.Preview.Attachments ?? this.Chat?.LatestMessage.Attachments;

                bool wasImageSent = false;
                foreach (var attachment in attachments)
                {
                    if (attachment.GetType() == typeof(GroupMeClientApi.Models.Attachments.ImageAttachment))
                    {
                        wasImageSent = true;
                    }
                }

                if (wasImageSent)
                {
                    return $"{sender} shared an picture";
                }
                else
                {
                    var message = this.Group?.MsgPreview.Preview.Text ?? this.Chat?.LatestMessage.Text;
                    return $"{sender}: {message}";
                }
            }
        }

        public string Title
        {
            get
            {
                var title = this.Group?.Name ?? this.Chat?.OtherUser.Name;

                return title;
            }
        }

        public DateTime LastUpdated
        {
            get
            {
                return this.Group?.UpdatedAtTime ?? this.Chat?.UpdatedAtTime ?? DateTime.Now;
            }
        }

        public string Id
        {
            get
            {
                return this.Group?.Id ?? this.Chat?.Id;
            }
        }

        private ImageSource avatar;

        /// <summary>
        /// Gets the image that should be used for rounded avatars.
        /// If the avatar shouldn't be rounded, null is returned.
        /// </summary>
        public ImageSource AvatarRound
        {
            get
            {
                if (this.Chat != null)
                {
                    return avatar;
                }
                else
                {
                    return null;
                }
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


        /// <summary>
        /// Gets the image that should be used for square avatars.
        /// If the avatar shouldn't be rectangular, null is returned.
        /// </summary>
        public ImageSource AvatarSquare
        {
            get
            {
                if (this.Group != null)
                {
                    return avatar;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value == avatar)
                {
                    return;
                }

                avatar = value;
                RaisePropertyChanged("AvatarSquare");
            }
        }

        public async Task LoadAvatar()
        {
            System.Drawing.Image image;
            if (this.Group != null)
            {
                image = await this.Group.DownloadAvatar();
            }
            else if (this.Chat != null)
            {
                image = await this.Chat.DownloadAvatar();
            }
            else
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

                // set the avatar and make sure both updates fire
                // let the UI bind to the correct one
                this.avatar = bitmapImage;
                RaisePropertyChanged("AvatarSquare");
                RaisePropertyChanged("AvatarRound");
            }
        }
    }
}
