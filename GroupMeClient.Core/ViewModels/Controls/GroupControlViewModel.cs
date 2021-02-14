using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GroupMeClient.Core.Controls.Documents;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="GroupControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.GroupControl"/> control.
    /// </summary>
    public class GroupControlViewModel : ViewModelBase
    {
        private IMessageContainer messageContainer;
        private AvatarControlViewModel avatar;
        private int unreadMessagesCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupControlViewModel"/> class.
        /// </summary>
        /// <param name="messageContainer">The Group or Chat to show in this control.</param>
        public GroupControlViewModel(IMessageContainer messageContainer)
        {
            this.MessageContainer = messageContainer;
            this.Avatar = new AvatarControlViewModel(this.MessageContainer, this.MessageContainer.Client.ImageDownloader);
        }

        /// <summary>
        /// Gets or sets the command to be performed when this Group or Chat is clicked.
        /// </summary>
        public ICommand GroupSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this group is loaded purely from cache.
        /// </summary>
        public bool IsHistorical { get; set; }

        /// <summary>
        /// Gets the title of this Group or Chat.
        /// </summary>
        public string Title => this.MessageContainer.Name;

        /// <summary>
        /// Gets the last updated time for this Group or Chat.
        /// </summary>
        public DateTime LastUpdated => this.MessageContainer.UpdatedAtTime;

        /// <summary>
        /// Gets the unique identifier for this Group or Chat.
        /// </summary>
        public string Id => this.MessageContainer.Id;

        /// <summary>
        /// Gets or sets the Container (Group or Chat) this control is displaying.
        /// </summary>
        public IMessageContainer MessageContainer
        {
            get
            {
                return this.messageContainer;
            }

            set
            {
                this.Set(() => this.MessageContainer, ref this.messageContainer, value);

                if (this.Avatar != null && this.MessageContainer.ImageOrAvatarUrl != this.Avatar.CurrentlyRenderedUrl)
                {
                    // Reload the avatar if the latest URL returned doesn't match what's currently rendered.
                    Task.Run(this.Avatar.LoadAvatarAsync);
                }

                this.RaisePropertyChangeForAll();
            }
        }

        /// <summary>
        /// Gets the avatar for this Group or Chat.
        /// </summary>
        public AvatarControlViewModel Avatar
        {
            get => this.avatar;
            private set => this.Set(() => this.Avatar, ref this.avatar, value);
        }

        /// <summary>
        /// Gets or sets the number of unread messages shown for this Group or Chat.
        /// </summary>
        public int TotalUnreadCount
        {
            get => this.unreadMessagesCounter;
            set => this.Set(() => this.TotalUnreadCount, ref this.unreadMessagesCounter, value);
        }

        /// <summary>
        /// Gets a string showing an easily readable timestamp for the last update to this Group or Chat.
        /// </summary>
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

        /// <summary>
        /// Gets a string showing the most recent post in this Group or Chat.
        /// </summary>
        public ObservableCollection<Inline> QuickPreview
        {
            get
            {
                var latestPreviewMessage = this.MessageContainer.LatestMessage;

                if (latestPreviewMessage == null)
                {
                    return new ObservableCollection<Inline>();
                }

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

                var result = new ObservableCollection<Inline>();

                if (wasImageSent)
                {
                    result.Add(new Run($"{sender} shared a picture"));
                }
                else
                {
                    result.Add(new Span(new Run(sender)) { FontWeight = Span.FontWeightOptions.SemiBold });
                    result.Add(new Run($": {message}"));
                }

                return result;
            }
        }

        private void RaisePropertyChangeForAll()
        {
            // since RaisePropertyChanged(string.empty) doesn't seem to work correctly...
            this.RaisePropertyChanged(nameof(this.Avatar));
            this.RaisePropertyChanged(nameof(this.LastUpdatedFriendlyTime));
            this.RaisePropertyChanged(nameof(this.QuickPreview));
            this.RaisePropertyChanged(nameof(this.Title));
            this.RaisePropertyChanged(nameof(this.LastUpdated));
        }
    }
}