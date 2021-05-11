﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Services;
using GroupMeClientApi.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="PaginatedMessagesControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.PaginatedMessagesControlViewModel"/> control that displays paginated messages.
    /// Controls for sending messages are also included.
    /// </summary>
    public class PaginatedMessagesControlViewModel : ObservableObject
    {
        private readonly TimeSpan maxMarkerDistanceTime = TimeSpan.FromMinutes(15);
        private IQueryable<Message> messages;
        private MessageControlViewModelBase selectedMessage;
        private bool newestAtBottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedMessagesControlViewModel"/> class.
        /// </summary>
        public PaginatedMessagesControlViewModel()
        {
            this.CurrentPage = new ObservableCollection<MessageControlViewModelBase>();
            this.MessagesPerPage = 25;

            this.GoBackCommand = new RelayCommand(this.GoBack, this.CanGoBack);
            this.GoForwardCommand = new RelayCommand(this.GoForward, this.CanGoForward);

            this.InitiateReply = new RelayCommand<MessageControlViewModel>(this.InitiateReplyCommand);

            this.NewestAtBottom = false;
        }

        /// <summary>
        /// Gets the collection of ViewModels for <see cref="Message"/>s to be displayed.
        /// </summary>
        public ObservableCollection<MessageControlViewModelBase> CurrentPage { get; }

        /// <summary>
        /// Gets or sets the number of <see cref="Message"/>s displayed per page.
        /// </summary>
        public int MessagesPerPage { get; set; }

        /// <summary>
        /// Gets the total number of messages displayed in all pages.
        /// </summary>
        public int TotalMessagesCount { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Group"/> or <see cref="Chat"/> the displayed messages are associated with.
        /// </summary>
        public IMessageContainer AssociateWith { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Like' status will be shown on Messages.
        /// </summary>
        public bool? ShowLikers { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether displayed messages will be sychronized with GroupMe,
        /// and displayed with up-to-date information.
        /// </summary>
        public bool SyncAndUpdate { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the ordering of <see cref="Messages"/>
        /// places new messages at the bottom.
        /// This value is used when placing inline timestamps.
        /// </summary>
        public bool NewestAtBottom
        {
            get
            {
                return this.newestAtBottom;
            }

            set
            {
                this.newestAtBottom = value;
                this.LastMarkerTime = this.NewestAtBottom ? DateTime.MinValue : DateTime.MaxValue;
            }
        }

        /// <summary>
        /// Gets the action to be performed when the back button is clicked.
        /// </summary>
        public ICommand GoBackCommand { get; }

        /// <summary>
        /// Gets the action to be performed when the forward/next button is clicked.
        /// </summary>
        public ICommand GoForwardCommand { get; }

        /// <summary>
        /// Gets or sets the action to be performed when a <see cref="Message"/> is selected.
        /// </summary>
        public ICommand MessageSelectedCommand { get; set; }

        /// <summary>
        /// Gets the action to be performed to initiate composing a reply to a specific message.
        /// </summary>
        public ICommand InitiateReply { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a title is shown.
        /// </summary>
        public bool ShowTitle { get; set; } = true;

        /// <summary>
        /// Gets the title to display above the messages view.
        /// </summary>
        public string Title
        {
            get
            {
                if (this.ShowTitle && this.TotalMessagesCount != 0)
                {
                    var startingMessageNum = (this.CurrentPageTop * this.MessagesPerPage) + 1;
                    var endingMessageNum = Math.Min(
                        (this.CurrentPageBottom * this.MessagesPerPage) + this.MessagesPerPage,
                        this.TotalMessagesCount);

                    return $"Showing {startingMessageNum}-{startingMessageNum + endingMessageNum - 1} of {this.TotalMessagesCount} Results";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the complete set of <see cref="Message"/>s to paginate and display.
        /// </summary>
        public IQueryable<Message> Messages
        {
            get => this.messages;
            private set => this.SetProperty(ref this.messages, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Message"/>.
        /// </summary>
        public MessageControlViewModelBase SelectedMessage
        {
            get => this.selectedMessage;
            set => this.SetProperty(ref this.selectedMessage, value);
        }

        private DateTime LastMarkerTime { get; set; }

        private CacheManager.CacheContext CurrentlyDisplayedCacheContext { get; set; }

        private int CurrentPageTop { get; set; }

        private int CurrentPageBottom { get; set; }

        /// <summary>
        /// Displays a collection of messages in the control.
        /// </summary>
        /// <param name="messages">The <see cref="Message"/>s to display.</param>
        /// <param name="cacheContext">The cache context the displayed messages belong to.</param>
        public void DisplayMessages(IQueryable<Message> messages, CacheManager.CacheContext cacheContext)
        {
            this.DisposeClearPage();
            this.CurrentlyDisplayedCacheContext?.Dispose();
            this.CurrentPageTop = 0;
            this.CurrentPageBottom = 0;

            this.Messages = messages;
            this.CurrentlyDisplayedCacheContext = cacheContext;

            this.TotalMessagesCount = this.Messages?.Count() ?? 0;

            // Reset timestamp ordering
            this.NewestAtBottom = this.NewestAtBottom;
        }

        /// <summary>
        /// Ensures the page containing a specific <see cref="Message"/> is displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EnsureVisible(Message message)
        {
            var preceedingMessages = this.Messages
                .Where(m => m.Id.CompareTo(message.Id) < 0)
                .OrderBy(m => m.Id);

            int index = preceedingMessages.Count();
            int pageNumber = (int)Math.Floor((double)index / this.MessagesPerPage);
            this.CurrentPageTop = pageNumber;
            this.CurrentPageBottom = pageNumber;
            await this.LoadPage(pageNumber);
            this.SelectedMessage = this.CurrentPage.First(m => m.Id == message.Id);
        }

        /// <summary>
        /// Changes the currently displayed page.
        /// </summary>
        /// <param name="pageNumber">The page number to display.</param>
        /// <param name="isUp">
        /// The direction the page is being loaded in. True for up, false for down.
        /// Use null for initially loaded pages that don't have an associated direction.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadPage(int pageNumber = 0, bool? isUp = null)
        {
            var uiDispatcher = Ioc.Default.GetService<IUserInterfaceDispatchService>();
            await uiDispatcher.InvokeAsync(() =>
            {
                var range = this.Messages.Skip(pageNumber * this.MessagesPerPage).Take(this.MessagesPerPage);

                IEnumerable<Message> displayRange = range;
                if (this.SyncAndUpdate)
                {
                    displayRange = this.GetFromGroupMe(range.First(), range.Last());
                }

                if (isUp == true)
                {
                    displayRange = displayRange.Reverse();
                }

                foreach (var msg in displayRange)
                {
                    if (this.AssociateWith is Group g)
                    {
                        msg.AssociateWithGroup(g);
                    }
                    else if (this.AssociateWith is Chat c)
                    {
                        msg.AssociateWithChat(c);
                    }

                    var msgVm = new MessageControlViewModel(
                        msg,
                        showLikers: this.ShowLikers);

                    // add an inline timestamp if needed
                    if ((this.NewestAtBottom && msg.CreatedAtTime.Subtract(this.LastMarkerTime) > this.maxMarkerDistanceTime) ||
                        (!this.NewestAtBottom && this.LastMarkerTime.Subtract(msg.CreatedAtTime) > this.maxMarkerDistanceTime))
                    {
                        this.CurrentPage.Add(new InlineTimestampControlViewModel(msg.CreatedAtTime, "id-not-used", msgVm.DidISendIt));
                        this.LastMarkerTime = msg.CreatedAtTime;
                    }

                    if (isUp == true)
                    {
                        this.CurrentPage.Insert(0, msgVm);
                    }
                    else
                    {
                        this.CurrentPage.Add(msgVm);
                    }
                }

                this.OnPropertyChanged(nameof(this.Title));
            });
        }

        private void DisposeClearPage()
        {
            foreach (var msg in this.CurrentPage)
            {
                (msg as IDisposable).Dispose();
            }

            this.CurrentPage.Clear();
        }

        private void GoBack()
        {
            this.CurrentPageTop--;
            _ = this.LoadPage(this.CurrentPageTop, true);
        }

        private bool CanGoBack()
        {
            return this.CurrentPageTop > 0;
        }

        private void GoForward()
        {
            this.CurrentPageBottom++;
            _ = this.LoadPage(this.CurrentPageBottom, false);
        }

        private bool CanGoForward()
        {
            return ((this.CurrentPageBottom * this.MessagesPerPage) + this.MessagesPerPage) < this.TotalMessagesCount;
        }

        private IEnumerable<Message> GetFromGroupMe(Message startAt, Message endAt)
        {
            return Task.Run(async () => await this.GetFromGroupMeAsync(startAt, endAt)).Result;
        }

        private async Task<IEnumerable<Message>> GetFromGroupMeAsync(Message startAt, Message endAt)
        {
            var result = new List<Message>(this.MessagesPerPage);
            long.TryParse(startAt.Id, out var startId);
            long.TryParse(endAt.Id, out var endId);

            // GroupMe only allows before_id searches on Chat's (not after_id), so we have to go backwards...
            // Add 1 to include the endAt message in the returned data.
            long currentId = endId + 1;
            while (currentId > startId)
            {
                var msgs = await this.AssociateWith.GetMessagesAsync(GroupMeClientApi.MessageRetreiveMode.BeforeId, currentId.ToString());
                result.AddRange(msgs);

                currentId = long.Parse(msgs.Last().Id);
            }

            // Since we went backwards, reverse the list.
            result.Reverse();

            // GroupMe block sizes might not align with the pagination page-sizes.
            // Cut to match the expected page size.
            int startIndex = result.FindIndex(m => m.Id == startAt.Id);
            return result.Skip(startIndex);
        }

        private void InitiateReplyCommand(MessageControlViewModel message)
        {
            var groupOrChatId = message.Message.GroupId ?? message.Message.Chat?.OtherUser.Id;

            WeakReferenceMessenger.Default.Send(new Messaging.SwitchToPageRequestMessage(Messaging.SwitchToPageRequestMessage.Page.Chats));

            WeakReferenceMessenger.Default.Send(new Messaging.ShowChatRequestMessage(
                groupOrChatId: groupOrChatId,
                startReply: message));
        }
    }
}