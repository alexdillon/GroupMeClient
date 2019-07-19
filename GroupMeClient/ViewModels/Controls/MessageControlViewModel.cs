using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.ViewModels.Controls.Attachments;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.ViewModels.Controls
{
    public class MessageControlViewModel : ViewModelBase, IDisposable
    {
        public MessageControlViewModel(Message message)
        {
            this.Message = message;

            this.Avatar = new AvatarControlViewModel(this.Message, this.Message.ImageDownloader);
            this.LikeAction = new RelayCommand(async () => { await this.LikeMessageActionAsync(); }, () => { return true; }, true);

            _ = this.LoadImageAttachment();
            this.LoadLinkPreview();
        }

        private Message message;
        private AvatarControlViewModel avatar;
        private string hiddenText = string.Empty;

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
                this.RaisePropertyChanged(string.Empty); // no property name to force every single property to be updated
            }
        }

        public void UpdateDisplay()
        {
            this.RaisePropertyChanged(string.Empty); // no property name to force every single property to be updated
        }

        public AvatarControlViewModel Avatar
        {
            get { return this.avatar; }
            set { this.Set(() => this.Avatar, ref this.avatar, value); }
        }

        public string Id => this.Message.Id;

        public string Sender => this.Message.Name;

        public ICommand LikeAction { get; }

        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(this.hiddenText))
                {
                    return this.Message.Text;
                }
                else
                {
                    return this.Message.Text.Replace(this.hiddenText, string.Empty);
                }
            }
        }

        public Brush GroupMeRedBrush { get; } = new SolidColorBrush(Color.FromRgb(247, 112, 112));
        public Brush GroupMeLightBlueBrush { get; } = new SolidColorBrush(Color.FromRgb(219, 244, 253));
        public Brush GroupMeLightGrayBrush { get; } = new SolidColorBrush(Color.FromRgb(247, 247, 247));

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
                    return this.GroupMeLightGrayBrush;
                }
            }
        }

        private System.IO.Stream imageAttachmentStream;

        /// <summary>
        /// Gets or sets the attached image if present.
        /// </summary>
        public System.IO.Stream ImageAttachmentStream
        {
            get { return this.imageAttachmentStream; }
            set { this.Set(() => this.ImageAttachmentStream, ref this.imageAttachmentStream, value); }
        }

        /// <summary>
        /// Gets or sets the attached items (Tweets, Web Links, Videos, etc.), if present.
        /// </summary>
        public ObservableCollection<LinkAttachmentBaseViewModel> AttachedItems { get; set; } = new ObservableCollection<LinkAttachmentBaseViewModel>();

        public MahApps.Metro.IconPacks.PackIconFontAwesomeKind LikeStatus
        {
            get
            {
                if (this.Message.FavoritedBy.Count > 0)
                {
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.HeartSolid;
                }
                else
                {
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.HeartRegular;
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

        public IEnumerable<AvatarControlViewModel> LikedByAvatars
        {
            get
            {
                foreach (var memberId in this.Message.FavoritedBy)
                {
                    // member is either a Group Member, Other Chat User, or This User
                    var member = this.Message.Group?.Members.FirstOrDefault(m => m.UserId == memberId) ??
                        (this.Message.Chat?.OtherUser.Id == memberId ? this.Message.Chat?.OtherUser : null) ??
                        ((this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI()).Id == memberId ? (this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI()) : null);

                    var liker = new AvatarControlViewModel(member, this.Message.ImageDownloader);
                    yield return liker;
                }
            }
        }

        private async Task LoadImageAttachment()
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

        private void LoadLinkPreview()
        {
            var text = this.Message.Text ?? string.Empty;
            if (text.Contains(" "))
            {
                // only look to see if the first chunk is a URL
                text = text.Substring(0, text.IndexOf(" "));
            }

            const string TwitterPrefixHttps = "https://twitter.com/";
            const string TwitterPrefixHttp = "http://twitter.com/";

            const string GroupMeVideoPrefixHttps = "https://v.groupme.com";

            string[] imageExtensions = { "png", "jpg", "jpeg", "gif", "bmp" };

            const string WebPrefixHttps = "https://";
            const string WebPrefixHttp = "http://";

            LinkAttachmentBaseViewModel vm;

            var linkExtension = text.Split('.').LastOrDefault();

            if (text.StartsWith(TwitterPrefixHttps) || text.StartsWith(TwitterPrefixHttp))
            {
                vm = new TwitterAttachmentControlViewModel(text);
                this.AttachedItems.Add(vm);
            }
            else if (text.StartsWith(GroupMeVideoPrefixHttps))
            {
                vm = new VideoAttachmentControlViewModel(text);
                this.AttachedItems.Add(vm);
            }
            else if (imageExtensions.Contains(linkExtension))
            {
                vm = new ImageLinkAttachmentControlViewModel(text);
                this.AttachedItems.Add(vm);
            }
            else if (text.StartsWith(WebPrefixHttps) || text.StartsWith(WebPrefixHttp))
            {
                vm = new GenericLinkAttachmentControlViewModel(text);
                this.AttachedItems.Add(vm);
            }
            else
            {
                return;
            }

            if (vm.Uri != null)
            {
                this.hiddenText = vm.Url;
                this.RaisePropertyChanged("Text");
            }
        }

        private async Task LikeMessageActionAsync()
        {
            var me = this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI();
            var alreadyLiked = this.Message.FavoritedBy.Contains(me.Id);

            if (alreadyLiked)
            {
                var success = await this.Message.UnlikeMessage();
                if (success)
                {
                    this.Message.FavoritedBy.Remove(me.Id);
                }
            }
            else
            {
                var success = await this.Message.LikeMessage();
                if (success)
                {
                    this.Message.FavoritedBy.Add(me.Id);
                }
            }

            this.RaisePropertyChanged("LikedByAvatars");
            this.RaisePropertyChanged("LikeCount");
            this.RaisePropertyChanged("LikeColor");
            this.RaisePropertyChanged("LikeStatus");
        }

        void IDisposable.Dispose()
        {
            ((IDisposable)this.imageAttachmentStream)?.Dispose();
        }
    }
}
