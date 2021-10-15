﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Controls;
using GroupMeClient.Core.Messaging;
using GroupMeClient.Core.Plugins;
using GroupMeClient.Core.Plugins.ViewModels;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Utilities;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ReactiveUI;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="GroupContentsControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.GroupContentsControl"/> control that displays the contents (messages) of a Group or Chat.
    /// Controls for sending messages are also included.
    /// </summary>
    public class GroupContentsControlViewModel : ObservableObject, IDragDropPasteTarget, IDisposable
    {
        private IMessageContainer messageContainer;
        private AvatarControlViewModel topBarAvatar;
        private string typedMessageContents = string.Empty;
        private bool isSelectionAllowed = false;
        private MessageControlViewModel messageBeingRepliedTo;
        private bool isSending;
        private double scalingFactor = 1.0;
        private bool showDisplayOptions;
        private bool showPluginOptions;
        private bool isMarkdownMode;
        public bool isFocused;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContentsControlViewModel"/> class.
        /// </summary>
        public GroupContentsControlViewModel()
        {
            this.AllMessages = new SourceList<MessageControlViewModelBase>();

            this.ReloadSem = new SemaphoreSlim(1, 1);

            this.SendMessage = new AsyncRelayCommand(this.SendMessageAsync, () => !this.IsSending);
            this.SendAttachment = new RelayCommand(this.SendFileImageAttachment);
            this.OpenMessageSuggestions = new RelayCommand(this.OpenMessageSuggestionsDialog);
            this.ReloadView = new AsyncRelayCommand(this.LoadMoreAsync);
            this.GroupChatPluginActivated = new RelayCommand<GroupMeClientPlugin.GroupChat.IGroupChatPlugin>(this.ActivateGroupPlugin);
            this.SelectionChangedCommand = new RelayCommand<object>(this.SelectionChangedHandler);
            this.InitiateReply = new RelayCommand<MessageControlViewModel>(this.InitiateReplyCommand);
            this.HideMessage = new RelayCommand<MessageControlViewModel>(this.HideMessageCommand);
            this.TerminateReply = new RelayCommand(() => this.MessageBeingRepliedTo = null);
            this.ShowMiniChat = new RelayCommand(this.PopoutMiniChat);
            this.GlobalRefreshAllCommand = new RelayCommand(this.SendGlobalRefresh);
            this.ToggleDisplayOptions = new RelayCommand(() => this.ShowDisplayOptions = !this.ShowDisplayOptions);
            this.ToggleMarkdownMode = new RelayCommand(() => this.IsMarkdownMode = !this.IsMarkdownMode);

            this.SmallDialogManager = new PopupViewModel()
            {
                ClosePopupCallback = new RelayCommand(this.ClosePopupHandler),
                EasyClosePopupCallback = null,  // EasyClose makes it too easy to accidently close the send dialog.
            };

            this.ReliabilityStateMachine = new ReliabilityStateMachine();

            this.GroupChatPlugins = new ObservableCollection<GroupMeClientPlugin.GroupChat.IGroupChatPlugin>();
            var pluginManager = Ioc.Default.GetService<IPluginManagerService>();
            foreach (var plugin in pluginManager.GroupChatPlugins)
            {
                this.GroupChatPlugins.Add(plugin);
            }

            this.MessagesSorted = new ObservableCollectionExtended<MessageControlViewModelBase>();

            this.AllMessages.AsObservableList()
                .Connect()
                .Sort(SortExpressionComparer<MessageControlViewModelBase>.Ascending(g => g.Id), SortOptions.UseBinarySearch)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(this.MessagesSorted)
                .Subscribe();

            this.CacheManager = Ioc.Default.GetService<CacheManager>();
            this.PersistManager = Ioc.Default.GetService<PersistManager>();
            this.PluginHost = Ioc.Default.GetService<PluginHost>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContentsControlViewModel"/> class.
        /// </summary>
        /// <param name="messageContainer">The Group or Chat to bind to.</param>
        /// <param name="cacheManager">The caching context in which messages are archived.</param>
        /// <param name="settings">The settings instance to use.</param>
        public GroupContentsControlViewModel(IMessageContainer messageContainer, Settings.SettingsManager settings)
            : this()
        {
            this.MessageContainer = messageContainer;
            this.Settings = settings;
            this.TopBarAvatar = new AvatarControlViewModel(this.MessageContainer, this.MessageContainer.Client.ImageDownloader);

            this.ScalingFactor = this.Settings.UISettings.ScalingFactorForMessages;

            // Generate an initial Guid to be used for the first message sent
            this.SendingMessageGuid = Guid.NewGuid().ToString();

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
        /// Gets or sets the action to be performed to close this as a MiniChat.
        /// </summary>
        public ICommand CloseMiniChat { get; set; }

        /// <summary>
        /// Gets or sets the action to be performed to register this chat as being a MiniChat.
        /// </summary>
        public ICommand RegisterAsMiniChat { get; set; }

        /// <summary>
        /// Gets the action to be performed when a message is ready to send.
        /// </summary>
        public ICommand SendMessage { get; }

        /// <summary>
        /// Gets the action to be performed when the user wants to send an attachment file.
        /// </summary>
        public ICommand SendAttachment { get; }

        /// <summary>
        /// Gets the action to be performd when the user has selected the Message Effects Generator.
        /// </summary>
        public ICommand OpenMessageSuggestions { get; }

        /// <summary>
        /// Gets the action to be performed when the user has selected markdown mode.
        /// </summary>
        public ICommand ToggleMarkdownMode { get; }

        /// <summary>
        /// Gets the action to be performed when more messages need to be loaded.
        /// </summary>
        public ICommand ReloadView { get; private set; }

        /// <summary>
        /// Gets the action to be performed when initiating a reply on a message.
        /// </summary>
        public ICommand InitiateReply { get; }

        /// <summary>
        /// Gets the action to be performed when hiding a message.
        /// </summary>
        public ICommand HideMessage { get; }

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
        /// Gets the action to be be performed to send a message to globally refresh all data
        /// in GMDC.
        /// </summary>
        public ICommand GlobalRefreshAllCommand { get; }

        /// <summary>
        /// Gets the collection of ViewModels for <see cref="Message"/>s to be displayed, sorted in ascending order.
        /// </summary>
        public IObservableCollection<MessageControlViewModelBase> MessagesSorted { get; private set; }

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
        /// Gets the action to be performed to toggle the <see cref="ShowDisplayOptions"/> property.
        /// </summary>
        public ICommand ToggleDisplayOptions { get; }

        /// <summary>
        /// Gets the action to be performed to pop out this chat as a MiniChat.
        /// </summary>
        public ICommand ShowMiniChat { get; }

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
            get => this.messageContainer;
            set => this.SetProperty(ref this.messageContainer, value);
        }

        /// <summary>
        /// Gets the avatar to be displayed in the top bar.
        /// </summary>
        public AvatarControlViewModel TopBarAvatar
        {
            get => this.topBarAvatar;
            private set => this.SetProperty(ref this.topBarAvatar, value);
        }

        /// <summary>
        /// Gets or sets the contents the user has typed for a new message.
        /// </summary>
        public string TypedMessageContents
        {
            get => this.typedMessageContents;
            set => this.SetProperty(ref this.typedMessageContents, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether selecting messages is allowed.
        /// </summary>
        public bool IsSelectionAllowed
        {
            get => this.isSelectionAllowed;
            set => this.SetProperty(ref this.isSelectionAllowed, value);
        }

        /// <summary>
        /// Gets or sets a displayable copy of a <see cref="Message"/> that is currently having a response composed for sending.
        /// If the currently composed message is not a reply, this property will be null.
        /// </summary>
        public MessageControlViewModel MessageBeingRepliedTo
        {
            get => this.messageBeingRepliedTo;
            set => this.SetProperty(ref this.messageBeingRepliedTo, value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Message"/> is currently sending.
        /// </summary>
        public bool IsSending
        {
            get => this.isSending;
            private set => this.SetProperty(ref this.isSending, value);
        }

        /// <summary>
        /// Gets or sets a value representing the scaling factor the Group Or Chat contents should be rendered at.
        /// </summary>
        public double ScalingFactor
        {
            get => this.scalingFactor;
            set => this.SetProperty(ref this.scalingFactor, value);
        }

        /// <summary>
        /// Gets a value indicating whether the current message is in markdown mode.
        /// </summary>
        public bool IsMarkdownMode
        {
            get => this.isMarkdownMode;
            private set => this.SetProperty(ref this.isMarkdownMode, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this chat currently has input focus.
        /// </summary>
        public bool IsFocused
        {
            get => this.isFocused;
            set => this.SetProperty(ref this.isFocused, value);
        }

        /// <summary>
        /// Gets a value indicating whether the display options should be shown.
        /// </summary>
        public bool ShowDisplayOptions
        {
            get => this.showDisplayOptions;
            private set
            {
                this.SetProperty(ref this.showDisplayOptions, value);
                this.showPluginOptions = false;
                this.OnPropertyChanged(nameof(this.ShowPluginOptions));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin menu should be shown.
        /// </summary>
        public bool ShowPluginOptions
        {
            get => this.showPluginOptions;
            set
            {
                this.SetProperty(ref this.showPluginOptions, value);
                this.showDisplayOptions = false;
                this.OnPropertyChanged(nameof(this.ShowDisplayOptions));
            }
        }

        private CacheManager CacheManager { get; }

        private PersistManager PersistManager { get; }

        private PluginHost PluginHost { get; }

        private SemaphoreSlim ReloadSem { get; }

        private SourceList<MessageControlViewModelBase> AllMessages { get; }

        private Message FirstDisplayedMessage { get; set; } = null;

        private DateTime LastMarkerTime { get; set; } = DateTime.MinValue;

        private ReliabilityStateMachine ReliabilityStateMachine { get; }

        private Timer RetryTimer { get; set; }

        private Settings.SettingsManager Settings { get; }

        private string SendingMessageGuid { get; set; }

        /// <summary>
        /// Reloads and redisplay the newest messages.
        /// This will capture any messages send since the last reload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadNewMessages()
        {
            await this.LoadMoreAsync(updateNewest: true);

            // Rebind the title incase the group metadata was updated
            this.OnPropertyChanged(nameof(this.Title));

            if (this.TopBarAvatar != null && this.MessageContainer.ImageOrAvatarUrl != this.TopBarAvatar.CurrentlyRenderedUrl)
            {
                // Reload the avatar if the latest URL returned doesn't match what's currently rendered.
                _ = this.TopBarAvatar.LoadAvatarAsync();
            }
        }

        /// <summary>
        /// Updates the 'Likes' for a currently displayed <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The message containing the updated list of likers.</param>
        public void UpdateMessageLikes(Message message)
        {
            var msgVm = this.AllMessages.Items.FirstOrDefault(m => m.Id == message.Id);
            if (msgVm is MessageControlViewModel messageVm)
            {
                messageVm.UpdateLikers(message.FavoritedBy);
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            this.RetryTimer?.Dispose();

            try
            {
                foreach (var msg in this.AllMessages.Items)
                {
                    (msg as IDisposable)?.Dispose();
                }
            }
            catch (Exception)
            {
            }

            this.AllMessages.Clear();
        }

        /// <inheritdoc />
        void IDragDropPasteTarget.OnFileDrop(string[] filepaths)
        {
            var supportedImageExtensions = ImageAttachment.SupportedExtensions.ToList();
            var supportedFileExtensions = FileAttachment.GroupMeDocumentMimeTypeMapper.SupportedExtensions.ToList();

            var imagesToSend = new List<string>();

            foreach (var file in filepaths)
            {
                if (supportedImageExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    imagesToSend.Add(file);
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

            if (imagesToSend.Count > 0)
            {
                this.ShowImageSendDialog(imagesToSend);
            }
        }

        /// <inheritdoc />
        void IDragDropPasteTarget.OnImageDrop(byte[] image)
        {
            var memoryStream = new MemoryStream(image);
            this.ShowImageSendDialog(new List<Stream>() { memoryStream });
        }

        private Task LoadMoreAsync()
        {
            return this.LoadMoreAsync(updateNewest: false);
        }

        private async Task LoadMoreAsync(bool updateNewest)
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

                await this.UpdateDisplay(results);

                // if everything was successful, reset the reliability monitor
                this.ReliabilityStateMachine.Succeeded();
                this.RetryTimer?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in {nameof(this.LoadMoreAsync)} - {ex.Message}. Retrying...");
                this.RetryTimer = this.ReliabilityStateMachine.GetRetryTimer(async () => await this.LoadMoreAsync(updateNewest));
            }
            finally
            {
                this.ReloadSem.Release();
            }
        }

        private async Task UpdateDisplay(ICollection<Message> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            var uiDispatcher = Ioc.Default.GetService<IUserInterfaceDispatchService>();
            await uiDispatcher.InvokeAsync(() =>
            {
                var maxTimeDifference = TimeSpan.FromMinutes(15);

                this.AllMessages.Edit(async innerList =>
                {
                    using (var persistContext = this.PersistManager.OpenNewContext())
                    {
                        // Messages retrieved with the before_id parameter are returned in descending order
                        // Reverse iterate through the messages collection to go newest->oldest
                        for (int i = messages.Count - 1; i >= 0; i--)
                        {
                            var msg = messages.ElementAt(i);

                            var oldMsg = innerList.FirstOrDefault(m => m.Id == msg.Id);
                            var messageHidden = persistContext.HiddenMessages.Find(msg.Id);
                            var messageStarred = persistContext.StarredMessages.Find(msg.Id);

                            if (oldMsg == null)
                            {
                                // Skip hidden messages
                                if (messageHidden == null)
                                {
                                    // add new message
                                    var msgVm = new MessageControlViewModel(
                                        msg,
                                        showPreviewsOnlyForMultiImages: this.Settings.UISettings.ShowPreviewsForMultiImages,
                                        isHidden: messageHidden != null,
                                        isStarred: messageStarred != null);
                                    innerList.Add(msgVm);

                                    // add an inline timestamp if needed
                                    if (msg.CreatedAtTime.Subtract(this.LastMarkerTime) > maxTimeDifference)
                                    {
                                        var messageId = long.Parse(msg.Id);
                                        var timeStampId = (messageId - 1).ToString();

                                        innerList.Add(new InlineTimestampControlViewModel(msg.CreatedAtTime, timeStampId, msgVm.DidISendIt));
                                        this.LastMarkerTime = msg.CreatedAtTime;
                                    }
                                }
                            }
                            else
                            {
                                // update an existing one if needed
                                oldMsg.Message = msg;
                            }
                        }
                    }

                    // process read receipt and sent receipts
                    if (this.MessageContainer.ReadReceipt != null)
                    {
                        // Remove old markers
                        var toRemove = innerList.OfType<InlineReadSentMarkerControlViewModel>().ToList();
                        foreach (var marker in toRemove)
                        {
                            innerList.Remove(marker);
                        }

                        // Attach a "Read Receipt" if the read message is displayed.
                        var matchedMessage = innerList.FirstOrDefault(m => m.Id == this.MessageContainer.ReadReceipt.MessageId);
                        if (matchedMessage != null)
                        {
                            var msgId = long.Parse(matchedMessage.Id);

                            var readMarker = new InlineReadSentMarkerControlViewModel(
                                this.MessageContainer.ReadReceipt.ReadAtTime,
                                true,
                                (msgId + 1).ToString(),
                                (matchedMessage as MessageControlViewModel).DidISendIt);

                            innerList.Add(readMarker);
                        }

                        // Attach a "Sent Receipt" to the last message confirmed sent by GroupMe
                        var me = this.MessageContainer.WhoAmI();
                        var lastSentMessage = innerList
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
                                lastSentMessage.DidISendIt);

                            innerList.Add(sentMarker);
                        }

                        // Send a Read Receipt for the last message received
                        var lastReceivedMessage = innerList
                            .OfType<MessageControlViewModel>()
                            .OrderByDescending(m => m.Id)
                            .FirstOrDefault(m => m.Message.UserId != me.Id);

                        if (lastReceivedMessage != null && this.MessageContainer is Chat c)
                        {
                            await c.SendReadReceipt(lastReceivedMessage.Message);
                        }
                    }
                });

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
                var clientIdentity = Ioc.Default.GetService<IClientIdentityService>();

                this.IsSending = true;
                var newMessage = Message.CreateMessage(
                    this.TypedMessageContents,
                    guidPrefix: clientIdentity.ClientGuidPrefix,
                    guid: this.SendingMessageGuid);
                await this.SendMessageAsync(newMessage);
            }
        }

        private void SendFileImageAttachment()
        {
            var supportedImages = ImageAttachment.SupportedExtensions.ToList();
            var supportedFiles = FileAttachment.GroupMeDocumentMimeTypeMapper.SupportedExtensions.ToList();

            var fileDialogService = Ioc.Default.GetService<IFileDialogService>();
            var filters = new List<FileFilter>
            {
                new FileFilter() { Name = "Images", Extensions = supportedImages },
                new FileFilter() { Name = "Documents", Extensions = supportedFiles },
            };

            var filename = fileDialogService.ShowOpenFileDialog("Select Attachment", filters);
            if (!string.IsNullOrEmpty(filename))
            {
                var extension = Path.GetExtension(filename);
                if (supportedImages.Contains(extension))
                {
                    this.ShowImageSendDialog(filename);
                }
                else if (supportedFiles.Contains(extension))
                {
                    this.ShowFileSendDialog(filename);
                }
            }
        }

        private async Task SendContentMessageAsync(List<Attachment> attachmentsList)
        {
            if (!(this.SmallDialogManager.PopupDialog is SendContentControlViewModelBase))
            {
                return;
            }

            var contentSendDialog = this.SmallDialogManager.PopupDialog as SendContentControlViewModelBase;

            if (!contentSendDialog.HasContents)
            {
                return;
            }

            contentSendDialog.IsSending = true;
            this.IsSending = true;

            var contents = contentSendDialog.TypedMessageContents;

            var clientIdentity = Ioc.Default.GetService<IClientIdentityService>();

            var message = Message.CreateMessage(
                contents,
                attachmentsList,
                guidPrefix: clientIdentity.ClientGuidPrefix,
                guid: this.SendingMessageGuid);
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

        private void InjectReplyData(Message responseMessage)
        {
            responseMessage.Attachments.RemoveAll(m => m is ReplyAttachment);
            responseMessage.Attachments.Add(ReplyAttachment.CreateReplyAttachment(this.MessageBeingRepliedTo.Message));
        }

        private Message ConvertToMarkdown(Message msg)
        {
            var clientIdentity = Ioc.Default.GetService<IClientIdentityService>();
            return Message.CreateMessage(
                msg.Text,
                msg.Attachments,
                guidPrefix: $"{clientIdentity.ClientGuidMarkdownPrefix}",
                guid: this.SendingMessageGuid);
        }

        private async Task<bool> SendMessageAsync(Message newMessage)
        {
            bool success;

            if (this.MessageBeingRepliedTo != null)
            {
                this.InjectReplyData(newMessage);
            }

            if (this.IsMarkdownMode)
            {
                newMessage = this.ConvertToMarkdown(newMessage);
            }

            success = await this.MessageContainer.SendMessage(newMessage);
            if (success)
            {
                this.MessageContainer.Messages.Add(newMessage);
                _ = this.LoadMoreAsync(updateNewest: true);

                this.TypedMessageContents = string.Empty;
                this.MessageBeingRepliedTo = null;
                this.IsMarkdownMode = false;
                this.SendingMessageGuid = Guid.NewGuid().ToString();
            }
            else
            {
                var messageBoxService = Ioc.Default.GetService<IMessageBoxService>();
                messageBoxService.ShowMessageBox(new MessageBoxParams()
                {
                    Title = "GroupMe Desktop Client",
                    Message = "Could Not Send Message",
                    MessageBoxButtons = MessageBoxParams.Buttons.Ok,
                    MessageBoxIcons = MessageBoxParams.Icon.Error,
                });
            }

            this.IsSending = false;

            return success;
        }

        private void ShowImageSendDialog(IEnumerable<string> imageFileNames)
        {
            var imagesData = new List<Stream>();

            foreach (var file in imageFileNames)
            {
                imagesData.Add(File.OpenRead(file));
            }

            this.ShowImageSendDialog(imagesData);
        }

        private void ShowImageSendDialog(string imageFileName)
        {
            this.ShowImageSendDialog(new List<Stream>() { File.OpenRead(imageFileName) });
        }

        private void ShowImageSendDialog(IEnumerable<Stream> imagesData)
        {
            var dialog = new SendImageControlViewModel()
            {
                MessageContainer = this.MessageContainer,
                TypedMessageContents = this.TypedMessageContents,
                SendMessage = new AsyncRelayCommand<List<Attachment>>(this.SendContentMessageAsync, (a) => !this.IsSending),
            };

            foreach (var image in imagesData)
            {
                dialog.ImagesCollection.Add(new SendImageControlViewModel.SendableImage(image));
            }

            this.SmallDialogManager.OpenPopup(dialog, Guid.Empty);
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
                    var tempFile = TempFileUtils.GetTempFileName(fileName);
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
                FileName = Path.GetFileName(fileName),
                MessageContainer = this.MessageContainer,
                TypedMessageContents = this.TypedMessageContents,
                SendMessage = new AsyncRelayCommand<List<Attachment>>(this.SendContentMessageAsync, (a) => !this.IsSending),
            };

            this.SmallDialogManager.OpenPopup(dialog, Guid.Empty);
        }

        private void ClosePopupHandler()
        {
            (this.SmallDialogManager.PopupDialog as IDisposable)?.Dispose();
            this.SmallDialogManager.ClosePopup();
        }

        private void ActivateGroupPlugin(GroupMeClientPlugin.GroupChat.IGroupChatPlugin plugin)
        {
            this.PluginHost.RunPlugin(this.MessageContainer, plugin);
        }

        private void OpenMessageSuggestionsDialog()
        {
            var dialog = new MessageEffectsControlViewModel()
            {
                TypedMessageContents = this.TypedMessageContents,
                UpdateMessage = new RelayCommand(this.UseMessageEffectSuggestion),
            };

            this.SmallDialogManager.OpenPopup(dialog, Guid.Empty);
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
            this.MessageBeingRepliedTo = new MessageControlViewModel(message.Message, false, true, 1);
        }

        private void HideMessageCommand(MessageControlViewModel message)
        {
            var persistManager = Ioc.Default.GetService<PersistManager>();
            using (var context = persistManager.OpenNewContext())
            {
                context.HideMessage(message.Message);
                this.AllMessages.Remove(message);
                context.SaveChanges();
            }
        }

        private void PopoutMiniChat()
        {
            var windowService = Ioc.Default.GetService<IWindowService>();
            windowService.ShowWindow(new WindowParams()
            {
                Content = this,
                Title = this.Title,
                Width = 350,
                Height = 550,
                TopMost = true,
                StartingLocation = this.Settings.UISettings.MiniChatOpenLocation,
                StartingX = this.Settings.UISettings.MiniChatManualX,
                StartingY = this.Settings.UISettings.MiniChatManualY,
                Tag = this.Id,
                CloseCallback = () => this.CloseMiniChat?.Execute(this),
            });

            this.RegisterAsMiniChat.Execute(this);
            this.ShowDisplayOptions = false;
        }

        private void SendGlobalRefresh()
        {
            WeakReferenceMessenger.Default.Send(new RefreshAllMessage());
        }
    }
}