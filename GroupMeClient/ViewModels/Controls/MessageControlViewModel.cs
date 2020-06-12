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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Caching;
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
        private RepliedMessageControlViewModel repliedMessage;

        private bool showDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageControlViewModel"/> class.
        /// </summary>
        /// <param name="message">The message to bind to this control.</param>
        /// <param name="cacheManager">The cache context in which this message is archived.</param>
        /// <param name="showLikers">Indicates whether the like status for a message should be displayed.</param>
        /// <param name="showPreviewsOnlyForMultiImages">Indicates whether only previews, or full images, should be shown for multi-images.</param>
        /// <param name="nestLevel">The number of <see cref="MessageControlViewModel"/>s deeply nested this is. Top level messages are 0.</param>
        public MessageControlViewModel(Message message, CacheManager cacheManager, bool showLikers = true, bool showPreviewsOnlyForMultiImages = false, int nestLevel = 0)
        {
            this.Message = message;
            this.CacheManager = cacheManager;

            this.Avatar = new AvatarControlViewModel(this.Message, this.Message.ImageDownloader);
            this.Inlines = new ObservableCollection<Inline>();
            this.LikeAction = new RelayCommand(async () => { await this.LikeMessageAsync(); }, () => { return true; }, true);
            this.ToggleMessageDetails = new RelayCommand(() => this.ShowDetails = !this.ShowDetails);

            this.ShowLikers = showLikers;
            this.ShowPreviewsOnlyForMultiImages = showPreviewsOnlyForMultiImages;
            this.NestLevel = nestLevel;
            this.RepliedMessage = null;

            this.LoadAttachments();
            this.LoadInlinesForMessageBody();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageControlViewModel"/> class.
        /// </summary>
        /// <param name="copy">An existing <see cref="MessageControlViewModel"/> to copy from.</param>
        /// <param name="nestLevelOffset">An additive offset to apply to the nest level that is copied from the existing message.</param>
        public MessageControlViewModel(MessageControlViewModel copy, int nestLevelOffset = 0)
        {
            this.Message = copy.Message;
            this.CacheManager = copy.CacheManager;
            this.Avatar = copy.Avatar;
            this.Inlines = copy.Inlines;
            this.LikeAction = copy.LikeAction;
            this.ToggleMessageDetails = copy.ToggleMessageDetails;
            this.ShowLikers = copy.ShowLikers;
            this.ShowPreviewsOnlyForMultiImages = copy.ShowPreviewsOnlyForMultiImages;
            this.NestLevel = copy.NestLevel + nestLevelOffset;
        }

        /// <summary>
        /// Gets or sets the attached items (Tweets, Web Links, Videos, etc.), if present.
        /// </summary>
        public ObservableCollection<object> AttachedItems { get; set; } = new ObservableCollection<object>();

        /// <summary>
        /// Gets the command to be performed when this <see cref="Message"/> is 'Liked'.
        /// </summary>
        public ICommand LikeAction { get; }

        /// <summary>
        /// Gets the command to be performed to toggle whether details are shwon for this <see cref="Message"/>.
        /// </summary>
        public ICommand ToggleMessageDetails { get; }

        /// <summary>
        /// Gets a value indicating the number of <see cref="MessageControlViewModel"/>s deep this <see cref="MessageControlViewModel"/> is nested. Top-level messages that
        /// are not included as attachments have a <see cref="NestLevel"/> of 0.
        /// </summary>
        public int NestLevel { get; }

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
            get => this.avatar;
            private set => this.Set(() => this.Avatar, ref this.avatar, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether details should be shown for this message.
        /// </summary>
        public bool ShowDetails
        {
            get => this.showDetails;
            set => this.Set(() => this.ShowDetails, ref this.showDetails, value);
        }

        /// <summary>
        /// Gets or sets a value containing a quoted message that is being replied to.
        /// If this <see cref="Message"/> is not a reply, this value is null.
        /// </summary>
        public RepliedMessageControlViewModel RepliedMessage
        {
            get => this.repliedMessage;
            set => this.Set(() => this.RepliedMessage, ref this.repliedMessage, value);
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
                else if (this.Message.SourceGuid.StartsWith("gmdctoast-"))
                {
                    return "GroupMe Desktop Client (Quick Reply)";
                }
                else if (this.Message.SourceGuid.StartsWith("gmdca-"))
                {
                    return "GroupMe Desktop Client Avalonia";
                }
                else if (this.Message.SourceGuid.StartsWith("gmdcatoast-"))
                {
                    return "GroupMe Desktop Client Avalonia (Quick Reply)";
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
        /// Gets a value indicating whether the current user sent this <see cref="Message"/> for the purposes of styling the message.
        /// </summary>
        public bool? DidISendItColoring
        {
            get
            {
                if (this.NestLevel > 0)
                {
                    return null;
                }
                else
                {
                    return this.DidISendIt;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user sent this <see cref="Message"/>.
        /// </summary>
        public bool DidISendIt
        {
            get
            {
                var me = this.Message.Group?.WhoAmI() ?? this.Message.Chat?.WhoAmI();
                return this.Message.UserId == me.UserId;
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
        /// Gets the people who have liked this <see cref="Message"/>.
        /// </summary>
        public IEnumerable<Liker> LikedByAvatars
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
                        yield return new Liker { Avatar = liker, Name = member.Name };
                    }
                }
            }
        }

        private CacheManager CacheManager { get; }

        private bool ShowLikers { get; }

        private string HiddenText { get; set; } = string.Empty;

        private bool ShowPreviewsOnlyForMultiImages { get; }

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
            bool doneWithAttachments = false;

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
                    doneWithAttachments = true;
                    break;
                }
                else if (attachment is VideoAttachment videoAttach)
                {
                    var videoLinkedVm = new VideoAttachmentControlViewModel(videoAttach, this.Message.ImageDownloader);
                    this.AttachedItems.Add(videoLinkedVm);

                    // Videos can have captions, so only exclude the v.groupme url from the body
                    this.HiddenText = videoAttach.Url;

                    // Don't allow any other attachment types to be included if a video is.
                    doneWithAttachments = true;
                    break;
                }
                else if (attachment is FileAttachment fileAttach)
                {
                    var container = (IMessageContainer)this.Message.Group ?? this.Message.Chat;
                    var documentVm = new FileAttachmentControlViewModel(fileAttach, container, this.Message);
                    this.AttachedItems.Add(documentVm);

                    // Files can have captions, so only exclude the share url from the body
                    if (this.Message.Text.Contains(" - Shared a document"))
                    {
                        this.HiddenText = this.Message.Text.Substring(this.Message.Text.LastIndexOf(" - Shared a document"));
                    }
                    else
                    {
                        this.HiddenText = this.Message.Text.Substring(this.Message.Text.LastIndexOf("Shared a document"));
                    }

                    // Don't allow any other attachment types to be included if a video is.
                    doneWithAttachments = true;
                    break;
                }
            }

            var repliedMessageId = string.Empty;

            // Check if this is a GroupMe Desktop Client Reply-extension message
            if (!string.IsNullOrEmpty(this.Message.Text) && Regex.IsMatch(this.Message.Text, Utilities.RegexUtils.RepliedMessageRegex))
            {
                // Method 1, where /rmid:<message-id> is appended to the end of the message body
                var token = Regex.Match(this.Message.Text, Utilities.RegexUtils.RepliedMessageRegex).Value;
                this.HiddenText = token + this.HiddenText;
                repliedMessageId = token.Replace("\n/rmid:", string.Empty);
            }
            else if (this.Message.SourceGuid.StartsWith("gmdc-r"))
            {
                // Method 2, where gmdc-r<message-id> is included as the prefix of the message GUID
                var parts = this.Message.SourceGuid.Split('-');
                repliedMessageId = parts[1].Substring(1);
            }

            // Handle if this is a GroupMe Desktop Client Reply-extension message
            if (!string.IsNullOrEmpty(repliedMessageId))
            {
                var container = (IMessageContainer)this.Message.Group ?? this.Message.Chat;
                var repliedMessageAttachment = new RepliedMessageControlViewModel(repliedMessageId, container, this.CacheManager, this.NestLevel);

                // Replace the photo of the original message that is included for non-GMDC clients with the real message
                if (this.AttachedItems.Count > 0)
                {
                    var lastIndexOfPhoto = -1;
                    for (int i = 0; i < this.AttachedItems.Count; i++)
                    {
                        if (this.AttachedItems[i] is GroupMeImageAttachmentControlViewModel)
                        {
                            lastIndexOfPhoto = i;
                        }
                    }

                    if (lastIndexOfPhoto >= 0)
                    {
                        this.AttachedItems.RemoveAt(lastIndexOfPhoto);
                    }
                }

                this.RepliedMessage = repliedMessageAttachment;
            }

            if (doneWithAttachments)
            {
                return;
            }

            // Load Link-Based Attachments (Tweets, Web Images, Websites, etc.)
            var text = this.Message.Text ?? string.Empty;
            if (text.IndexOf(" ") > 0)
            {
                // Use IndexOf instead of Contains to prevent issues with strange unicode characters.
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
                this.HiddenText = vm.Url + this.HiddenText;
            }
        }

        private void LoadInlinesForMessageBody()
        {
            var text = this.Message.Text ?? string.Empty;

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

            // Remove any hidden text
            inlinesResult.Clear();
            foreach (var inline in inlinesTemp)
            {
                if (!string.IsNullOrEmpty(this.HiddenText) &&
                    inline is Run r &&
                    r.Text.Contains(this.HiddenText))
                {
                    // split or remove the inline to eliminate the hidden text
                    if (r.Text.Trim() == this.HiddenText)
                    {
                        // eliminate the entire run
                    }
                    else
                    {
                        var index = r.Text.IndexOf(this.HiddenText);
                        if (index != 0)
                        {
                            var before = new Run(r.Text.Substring(0, index));
                            inlinesResult.Add(before);
                        }

                        if (index + this.HiddenText.Length != r.Text.Length)
                        {
                            var after = new Run(r.Text.Substring(index + this.HiddenText.Length));
                            inlinesResult.Add(after);
                        }
                    }
                }
                else
                {
                    inlinesResult.Add(inline);
                }
            }

            inlinesTemp.Clear();

            // Process Hyperlinks
            foreach (var part in inlinesResult)
            {
                if (part is Run r)
                {
                    // scan this portion of the message for hyperlinks
                    inlinesTemp.AddRange(this.ProcessHyperlinks(r));
                }
                else
                {
                    // this part of the message has already been processed, pass it through
                    inlinesTemp.Add(part);
                }
            }

            /* TODO: Swap inlinesTemp and inlinesResult and repeat for addition special content types */

            this.Inlines.Clear();
            foreach (var result in inlinesTemp)
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

        /// <summary>
        /// <see cref="Liker"/> represents an avatar/name pair for a person who has liked a <see cref="Message"/>.
        /// </summary>
        public class Liker
        {
            /// <summary>
            /// Gets or sets the liker's avatar.
            /// </summary>
            public AvatarControlViewModel Avatar { get; set; }

            /// <summary>
            /// Gets or sets the name of the liker.
            /// </summary>
            public string Name { get; set; }
        }
    }
}
