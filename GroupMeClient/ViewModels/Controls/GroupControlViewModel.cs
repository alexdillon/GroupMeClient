using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    public class GroupControlViewModel : ViewModelBase
    {
        public GroupControlViewModel()
        {
        }

        public GroupControlViewModel(IMessageContainer messageContainer)
        {
            this.MessageContainer = messageContainer;
            this.Avatar = new AvatarControlViewModel(this.MessageContainer, this.MessageContainer.Client.ImageDownloader);
        }

        private IMessageContainer messageContainer;
        private AvatarControlViewModel avatar;

        public ICommand GroupSelected { get; set; }

        public IMessageContainer MessageContainer
        {
            get { return this.messageContainer; }
            set
            {
                Set(() => this.MessageContainer, ref messageContainer, value);
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
                var latestPreviewMessage = this.MessageContainer.LatestMessage;

                var sender = latestPreviewMessage.Name;
                var attachments = latestPreviewMessage.Attachments;
                var message = latestPreviewMessage.Text;

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
                    return $"{sender}: {message}";
                }
            }
        }

        public string Title => this.MessageContainer.Name;

        public DateTime LastUpdated => this.MessageContainer.UpdatedAtTime;

        public string Id => this.MessageContainer.Id;

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