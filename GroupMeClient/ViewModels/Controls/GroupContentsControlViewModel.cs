using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Data;
using System.Windows;

namespace GroupMeClient.ViewModels.Controls
{
    public class GroupContentsControlViewModel : ViewModelBase
    {
        public GroupContentsControlViewModel()
        {
            this.Messages = new ObservableCollection<MessageControlViewModel>();
            this.SendMessage = new RelayCommand(async () => await SendMessageAsync(), true);
            this.ReloadView = new RelayCommand<ScrollViewer>(async (s) => await LoadMoreAsync(s), true);
        }

        public GroupContentsControlViewModel(Group group) : this()
        {
            this.Group = group;
            this.TopBarAvatar = new AvatarControlViewModel(this.Group);

            _ = Loaded();
        }

        public GroupContentsControlViewModel(Chat chat) : this()
        {
            this.Chat = chat;
            this.TopBarAvatar = new AvatarControlViewModel(this.Chat);

            _ = Loaded();
        }

        private Group group;
        private Chat chat;
        private AvatarControlViewModel topBarAvatar;
        private string typedMessageContents;

        public ICommand CloseGroup { get; set; }
        public ICommand SendMessage { get; set; }
        public ICommand ReloadView { get; set; }

        public ObservableCollection<MessageControlViewModel> Messages { get; }

        private SemaphoreSlim ReloadSem { get; } = new SemaphoreSlim(1, 1);

        public Group Group
        {
            get { return this.group; }
            set { Set(() => this.Group, ref group, value); }
        }

        public Chat Chat
        {
            get { return this.chat; }
            set { Set(() => this.Chat, ref chat, value); }
        }

        public string Title => this.Group?.Name ?? this.Chat?.OtherUser.Name;

        public string Id => this.Group?.Id ?? this.Chat?.Id;

        public AvatarControlViewModel TopBarAvatar
        {
            get { return this.topBarAvatar; }
            set { Set(() => this.TopBarAvatar, ref topBarAvatar, value); }
        }

        public string TypedMessageContents
        {
            get { return this.typedMessageContents; }
            set { Set(() => this.TypedMessageContents, ref typedMessageContents, value); }
        }

        private Message FirstDisplayedMessage { get; set; } = null;

        public async Task LoadNewMessages()
        {
            await Application.Current.Dispatcher.Invoke(async () =>
            {
                // the code that's accessing UI properties
                await LoadMoreAsync(null, true);
            });
        }

        private async Task Loaded()
        {
            await LoadMoreAsync();
        }

        private async Task LoadMoreAsync(ScrollViewer scrollViewer = null, bool updateNewest = false)
        {
            await this.ReloadSem.WaitAsync();

            try
            {
                if (this.Group != null)
                {
                    await LoadMoreGroupMessagesAsync(scrollViewer, updateNewest);
                }
                else if (this.Chat != null)
                {
                    await LoadMoreChatMessagesAsync(scrollViewer, updateNewest);
                }
            }
            finally
            {
                this.ReloadSem.Release();
            }
        }

        private async Task LoadMoreGroupMessagesAsync(ScrollViewer scrollViewer = null, bool updateNewest = false)
        {
            ICollection<Message> results;

            if (this.FirstDisplayedMessage == null || updateNewest)
            {
                // load the most recent messages
                results = await group.GetMessagesAsync();
            }
            else
            {
                // load the 20 messages preceeding the oldest (first) one displayed
                results = await group.GetMessagesAsync(20, GroupMeClientApi.MessageRetreiveMode.BeforeId, this.FirstDisplayedMessage.Id);
            }

            UpdateDisplay(scrollViewer, results);
        }

        private async Task LoadMoreChatMessagesAsync(ScrollViewer scrollViewer = null, bool updateNewest = false)
        {
            ICollection<Message> results;

            if (this.FirstDisplayedMessage == null || updateNewest)
            {
                // load the most recent messages
                results = await this.Chat.GetMessagesAsync();
            }
            else
            {
                // load the 20 messages preceeding the oldest (first) one displayed
                results = await this.Chat.GetMessagesAsync(GroupMeClientApi.MessageRetreiveMode.BeforeId, this.FirstDisplayedMessage.Id);
            }

            UpdateDisplay(scrollViewer, results);
        }

        private void UpdateDisplay(ScrollViewer scrollViewer, ICollection<Message> messages)
        {
            double originalHeight = scrollViewer?.ExtentHeight ?? 0.0;
            if (originalHeight != 0)
            {
                // prevent the At Top event from firing while we are adding new messages
                scrollViewer.ScrollToVerticalOffset(1);
            }

            foreach (var msg in messages)
            {
                var oldMsg = this.Messages.FirstOrDefault(m => m.Id == msg.Id);

                if (oldMsg == null)
                {
                    // add new message
                    this.Messages.Add(new MessageControlViewModel(msg));
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

            this.TypedMessageContents = string.Empty;

            bool success;

            if (this.Group != null)
            {
                success = await this.Group.SendMessage(newMessage);
                if (success)
                {
                    this.Group.Messages.Add(newMessage);
                    await this.LoadMoreAsync(null, true);
                }
            }
            else if (this.Chat != null)
            {
                success = await this.Chat.SendMessage(newMessage);
                if (success)
                {
                    this.Chat.Messages.Add(newMessage);
                    await this.LoadMoreAsync(null, true);
                }
            }
        }
    }
}