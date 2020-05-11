using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.Caching;
using GroupMeClient.Extensions;
using GroupMeClient.Utilities;
using GroupMeClient.Views.Controls;
using GroupMeClientApi.Models;
using Microsoft.Win32;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="GroupContentsControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.GroupContentsControl"/> control that displays the contents (messages) of a Group or Chat.
    /// Controls for sending messages are also included.
    /// </summary>
    public class GroupContentsControlViewModel : ViewModelBase, FileDragDropPasteHelper.IDragDropPasteTarget, IDisposable
    {
        private IMessageContainer messageContainer;
        private AvatarControlViewModel topBarAvatar;
        private string typedMessageContents = string.Empty;
        private bool isSelectionAllowed = false;
        private MessageControlViewModel messageBeingRepliedTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContentsControlViewModel"/> class.
        /// </summary>
        public GroupContentsControlViewModel()
        {
            this.Messages = new ObservableCollection<MessageControlViewModelBase>();

            this.ReloadSem = new SemaphoreSlim(1, 1);

            this.SendMessage = new RelayCommand(async () => await this.SendMessageAsync(), () => !this.IsSending, true);
            this.SendAttachment = new RelayCommand(this.SendFileImageAttachment);
            this.OpenMessageSuggestions = new RelayCommand(this.OpenMessageSuggestionsDialog);
            this.ReloadView = new RelayCommand<ScrollViewer>(async (s) => await this.LoadMoreAsync(s), true);
            this.GroupChatPluginActivated = new RelayCommand<GroupMeClientPlugin.GroupChat.IGroupChatPlugin>(this.ActivateGroupPlugin);
            this.SelectionChangedCommand = new RelayCommand<object>(this.SelectionChangedHandler);
            this.InitiateReply = new RelayCommand<MessageControlViewModel>(m => this.InitiateReplyCommand(m));
            this.TerminateReply = new RelayCommand(() => this.MessageBeingRepliedTo = null, true);

            this.SmallDialogManager = new PopupViewModel()
            {
                ClosePopup = new RelayCommand(this.ClosePopupHandler),
                EasyClosePopup = null,  // EasyClose makes it too easy to accidently close the send dialog.
                PopupDialog = null
            };

            this.ReliabilityStateMachine = new ReliabilityStateMachine();

            this.GroupChatPlugins = new ObservableCollection<GroupMeClientPlugin.GroupChat.IGroupChatPlugin>();
            foreach (var plugin in Plugins.PluginManager.Instance.GroupChatPlugins)
            {
                this.GroupChatPlugins.Add(plugin);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContentsControlViewModel"/> class.
        /// </summary>
        /// <param name="messageContainer">The Group or Chat to bind to.</param>
        /// <param name="cacheManager">The caching context in which messages are archived.</param>
        /// <param name="settings">The settings instance to use.</param>
        public GroupContentsControlViewModel(IMessageContainer messageContainer, CacheManager cacheManager, Settings.SettingsManager settings)
            : this()
        {
            this.MessageContainer = messageContainer;
            this.CacheManager = cacheManager;
            this.Settings = settings;
            this.TopBarAvatar = new AvatarControlViewModel(this.MessageContainer, this.MessageContainer.Client.ImageDownloader);

            // Install Pseduo-Plugins
            if (this.MessageContainer is Group g)
            {
                this.GroupChatPlugins.Add(new GroupInfoControlViewModel.GroupInfoPseudoPlugin(g));
            }

            this.GroupChatPlugins.Add(new MultiLikeControlViewModel.MultiLikePseudoPlugin(this));

            _ = this.LoadMoreAsync();
        }

        /// <summary>
        /// Gets or sets the action to be performed when the close button is pressed.
        /// </summary>
        public ICommand CloseGroup { get; set; }

        /// <summary>
        /// Gets or sets the action to be performed when a message is ready to send.
        /// </summary>
        public ICommand SendMessage { get; set; }

        /// <summary>
        /// Gets or sets the action to be performed when the user wants to send an attachment file.
        /// </summary>
        public ICommand SendAttachment { get; set; }

        /// <summary>
        /// Gets or sets the action to be performd when the user has selected the Message Effects Generator.
        /// </summary>
        public ICommand OpenMessageSuggestions { get; set; }

        /// <summary>
        /// Gets the action to be performed when more messages need to be loaded.
        /// </summary>
        public ICommand ReloadView { get; private set; }

        /// <summary>
        /// Gets the action to be performed when initiating a reply on a message.
        /// </summary>
        public ICommand InitiateReply { get; }

        /// <summary>
        /// Gets the action to be performed when terminating a reply to a message.
        /// </summary>
        public ICommand TerminateReply { get; }

        /// <summary>
        /// Gets the action to be be performed when the selected messages have changed.
        /// This is used by the Multi-Like plugin.
        /// </summary>
        public ICommand SelectionChangedCommand { get; }

        /// <summary>
        /// Gets the collection of ViewModels for <see cref="Message"/>s to be displayed.
        /// </summary>
        public ObservableCollection<MessageControlViewModelBase> Messages { get; }

        /// <summary>
        /// Gets the collection of available Group/Chat UI Plugins to display.
        /// </summary>
        public ObservableCollection<GroupMeClientPlugin.GroupChat.IGroupChatPlugin> GroupChatPlugins { get; }

        /// <summary>
        /// Gets the action to be performed when a Plugin in the
        /// Options Menu is activated.
        /// </summary>
        public ICommand GroupChatPluginActivated { get; }

        /// <summary>
        /// Gets the title of the <see cref="Group"/> or <see cref="Chat"/>.
        /// </summary>
        public string Title => this.MessageContainer.Name;

        /// <summary>
        /// Gets the unique identifier for the <see cref="Group"/> or <see cref="Chat"/>.
        /// </summary>
        public string Id => this.MessageContainer.Id;

        /// <summary>
        /// Gets the list of messages that are currently selected.
        /// </summary>
        public object CurrentlySelectedMessages { get; private set; }

        /// <summary>
        /// Gets the manager for the dialog that should be displayed as a small popup.
        /// </summary>
        public PopupViewModel SmallDialogManager { get; }

        /// <summary>
        /// Gets or sets the <see cref="IMessageContainer"/> being displayed.
        /// </summary>
        public IMessageContainer MessageContainer
        {
            get { return this.messageContainer; }
            set { this.Set(() => this.MessageContainer, ref this.messageContainer, value); }
        }

        /// <summary>
        /// Gets the avatar to be displayed in the top bar.
        /// </summary>
        public AvatarControlViewModel TopBarAvatar
        {
            get { return this.topBarAvatar; }
            private set { this.Set(() => this.TopBarAvatar, ref this.topBarAvatar, value); }
        }

        /// <summary>
        /// Gets or sets the contents the user has typed for a new message.
        /// </summary>
        public string TypedMessageContents
        {
            get { return this.typedMessageContents; }
            set { this.Set(() => this.TypedMessageContents, ref this.typedMessageContents, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether selecting messages is allowed.
        /// </summary>
        public bool IsSelectionAllowed
        {
            get { return this.isSelectionAllowed; }
            set { this.Set(() => this.IsSelectionAllowed, ref this.isSelectionAllowed, value); }
        }

        /// <summary>
        /// Gets or sets a displayable copy of a <see cref="Message"/> that is currently having a response composed for sending.
        /// If the currently composed message is not a reply, this property will be null.
        /// </summary>
        public MessageControlViewModel MessageBeingRepliedTo
        {
            get { return this.messageBeingRepliedTo; }
            set { this.Set(() => this.MessageBeingRepliedTo, ref this.messageBeingRepliedTo, value); }
        }

        private CacheManager CacheManager { get; }

        private SemaphoreSlim ReloadSem { get; }

        private Message FirstDisplayedMessage { get; set; } = null;

        private DateTime LastMarkerTime { get; set; } = DateTime.MinValue;

        private ReliabilityStateMachine ReliabilityStateMachine { get; }

        private Timer RetryTimer { get; set; }

        private bool IsSending { get; set; }

        private Settings.SettingsManager Settings { get; }

        /// <summary>
        /// Reloads and redisplay the newest messages.
        /// This will capture any messages send since the last reload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadNewMessages()
        {
            await this.LoadMoreAsync(null, true);
        }

        /// <summary>
        /// Updates the 'Likes' for a currently displayed <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The message containing the updated list of likers.</param>
        public void UpdateMessageLikes(Message message)
        {
            var msgVm = this.Messages.FirstOrDefault(m => m.Id == message.Id);
            if (msgVm is MessageControlViewModel messageVm)
            {
                messageVm.UpdateLikers(message.FavoritedBy);
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            this.Messages.Clear();

            this.RetryTimer?.Dispose();

            try
            {
                foreach (var msg in this.Messages)
                {
                    (msg as IDisposable)?.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <inheritdoc />
        void FileDragDropPasteHelper.IDragDropPasteTarget.OnFileDrop(string[] filepaths)
        {
            var supportedImageExtensions = GroupMeClientApi.Models.Attachments.ImageAttachment.SupportedExtensions.ToList();
            var supportedFileExtensions = GroupMeClientApi.Models.Attachments.FileAttachment.GroupMeDocumentMimeTypeMapper.SupportedExtensions.ToList();

            foreach (var file in filepaths)
            {
                if (supportedImageExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    this.ShowImageSendDialog(file);
                    break;
                }
                else if (supportedFileExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    this.ShowFileSendDialog(file);
                    break;
                }
                else
                {
                    // Allow sending unsupported files anyway if they drag-drop them
                    this.ShowFileSendDialog(file);
                    break;
                }
            }
        }

        /// <inheritdoc />
        void FileDragDropPasteHelper.IDragDropPasteTarget.OnImageDrop(byte[] image)
        {
            var memoryStream = new MemoryStream(image);
            this.ShowImageSendDialog(memoryStream);
        }

        private async Task LoadMoreAsync(ScrollViewer scrollViewer = null, bool updateNewest = false)
        {
            await this.ReloadSem.WaitAsync();

            try
            {
                ICollection<Message> results;

                if (this.FirstDisplayedMessage == null || updateNewest)
                {
                    // load the most recent messages
                    results = await this.MessageContainer.GetMessagesAsync();

                    this.CacheManager.SuperIndexer.SubmitGroupUpdate(this.MessageContainer, results);
                }
                else
                {
                    // load the 20 (GroupMe default) messages preceeding the oldest (first) one displayed
                    results = await this.MessageContainer.GetMessagesAsync(GroupMeClientApi.MessageRetreiveMode.BeforeId, this.FirstDisplayedMessage.Id);
                }

                await this.UpdateDisplay(scrollViewer, results);

                // if everything was successful, reset the reliability monitor
                this.ReliabilityStateMachine.Succeeded();
                this.RetryTimer?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in {nameof(this.LoadMoreAsync)} - {ex.Message}. Retrying...");
                this.RetryTimer = this.ReliabilityStateMachine.GetRetryTimer(async () => await this.LoadMoreAsync(scrollViewer, updateNewest));
            }
            finally
            {
                this.ReloadSem.Release();
            }
        }

        private async Task UpdateDisplay(ScrollViewer scrollViewer, ICollection<Message> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // the code that's accessing UI properties
                double originalHeight = scrollViewer?.ExtentHeight ?? 0.0;
                if (originalHeight != 0)
                {
                    // prevent the At Top event from firing while we are adding new messages
                    scrollViewer.ScrollToVerticalOffset(1);
                }

                var maxTimeDifference = TimeSpan.FromMinutes(15);

                // Messages retrieved with the before_id parameter are returned in descending order
                // Reverse iterate through the messages collection to go newest->oldest
                for (int i = messages.Count - 1; i >= 0; i--)
                {
                    var msg = messages.ElementAt(i);

                    var oldMsg = this.Messages.FirstOrDefault(m => m.Id == msg.Id);

                    if (oldMsg == null)
                    {
                        // add new message
                        var msgVm = new MessageControlViewModel(
                            msg,
                            this.CacheManager,
                            showPreviewsOnlyForMultiImages: this.Settings.UISettings.ShowPreviewsForMultiImages);
                        this.Messages.Add(msgVm);

                        // add an inline timestamp if needed
                        if (msg.CreatedAtTime.Subtract(this.LastMarkerTime) > maxTimeDifference)
                        {
                            var messageId = long.Parse(msg.Id);
                            var timeStampId = (messageId - 1).ToString();

                            this.Messages.Add(new InlineTimestampControlViewModel(msg.CreatedAtTime, timeStampId, msgVm.DidISendIt));
                            this.LastMarkerTime = msg.CreatedAtTime;
                        }
                    }
                    else
                    {
                        // update an existing one if needed
                        oldMsg.Message = msg;
                    }
                }

                // process read receipt and sent receipts
                if (this.MessageContainer.ReadReceipt != null)
                {
                    // Remove old markers
                    var toRemove = this.Messages.OfType<InlineReadSentMarkerControlViewModel>().ToList();
                    foreach (var marker in toRemove)
                    {
                        this.Messages.Remove(marker);
                    }

                    // Attach a "Read Receipt" if the read message is displayed.
                    var matchedMessage = this.Messages.FirstOrDefault(m => m.Id == this.MessageContainer.ReadReceipt.MessageId);
                    if (matchedMessage != null)
                    {
                        var msgId = long.Parse(matchedMessage.Id);

                        var readMarker = new InlineReadSentMarkerControlViewModel(
                            this.MessageContainer.ReadReceipt.ReadAtTime,
                            true,
                            (msgId + 1).ToString(),
                            (matchedMessage as MessageControlViewModel).DidISendIt);

                        this.Messages.Add(readMarker);
                    }

                    // Attach a "Sent Receipt" to the last message confirmed sent by GroupMe
                    var me = this.MessageContainer.WhoAmI();
                    var lastSentMessage = this.Messages
                        .OfType<MessageControlViewModel>()
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefault(m => m.Message.UserId == me.Id);

                    if (lastSentMessage != null && lastSentMessage != matchedMessage)
                    {
                        var msgId = long.Parse(lastSentMessage.Id);

                        var sentMarker = new InlineReadSentMarkerControlViewModel(
                            lastSentMessage.Message.CreatedAtTime,
                            false,
                            (msgId + 1).ToString(),
                            (lastSentMessage as MessageControlViewModel).DidISendIt);

                        this.Messages.Add(sentMarker);
                    }

                    // Send a Read Receipt for the last message received
                    var lastReceivedMessage = this.Messages
                        .OfType<MessageControlViewModel>()
                        .OrderByDescending(m => m.Id)
                        .FirstOrDefault(m => m.Message.UserId != me.Id);

                    if (lastReceivedMessage != null && this.MessageContainer is Chat c)
                    {
                        var result = Task.Run(async () => await c.SendReadReceipt(lastReceivedMessage.Message)).Result;
                    }
                }

                if (originalHeight != 0)
                {
                    // Calculate the offset where the last message the user was looking at is
                    // Scroll back to there so new messages appear on top, above screen
                    scrollViewer.UpdateLayout();
                    double newHeight = scrollViewer?.ExtentHeight ?? 0.0;
                    double difference = newHeight - originalHeight;

                    scrollViewer.ScrollToVerticalOffset(difference);
                }

                if (messages.Count > 0)
                {
                    this.FirstDisplayedMessage = messages.Last();
                }
            });
        }

        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrEmpty(this.TypedMessageContents))
            {
                this.IsSending = true;
                var newMessage = Message.CreateMessage(
                    this.TypedMessageContents,
                    guidPrefix: "gmdc");
                await this.SendMessageAsync(newMessage);
            }
        }

        private void SendFileImageAttachment()
        {
            var supportedImages = GroupMeClientApi.Models.Attachments.ImageAttachment.SupportedExtensions.ToList();
            var supportedFiles = GroupMeClientApi.Models.Attachments.FileAttachment.GroupMeDocumentMimeTypeMapper.SupportedExtensions.ToList();

            var imageExtensions = string.Join(";", supportedImages.Select(x => "*" + x));
            var imageExtensionsDisplay = string.Join(", ", supportedImages.Select(x => "*" + x));
            var fileExtensions = string.Join(";", supportedFiles.Select(x => "*" + x));
            var fileExtensionsDisplay = string.Join(", ", supportedFiles.Select(x => "*" + x));

            var imageFilter = $"Images ({imageExtensionsDisplay})|{imageExtensions}";
            var fileFilter = $"Documents ({fileExtensionsDisplay})|{fileExtensions}";

            var openFileDialog = new OpenFileDialog
            {
                Filter = $"{imageFilter}|{fileFilter}",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(openFileDialog.FileName);
                if (supportedImages.Contains(extension))
                {
                    this.ShowImageSendDialog(openFileDialog.FileName);
                }
                else if (supportedFiles.Contains(extension))
                {
                    this.ShowFileSendDialog(openFileDialog.FileName);
                }
            }
        }

        private async Task SendContentMessageAsync(GroupMeClientApi.Models.Attachments.Attachment attachment)
        {
            if (!(this.SmallDialogManager.PopupDialog is SendContentControlViewModelBase))
            {
                return;
            }

            var contentSendDialog = this.SmallDialogManager.PopupDialog as SendContentControlViewModelBase;

            if (contentSendDialog.ContentStream == null)
            {
                return;
            }

            contentSendDialog.IsSending = true;
            this.IsSending = true;

            var contents = contentSendDialog.TypedMessageContents;
            var attachmentsList = new List<GroupMeClientApi.Models.Attachments.Attachment> { attachment };

            var message = Message.CreateMessage(
                contents,
                attachmentsList,
                "gmdc");
            bool success = await this.SendMessageAsync(message);

            if (success)
            {
                this.ClosePopupHandler();
            }
            else
            {
                contentSendDialog.IsSending = false;
            }
        }

        private byte[] RenderMessageToPngImage(Message message)
        {
            var messageDataContext = new MessageControlViewModel(message, this.CacheManager, false, true, 1);

            // Copy the attachments from the version of the message that is already rendered and displayed.
            // These attachments already have previews downloaded and ready-to-render.
            messageDataContext.AttachedItems.Clear();
            var displayedMessage = this.Messages.First(m => m.Id == message.Id);
            foreach (var attachment in (displayedMessage as MessageControlViewModel).AttachedItems)
            {
                // Images don't render correctly as-is due to the usage of the GIF attached property.
                if (attachment is Attachments.GroupMeImageAttachmentControlViewModel gmImage)
                {
                    byte[] imageBytes = null;
                    using (var memoryStream = new MemoryStream())
                    {
                        gmImage.ImageAttachmentStream.Seek(0, SeekOrigin.Begin);
                        gmImage.ImageAttachmentStream.CopyTo(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }

                    messageDataContext.AttachedItems.Add(new Image()
                    {
                        Source = ImageUtils.BytesToImageSource(imageBytes),
                    });
                }
                else if (attachment is Attachments.ImageLinkAttachmentControlViewModel linkedImage)
                {
                    // Linked Images aren't downloaded on the ViewModel side
                    // Just include the URL of the image
                    messageDataContext.AttachedItems.Add($"Image: {linkedImage.Url}");
                }
                else
                {
                    messageDataContext.AttachedItems.Add(attachment);
                }
            }

            var messageControl = new MessageControl()
            {
                DataContext = messageDataContext,
                Background = (Brush)Application.Current.FindResource("MessageTheySentBackdropBrush"),
                Foreground = (Brush)Application.Current.FindResource("BlackBrush"),
            };

            messageControl.Measure(new Size(500, double.PositiveInfinity));
            messageControl.ApplyTemplate();
            messageControl.UpdateLayout();
            var desiredSize = messageControl.DesiredSize;
            desiredSize.Width = Math.Max(300, desiredSize.Width);
            desiredSize.Height = Math.Min(250, desiredSize.Height);
            messageControl.Arrange(new Rect(new Point(0, 0), desiredSize));

            var bmp = new RenderTargetBitmap((int)messageControl.RenderSize.Width, (int)messageControl.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(messageControl);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using (var quotedMessageRenderPng = new MemoryStream())
            {
                encoder.Save(quotedMessageRenderPng);
                return quotedMessageRenderPng.ToArray();
            }
        }

        private async Task<Message> InjectReplyData(Message responseMessage)
        {
            var suffix = $"\n/rmid:{this.MessageBeingRepliedTo.Message.Id}";

            var renderedOriginalMessage = this.RenderMessageToPngImage(this.MessageBeingRepliedTo.Message);
            var renderedImageAttachment = await GroupMeClientApi.Models.Attachments.ImageAttachment.CreateImageAttachment(renderedOriginalMessage, this.MessageContainer);

            var attachments = responseMessage.Attachments.ToList();
            attachments.Add(renderedImageAttachment);

            var amendedMessage = Message.CreateMessage(
                responseMessage.Text + suffix,
                attachments,
                "gmdc");

            return amendedMessage;
        }

        private async Task<bool> SendMessageAsync(Message newMessage)
        {
            bool success;

            if (this.messageBeingRepliedTo != null)
            {
                newMessage = await this.InjectReplyData(newMessage);
            }

            success = await this.MessageContainer.SendMessage(newMessage);
            if (success)
            {
                this.MessageContainer.Messages.Add(newMessage);
                await this.LoadMoreAsync(null, true);

                this.TypedMessageContents = string.Empty;
            }
            else
            {
                MessageBox.Show(
                    "Could Not Send Message",
                    "GroupMe Desktop Client",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            this.IsSending = false;
            this.MessageBeingRepliedTo = null;

            return success;
        }

        private void ShowImageSendDialog(string imageFileName)
        {
            this.ShowImageSendDialog(System.IO.File.OpenRead(imageFileName));
        }

        private void ShowImageSendDialog(Stream imageData)
        {
            var dialog = new SendImageControlViewModel()
            {
                ContentStream = imageData,
                MessageContainer = this.MessageContainer,
                TypedMessageContents = this.TypedMessageContents,
                SendMessage = new RelayCommand<GroupMeClientApi.Models.Attachments.Attachment>(async (a) => await this.SendContentMessageAsync(a), (a) => !this.IsSending, true),
            };

            this.SmallDialogManager.PopupDialog = dialog;
        }

        private void ShowFileSendDialog(string fileName)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception)
            {
                try
                {
                    // When many files hosted on OneDrive are opened, they are hardlocked
                    // and cannot be opened for reading. Copying them to tmp typically is allowed though.
                    var tempFile = Utilities.TempFileUtils.GetTempFileName(fileName);
                    File.Copy(fileName, tempFile, true);
                    fileStream = File.Open(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    // The copied file will automatically be cleaned up by the temporary storage system.
                }
                catch (Exception)
                {
                }
            }

            var dialog = new SendFileControlViewModel()
            {
                ContentStream = fileStream,
                FileName = System.IO.Path.GetFileName(fileName),
                MessageContainer = this.MessageContainer,
                TypedMessageContents = this.TypedMessageContents,
                SendMessage = new RelayCommand<GroupMeClientApi.Models.Attachments.Attachment>(async (a) => await this.SendContentMessageAsync(a), (a) => !this.IsSending, true),
            };

            this.SmallDialogManager.PopupDialog = dialog;
        }

        private void ClosePopupHandler()
        {
            (this.SmallDialogManager.PopupDialog as IDisposable)?.Dispose();
            this.SmallDialogManager.PopupDialog = null;
        }

        private void ActivateGroupPlugin(GroupMeClientPlugin.GroupChat.IGroupChatPlugin plugin)
        {
            var command = new Messaging.IndexAndRunPluginRequestMessage(this.MessageContainer, plugin);
            Messenger.Default.Send(command);
        }

        private void OpenMessageSuggestionsDialog()
        {
            var dialog = new MessageEffectsControlViewModel()
            {
                TypedMessageContents = this.TypedMessageContents,
                UpdateMessage = new RelayCommand(this.UseMessageEffectSuggestion),
            };

            this.SmallDialogManager.PopupDialog = dialog;
        }

        private void UseMessageEffectSuggestion()
        {
            if (!(this.SmallDialogManager.PopupDialog is MessageEffectsControlViewModel))
            {
                return;
            }

            var messageEffectsDialog = this.SmallDialogManager.PopupDialog as MessageEffectsControlViewModel;

            this.TypedMessageContents = messageEffectsDialog.SelectedMessageContents;

            this.ClosePopupHandler();
        }

        private void SelectionChangedHandler(object data)
        {
            this.CurrentlySelectedMessages = data;
        }

        private void InitiateReplyCommand(MessageControlViewModel message)
        {
            this.MessageBeingRepliedTo = new MessageControlViewModel(message.Message, this.CacheManager, false, true, 1);
        }
    }
}