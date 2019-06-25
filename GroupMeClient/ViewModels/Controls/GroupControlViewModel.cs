using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
        }

        public GroupControlViewModel(Chat chat)
        {
            this.chat = chat;
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
    }
}
