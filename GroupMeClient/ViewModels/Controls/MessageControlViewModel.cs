using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.ViewModels.Controls.Attachments;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="MessageControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.MessageControl"/> control.
    /// </summary>
    public class MessageControlViewModel : MessageControlViewModelBase, IDisposable
    {
        private readonly object messageLock = new object();

        private Message message;
        private AvatarControlViewModel avatar;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageControlViewModel"/> class.
        /// </summary>
        /// <param name="message">The message to bind to this control.</param>
        /// <param name="lowQualityPreview">Low quality preview lowers the resolution of attachments but increases performance.</param>
        /// <param name="showLikers">Indicates whether the like status for a message should be displayed.</param>
        public MessageControlViewModel(Message message, bool lowQualityPreview = false, bool showLikers = true)
        {
            this.Message = message;

            this.Avatar = new AvatarControlViewModel(this.Message, this.Message.ImageDownloader);
            this.Inlines = new ObservableCollection<Inline>();
            this.LikeAction = new RelayCommand(async () => { await this.LikeMessageActionAsync(); }, () => { return true; }, true);

            this.LowQualityPreview = lowQualityPreview;
            this.ShowLikers = showLikers;

            this.LoadAttachments();
            this.LoadInlinesForMessageBody();
        }

        /// <summary>
        /// Gets the GroupMe Red Brush Color for showing messages liked by the user.
        /// </summary>
        public Brush GroupMeRedBrush { get; } = new SolidColorBrush(Color.FromRgb(247, 112, 112));

        /// <summary>
        /// Gets the GroupMe Light Blue Brush Color for showing messages sent by the user.
        /// </summary>
        public Brush GroupMeLightBlueBrush { get; } = new SolidColorBrush(Color.FromRgb(219, 244, 253));

        /// <summary>
        /// Gets the GroupMe Light Gray Brush Color for showing messages liked by others.
        /// </summary>
        public Brush GroupMeLightGrayBrush { get; } = new SolidColorBrush(Color.FromRgb(247, 247, 247));

        /// <summary>
        /// Gets or sets the attached items (Tweets, Web Links, Videos, etc.), if present.
        /// </summary>
        public ObservableCollection<LinkAttachmentBaseViewModel> AttachedItems { get; set; } = new ObservableCollection<LinkAttachmentBaseViewModel>();

        /// <summary>
        /// Gets the command to be performed when this <see cref="Message"/> is 'Liked'.
        /// </summary>
        public ICommand LikeAction { get; }

        /// <summary>
        /// Gets the unique identifier for this <see cref="Message"/>.
        /// </summary>
        public override string Id => this.Message.Id;

        /// <summary>
        /// Gets the sender of this <see cref="Message"/>.
        /// </summary>
        public string Sender => this.Message.Name;

        /// <inheritdoc />
        public override bool IsSelectable => true;

        /// <summary>
        /// Gets or sets the displayed <see cref="Message"/>.
        /// </summary>
        public override Message Message
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

                lock (this.messageLock)
                {
                    this.message = value;
                    this.UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// Gets avatar to use when displaying this <see cref="Message"/>.
        /// </summary>
        public AvatarControlViewModel Avatar
        {
            get { return this.avatar; }
            private set { this.Set(() => this.Avatar, ref this.avatar, value); }
        }

        /// <summary>
        /// Gets a collection of inline text blocks to display for the message.
        /// </summary>
        public ObservableCollection<Inline> Inlines { get; }

        /// <summary>
        /// Gets the background color to use when rendering this <see cref="Message"/>.
        /// </summary>
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

        /// <summary>
        /// Gets the icon to display for the like status of this <see cref="Message"/>.
        /// </summary>
        public MahApps.Metro.IconPacks.PackIconFontAwesomeKind LikeStatus
        {
            get
            {
                if (!this.ShowLikers)
                {
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.None;
                }
                else if (this.Message.FavoritedBy.Count > 0)
                {
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.HeartSolid;
                }
                else
                {
                    return MahApps.Metro.IconPacks.PackIconFontAwesomeKind.HeartRegular;
                }
            }
        }

        /// <summary>
        /// Gets the brush color to render the like icon with for this <see cref="Message"/>.
        /// </summary>
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

        /// <summary>
        /// Gets the number of people who have liked this <see cref="Message"/>.
        /// </summary>
        public string LikeCount
        {
            get
            {
                if (this.message.FavoritedBy.Count == 0 || !this.ShowLikers)
                {
                    return string.Empty;
                }
                else
                {
                    return this.Message.FavoritedBy.Count.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the avatars of the people who have liked this <see cref="Message"/>.
        /// </summary>
        public IEnumerable<AvatarControlViewModel> LikedByAvatars
        {
            get
            {
                // Lock on Message is needed to prevent the FavoritedBy collection from
                // changing mid-update if new 'like' notifications are delivered asyncronously.
                lock (this.messageLock)
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
        }

        private bool LowQualityPreview { get; }

        private bool ShowLikers { get; }

        private string HiddenText { get; set; }

        /// <summary>
        /// Redraw the message immediately.
        /// </summary>
        public override void UpdateDisplay()
        {
            this.RaisePropertyChanged(string.Empty); // no property name to force every single property to be updated
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            foreach (var attachment in this.AttachedItems)
            {
                (attachment as IDisposable).Dispose();
            }
        }

        private void LoadAttachments()
        {
            // Load GroupMe Image Attachments
            foreach (var attachment in this.Message.Attachments)
            {
                if (attachment is ImageAttachment imageAttach)
                {
                    var imageVm = new GroupMeImageAttachmentControlViewModel(imageAttach, this.Message.ImageDownloader, this.LowQualityPreview);
                    this.AttachedItems.Add(imageVm);
                    break;
                }
                else if (attachment is LinkedImageAttachment linkedImage)
                {
                    var imageLinkVm = new ImageLinkAttachmentControlViewModel(linkedImage.Url, this.Message.ImageDownloader, this.Message.Text);
                    this.AttachedItems.Add(imageLinkVm);

                    // Linked Images can't have captions, so hide the entire body
                    this.HiddenText = this.Message.Text;

                    // Don't allow any other attachment types to be included if a linked_image is.
                    return;
                }
            }

            // Load Link-Based Attachments (Tweets, Web Images, GroupMe Hosted Video, Websites, etc.)
            var text = this.Message.Text ?? string.Empty;
            if (text.Contains(" "))
            {
                // only look to see if the first chunk is a URL
                text = text.Substring(0, text.IndexOf(" "));
            }

            const string TwitterPrefixHttps = "https://twitter.com/";
            const string TwitterPrefixHttp = "http://twitter.com/";

            const string GroupMeVideoPrefixHttps = "https://v.groupme.com";
            const string GroupMeImageRegexHttps = @"https:\/\/i.groupme.com\/[0-99999]*x[0-99999]*\..*";

            string[] imageExtensions = { "png", "jpg", "jpeg", "gif", "bmp" };

            const string WebPrefixHttps = "https://";
            const string WebPrefixHttp = "http://";

            LinkAttachmentBaseViewModel vm;

            var linkExtension = text.Split('.').LastOrDefault();

            if (text.StartsWith(TwitterPrefixHttps) || text.StartsWith(TwitterPrefixHttp))
            {
                vm = new TwitterAttachmentControlViewModel(text, this.Message.ImageDownloader);
                this.AttachedItems.Add(vm);
            }
            else if (text.StartsWith(GroupMeVideoPrefixHttps))
            {
                vm = new VideoAttachmentControlViewModel(text, this.Message.ImageDownloader);
                this.AttachedItems.Add(vm);
            }
            else if (imageExtensions.Contains(linkExtension))
            {
                vm = new ImageLinkAttachmentControlViewModel(text, this.Message.ImageDownloader);
                this.AttachedItems.Add(vm);
            }
            else if (Regex.IsMatch(text, GroupMeImageRegexHttps))
            {
                var groupMeIUrl = Regex.Match(text, GroupMeImageRegexHttps).Value;
                vm = new ImageLinkAttachmentControlViewModel(groupMeIUrl, this.Message.ImageDownloader);
                this.AttachedItems.Add(vm);
            }
            else if (text.StartsWith(WebPrefixHttps) || text.StartsWith(WebPrefixHttp))
            {
                vm = new GenericLinkAttachmentControlViewModel(text, this.Message.ImageDownloader);
                this.AttachedItems.Add(vm);
            }
            else
            {
                return;
            }

            if (vm.Uri != null)
            {
                this.HiddenText = vm.Url;
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

        private void LoadInlinesForMessageBody()
        {
            var text = this.Message.Text ?? string.Empty;

            if (!string.IsNullOrEmpty(this.HiddenText))
            {
                text = text.Replace(this.HiddenText, string.Empty);
            }

            var inlinesTemp = new List<Inline>();
            var inlinesResult = new List<Inline>();
            inlinesTemp.Add(new Run(text));

            foreach (var part in inlinesTemp)
            {
                if (part is Run r)
                {
                    // scan this portion of the message for hyperlinks
                    inlinesResult.AddRange(this.ProcessHyperlinks(r));
                }
                else
                {
                    // this part of the message has already been processed, pass it through
                    inlinesResult.Add(part);
                }
            }

            /* TODO: Swap inlinesTemp and inlinesResult and repeat for addition special content types */

            this.Inlines.Clear();
            foreach (var result in inlinesResult)
            {
                this.Inlines.Add(result);
            }
        }

        private List<Inline> ProcessHyperlinks(Run run)
        {
            var result = new List<Inline>();
            var text = run.Text;

            while (true)
            {
                if (!Regex.IsMatch(text, Extensions.RegexUtils.UrlRegex))
                {
                    // no URLs contained
                    result.Add(new Run(text));
                    break;
                }
                else
                {
                    // url is contained in the input string
                    var match = Regex.Match(text, Extensions.RegexUtils.UrlRegex);

                    if (match.Index > 0)
                    {
                        // convert the leading text to a Run
                        result.Add(new Run(text.Substring(0, match.Index)));
                    }

                    var hyperlink = new Hyperlink(new Run(match.Value))
                    {
                        NavigateUri = new Uri(match.Value),
                    };
                    hyperlink.RequestNavigate += this.HyperlinkHandler;

                    result.Add(hyperlink);

                    // Keep looping over the rest of the string.
                    text = text.Substring(match.Index + match.Length);
                }
            }

            return result;
        }

        private void HyperlinkHandler(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Extensions.WebBrowserHelper.OpenUrl(e.Uri.ToString());
        }
    }
}
