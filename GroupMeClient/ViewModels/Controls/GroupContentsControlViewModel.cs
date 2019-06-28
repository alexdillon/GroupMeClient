using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GroupMeClient.ViewModels.Controls
{
    public class GroupContentsControlViewModel : ViewModelBase
    {
        public GroupContentsControlViewModel()
        {
            Messages = new ObservableCollection<MessageControlViewModel>();
        }

        public GroupContentsControlViewModel(Group group) : this()
        {
            this.group = group;

            _ = Loaded();
        }

        public GroupContentsControlViewModel(Chat chat) : this()
        {
            this.chat = chat;

            _ = Loaded();
        }

        private Group group;
        private Chat chat;

        public ICommand CloseGroup { get; set; }

        public ObservableCollection<MessageControlViewModel> Messages { get; } 

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

        public string Title
        {
            get
            {
                var title = this.Group?.Name ?? this.Chat?.OtherUser.Name;

                return title;
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

        private async Task Loaded()
        {
            // for the initial load, call ignore the return from the GetMessage call
            // and bind everything from the Messages list instead. New ones will be automatically added

            if (this.Group != null)
            {
                await group.GetMessagesAsync();
                foreach (var msg in group.Messages)
                {
                    if (!this.Messages.Any(m => m.Id == msg.Id))
                    {
                        this.Messages.Add(new MessageControlViewModel(msg));
                    }
                }
            } 
            else if (this.Chat != null)
            {
                await chat.GetMessagesAsync();
                foreach (var msg in chat.Messages)
                {
                    if (!this.Messages.Any(m => m.Id == msg.Id))
                    {
                        this.Messages.Add(new MessageControlViewModel(msg));
                    }
                }
            }

            await LoadAvatar();
        }

        public string Id
        {
            get
            {
                return this.Group?.Id ?? this.Chat?.Id;
            }
        }
        
    }
}
