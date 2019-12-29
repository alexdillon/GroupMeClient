using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
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

        private bool showDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageControlViewModel"/> class.
        /// </summary>
        /// <param name="message">The message to bind to this control.</param>
        /// <param name="showLikers">Indicates whether the like status for a message should be displayed.</param>
        /// <param name="showPreviewsOnlyForMultiImages">Indicates whether only previews, or full images, should be shown for multi-images.</param>
        public MessageControlViewModel(Message message, bool showLikers = true, bool showPreviewsOnlyForMultiImages = false)
        {
            this.Message = message;

            this.Avatar = new AvatarControlViewModel(this.Message, this.Message.ImageDownloader);
            this.Inlines = new ObservableCollection<Inline>();
            this.LikeAction = new RelayCommand(async () => { await this.LikeMessageAsync(); }, () => { return true; }, true);
            this.ToggleMessageDetails = new RelayCommand(() => this.ShowDetails = !this.ShowDetails);

            this.ShowLikers = showLikers;
            this.ShowPreviewsOnlyForMultiImages = showPreviewsOnlyForMultiImages;

            this.LoadAttachments();
            this.LoadInlinesForMessageBody();
        }

        /// <summary>
        /// Gets or sets the attached items (Tweets, Web Links, Videos, etc.), if present.
        /// </summary>
        public ObservableCollection<LinkAttachmentBaseViewModel> AttachedItems { get; set; } = new ObservableCollection<LinkAttachmentBaseViewModel>();

        /// <summary>
        /// Gets the command to be performed when this <see cref="Message"/> is 'Liked'.
        /// </summary>
        public ICommand LikeAction { get; }

        /// <summary>
        /// Gets the command to be performed to toggle whether details are shwon for this <see cref="Message"/>.
        /// </summary>
        public ICommand ToggleMessageDetails { get; }

        /// <summary>
        /// Gets the unique identifier for this <see cref="Message"/>.
        /// </summary>
        public override string Id => this.Message.Id;

        /// <summary>
        /// Gets the sender of this <see cref="Message"/>.
        /// </summary>
        public string Sender => this.Message.Name;

        /// <summary>
        /// Gets a formatted string with the date and time this <see cref="Message"/> was sent.
        /// </summary>
        public string SentTimeString => this.Message.CreatedAtTime.ToString("MM/dd/yy h:mm:ss tt");

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
        /// Gets or sets a value indicating whether details should be shown for this message.
        /// </summary>
        public bool ShowDetails
        {
            get
            {
                return this.showDetails;
            }

            set
            {
                this.Set(() => this.ShowDetails, ref this.showDetails, value);
            }
        }

        /// <summary>
        /// Gets a collection of inline text blocks to display for the message.
        /// </summary>
        public ObservableCollection<Inline> Inlines { get; }

        /// <summary>
        /// Gets a string indicating which GroupMe Platform was used to send this <see cref="Message"/>.
        /// </summary>
        public string SenderPlatform
        {
            get
            {
                if (this.Message.SourceGuid.StartsWith("gmdc-"))
                {
                    return "GroupMe Desktop Client";
                }
                else if (this.Message.SourceGuid.StartsWith("gmdca-"))
                {
                    return "GroupMe Desktop Client Avalonia";
                }
                else if (this.message.SourceGuid.StartsWith("android"))
                {
                    return "Android";
                }
                else if (this.message.SourceGuid.ToUpper() == this.Message.SourceGuid)
                {
                    return "iOS";
                }
                else if (!this.message.SourceGuid.Contains("-"))
                {
                    return "Web Client";
                }
                else
                {
                    return "GroupMe UWP";
                }
            }
        }

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
                    return (Brush)Application.Current.FindResource("MessageISentBackdropBrush");
                }
                else
                {
                    return (Brush)Application.Current.FindResource("MessageTheySentBackdropBrush");
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
                    return (Brush)Application.Current.FindResource("MessageILikedBrush");
                }
                else
                {
                    return (Brush)Application.Current.FindResource("MessageTheyLikedBrush");
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

        private bool ShowLikers { get; }

        private string HiddenText { get; set; }

        private bool ShowPreviewsOnlyForMultiImages { get; set; }

        /// <summary>
        /// Redraw the message immediately.
        /// </summary>
        public void UpdateDisplay()
        {
            this.RaisePropertyChanged(string.Empty); // no property name to force every single property to be updated
        }

        /// <summary>
        /// Likes a message and updates the Liker's Display area for the current <see cref="Message"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LikeMessageAsync()
        {
            var me = this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI();
            var alreadyLiked = this.Message.FavoritedBy.Contains(me.Id);

            if (alreadyLiked)
            {
                var success = await this.Message.UnlikeMessage();
                if (success)
                {
                    lock (this.messageLock)
                    {
                        this.Message.FavoritedBy.Remove(me.Id);
                    }
                }
            }
            else
            {
                var success = await this.Message.LikeMessage();
                if (success)
                {
                    lock (this.messageLock)
                    {
                        this.Message.FavoritedBy.Add(me.Id);
                    }
                }
            }

            this.UpdateDisplay();
        }

        /// <summary>
        /// Updates the list of Likers for the <see cref="Message"/>, and re-draws
        /// the message on-screen.
        /// </summary>
        /// <param name="newLikersList">The list of new likers for the <see cref="Message"/>.</param>
        public void UpdateLikers(IEnumerable<string> newLikersList)
        {
            // The Message needs to be locked to prevent accessing the liker's
            // collection asychronously on a UI thread (for example, on mouse-over)
            // mid-update.
            lock (this.messageLock)
            {
                this.Message.FavoritedBy.Clear();
                foreach (var liker in newLikersList)
                {
                    this.Message.FavoritedBy.Add(liker);
                }

                this.UpdateDisplay();
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            foreach (var attachment in this.AttachedItems)
            {
                (attachment as IDisposable)?.Dispose();
            }
        }

        private void LoadAttachments()
        {
            bool hasMultipleImages = this.Message.Attachments.OfType<ImageAttachment>().Count() > 1;

            // Load GroupMe Image and Video Attachments
            foreach (var attachment in this.Message.Attachments)
            {
                if (attachment is ImageAttachment imageAttach)
                {
                    var displayMode = GroupMeImageAttachmentControlViewModel.GroupMeImageDisplayMode.Large;
                    if (hasMultipleImages && this.ShowPreviewsOnlyForMultiImages)
                    {
                        displayMode = GroupMeImageAttachmentControlViewModel.GroupMeImageDisplayMode.Preview;
                    }

                    var imageVm = new GroupMeImageAttachmentControlViewModel(imageAttach, this.Message.ImageDownloader, displayMode);
                    this.AttachedItems.Add(imageVm);

                    // Starting in 9/2019, GroupMe supports multiple images-per-message.
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
                else if (attachment is VideoAttachment videoAttach)
                {
                    var videoLinkedVm = new VideoAttachmentControlViewModel(videoAttach, this.Message.ImageDownloader);
                    this.AttachedItems.Add(videoLinkedVm);

                    // Videos can have captions, so only exclude the v.groupme url from the body
                    this.HiddenText = videoAttach.Url;

                    // Don't allow any other attachment types to be included if a video is.
                    return;
                }
            }

            // Load Link-Based Attachments (Tweets, Web Images, Websites, etc.)
            var text = this.Message.Text ?? string.Empty;
            if (text.Contains(" "))
            {
                // only look to see if the first chunk is a URL
                text = text.Substring(0, text.IndexOf(" "));
            }

            const string TwitterPrefixHttps = "https://twitter.com/";
            const string TwitterPrefixHttp = "http://twitter.com/";

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

        private void LoadInlinesForMessageBody()
        {
            var text = this.Message.Text ?? string.Empty;

            if (!string.IsNullOrEmpty(this.HiddenText))
            {
                text = text.Replace(this.HiddenText, string.Empty);
            }

            var inlinesTemp = new List<Inline>();
            var inlinesResult = new List<Inline>();

            // Process Mentions
            var sortedMentions = this.Message.Attachments.OfType<MentionsAttachment>().FirstOrDefault()?.Mentions().ToList().OrderBy(m => m.startIndex) ?? null;
            if (sortedMentions != null)
            {
                var remainingText = text;
                var lastOffset = 0;
                foreach (var mention in sortedMentions)
                {
                    var leadingText = new Run(remainingText.Substring(0, mention.startIndex - lastOffset));
                    var mentionTag = new Bold(new Run(remainingText.Substring(mention.startIndex - lastOffset, mention.length)) { FontWeight = FontWeights.SemiBold });

                    remainingText = remainingText.Substring(mention.startIndex + mention.length - lastOffset);
                    lastOffset = mention.startIndex + mention.length;

                    inlinesTemp.Add(leadingText);
                    inlinesTemp.Add(mentionTag);
                }

                inlinesTemp.Add(new Run(remainingText));
            }
            else
            {
                inlinesTemp.Add(new Run(text));
            }

            // Process Hyperlinks
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
                if (!Regex.IsMatch(text, Utilities.RegexUtils.UrlRegex))
                {
                    // no URLs contained
                    result.Add(new Run(text));
                    break;
                }
                else
                {
                    // url is contained in the input string
                    var match = Regex.Match(text, Utilities.RegexUtils.UrlRegex);

                    if (match.Index > 0)
                    {
                        // convert the leading text to a Run
                        result.Add(new Run(text.Substring(0, match.Index)));
                    }

                    try
                    {
                        var hyperlink = new Hyperlink(new Run(match.Value))
                        {
                            NavigateUri = new Uri(match.Value),
                        };

                        Extensions.WebHyperlinkExtensions.SetIsWebLink(hyperlink, true);

                        result.Add(hyperlink);
                    }
                    catch (Exception)
                    {
                        // Some super strange URLs pass the regex, but fail
                        // to decode correctly. Ignore if this happens.
                        result.Add(new Run(match.Value));
                    }

                    // Keep looping over the rest of the string.
                    text = text.Substring(match.Index + match.Length);
                }
            }

            return result;
        }
    }
}
