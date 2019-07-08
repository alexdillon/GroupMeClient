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
            this.Group = group;
            this.Avatar = new AvatarControlViewModel(this.Group);
        }

        public GroupControlViewModel(Chat chat)
        {
            this.Chat = chat;
            this.Avatar = new AvatarControlViewModel(this.Chat);
        }

        private Group group;
        private Chat chat;
        private AvatarControlViewModel avatar;

        public ICommand GroupSelected { get; set; }

        public Group Group
        {
            get { return this.group; }
            set
            {
                Set(() => this.Group, ref group, value);
                RaisePropertyChangeForAll();
            }
        }

        public Chat Chat
        {
            get { return this.chat; }
            set
            {
                Set(() => this.Chat, ref chat, value);
                RaisePropertyChangeForAll();
            }
        }

        public AvatarControlViewModel Avatar
        {
            get { return this.avatar; }
            set { Set(() => this.Avatar, ref avatar, value); }
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

        private void RaisePropertyChangeForAll()
        {
            // since RaisePropertyChanged(string.empty) doesn't seem to work correctly...
            RaisePropertyChanged(nameof(this.Avatar));
            RaisePropertyChanged(nameof(this.LastUpdatedFriendlyTime));
            RaisePropertyChanged(nameof(this.QuickPreview));
            RaisePropertyChanged(nameof(this.Title));
            RaisePropertyChanged(nameof(this.LastUpdated));
        }
    }
}