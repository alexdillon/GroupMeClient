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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Extensions;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="GroupContentsControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.GroupContentsControl"/> control that displays the contents (messages) of a Group or Chat.
    /// Controls for sending messages are also included.
    /// </summary>
    public class GroupContentsControlViewModel : ViewModelBase, FileDragDropHelper.IDragDropTarget, IDisposable
    {
        private IMessageContainer messageContainer;
        private AvatarControlViewModel topBarAvatar;
        private string typedMessageContents;
        private SendImageControlViewModel imageSendDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContentsControlViewModel"/> class.
        /// </summary>
        public GroupContentsControlViewModel()
        {
            this.Messages = new ObservableCollection<MessageControlViewModelBase>();
            this.ReloadSem = new SemaphoreSlim(1, 1);
            this.SendMessage = new RelayCommand(async () => await this.SendMessageAsync(), true);
            this.ReloadView = new RelayCommand<ScrollViewer>(async (s) => await this.LoadMoreAsync(s), true);
            this.ClosePopup = new RelayCommand(this.ClosePopupHandler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContentsControlViewModel"/> class.
        /// </summary>
        /// <param name="messageContainer">The Group or Chat to bind to.</param>
        public GroupContentsControlViewModel(IMessageContainer messageContainer)
            : this()
        {
            this.MessageContainer = messageContainer;
            this.TopBarAvatar = new AvatarControlViewModel(this.MessageContainer, this.MessageContainer.Client.ImageDownloader);

            _ = this.LoadMoreAsync();
        }

        /// <summary>
        /// Gets or sets the action to be performed when the close button is pressed.
        /// </summary>
        public ICommand CloseGroup { get; set; }

        /// <summary>
        /// Gets or sets the action to be performd when a message is ready to send.
        /// </summary>
        public ICommand SendMessage { get; set; }

        /// <summary>
        /// Gets the action to be performed when more messages need to be loaded.
        /// </summary>
        public ICommand ReloadView { get; private set; }

        /// <summary>
        /// Gets or sets the action to be be performed when a popup has been closed.
        /// </summary>
        public ICommand ClosePopup { get; set; }

        /// <summary>
        /// Gets the collection of ViewModels for <see cref="Message"/>s to be displayed.
        /// </summary>
        public ObservableCollection<MessageControlViewModelBase> Messages { get; }

        /// <summary>
        /// Gets the title of the <see cref="Group"/> or <see cref="Chat"/>.
        /// </summary>
        public string Title => this.MessageContainer.Name;

        /// <summary>
        /// Gets the unique identifier for the <see cref="Group"/> or <see cref="Chat"/>.
        /// </summary>
        public string Id => this.MessageContainer.Id;

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
        /// Gets the Image Send Dialog that should be displayed as a popup.
        /// Gets null if no Image Send Dialog should be displayed.
        /// </summary>
        public SendImageControlViewModel ImageSendDialog
        {
            get { return this.imageSendDialog; }
            private set { this.Set(() => this.ImageSendDialog, ref this.imageSendDialog, value); }
        }

        private SemaphoreSlim ReloadSem { get; }

        private Message FirstDisplayedMessage { get; set; } = null;

        /// <summary>
        /// Reloads and redisplay the newest messages.
        /// This will capture any messages send since the last reload.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadNewMessages()
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                // the code that's accessing UI properties
                await this.LoadMoreAsync(null, true);
            });
        }

        /// <summary>
        /// Updates the 'Likes' for a currently displayed <see cref="Message"/>.
        /// </summary>
        /// <param name="message">The message containing the updated list of likers.</param>
        public void UpdateMessageLikes(Message message)
        {
            var msgVm = this.Messages.FirstOrDefault(m => m.Id == message.Id);

            // Only update the display copy and leave the cached copy alone
            // Cached copy is never used for displaying 'Like' status
            msgVm.Message.FavoritedBy.Clear();
            foreach (var liker in message.FavoritedBy)
            {
                msgVm.Message.FavoritedBy.Add(liker);
            }

            msgVm.UpdateDisplay();
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            this.Messages.Clear();
            /*foreach (var msg in this.Messages)
            {
                (msg as IDisposable)?.Dispose();
            }*/
        }

        /// <inheritdoc />
        void FileDragDropHelper.IDragDropTarget.OnFileDrop(string[] filepaths)
        {
            string[] supportedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

            foreach (var file in filepaths)
            {
                if (supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                {
                    this.ShowImageSendDialog(File.OpenRead(file));
                    break;
                }
            }
        }

        /// <inheritdoc />
        void FileDragDropHelper.IDragDropTarget.OnImageDrop(byte[] image)
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
                }
                else
                {
                    // load the 20 (GroupMe default) messages preceeding the oldest (first) one displayed
                    results = await this.MessageContainer.GetMessagesAsync(GroupMeClientApi.MessageRetreiveMode.BeforeId, this.FirstDisplayedMessage.Id);
                }

                this.UpdateDisplay(scrollViewer, results);
            }
            finally
            {
                this.ReloadSem.Release();
            }
        }

        private void UpdateDisplay(ScrollViewer scrollViewer, ICollection<Message> messages)
        {
            double originalHeight = scrollViewer?.ExtentHeight ?? 0.0;
            if (originalHeight != 0)
            {
                // prevent the At Top event from firing while we are adding new messages
                scrollViewer.ScrollToVerticalOffset(1);
            }

            var maxTimeDifference = TimeSpan.FromMinutes(15);

            var lastMarkerTime = DateTime.MinValue;

            // Messages retrieved with the before_id parameter are returned in descending order
            // Reverse iterate through the messages collection to go newest->oldest
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                var msg = messages.ElementAt(i);

                var oldMsg = this.Messages.FirstOrDefault(m => m.Id == msg.Id);

                if (oldMsg == null)
                {
                    // add new message
                    var msgVm = new MessageControlViewModel(msg);
                    this.Messages.Add(msgVm);

                    // add an inline timestamp if needed
                    if (msg.CreatedAtTime.Subtract(lastMarkerTime) > maxTimeDifference)
                    {
                        var messageId = long.Parse(msg.Id);
                        var timeStampId = (messageId - 1).ToString();

                        this.Messages.Add(new InlineTimestampControlViewModel(msg.CreatedAtTime, timeStampId, msgVm.MessageColor));
                        lastMarkerTime = msg.CreatedAtTime;
                    }
                }
                else
                {
                    // update and existing one if needed
                    oldMsg.Message = msg;
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
        }

        private async Task SendMessageAsync()
        {
            var newMessage = Message.CreateMessage(this.TypedMessageContents);
            await this.SendMessageAsync(newMessage);
        }

        private async Task SendImageMessageAsync()
        {
            this.ImageSendDialog.IsSending = true;

            var contents = this.ImageSendDialog.TypedMessageContents;
            byte[] image;

            using (var ms = new MemoryStream())
            {
                this.ImageSendDialog.ImageStream.Seek(0, SeekOrigin.Begin);
                await this.ImageSendDialog.ImageStream.CopyToAsync(ms);
                image = ms.ToArray();
            }

            GroupMeClientApi.Models.Attachments.ImageAttachment attachment;
            attachment = await GroupMeClientApi.Models.Attachments.ImageAttachment.CreateImageAttachment(image, this.MessageContainer);

            var attachmentsList = new List<GroupMeClientApi.Models.Attachments.Attachment> { attachment };

            var message = Message.CreateMessage(contents, attachmentsList);
            await this.SendMessageAsync(message);

            this.ClosePopupHandler();
        }

        private async Task<bool> SendMessageAsync(Message newMessage)
        {
            this.TypedMessageContents = string.Empty;

            bool success;

            success = await this.MessageContainer.SendMessage(newMessage);
            if (success)
            {
                this.MessageContainer.Messages.Add(newMessage);
                await this.LoadMoreAsync(null, true);
            }

            return success;
        }

        private void ShowImageSendDialog(byte[] image)
        {
            var ms = new MemoryStream(image);
            this.ShowImageSendDialog(ms);
        }

        private void ShowImageSendDialog(Stream image)
        {
            var dialog = new SendImageControlViewModel()
            {
                ImageStream = image,
                TypedMessageContents = this.TypedMessageContents,
                SendMessage = new RelayCommand(async () => await this.SendImageMessageAsync(), true),
            };

            this.ImageSendDialog = dialog;
        }

        private void ClosePopupHandler()
        {
            (this.ImageSendDialog as IDisposable)?.Dispose();
            this.ImageSendDialog = null;
        }
    }
}