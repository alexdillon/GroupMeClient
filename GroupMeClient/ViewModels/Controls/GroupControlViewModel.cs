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

        public string LastUpdatedFriendlyTime
        {
            get
            {
                var elapsedTime = DateTime.Now.Subtract(this.Group.UpdatedAtTime).Duration();
                if (elapsedTime < TimeSpan.FromDays(1))
                {
                    return this.Group.UpdatedAtTime.ToShortTimeString();
                }
                else
                {
                    return this.Group.UpdatedAtTime.ToString("MMM d");
                }
            }
        }

        public string QuickPreview
        {
            get
            {
                var sender = this.Group.MsgPreview.Preview.Nickname;

                bool wasImageSent = false;
                foreach (var attachment in this.Group.MsgPreview.Preview.Attachments)
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
                    var message = this.Group.MsgPreview.Preview.Text;
                    return $"{sender}: {message}";
                }
            }
        }
    }
}
