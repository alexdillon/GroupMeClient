using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="PaginatedMessagesControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.PaginatedMessagesControlViewModel"/> control that displays paginated messages.
    /// Controls for sending messages are also included.
    /// </summary>
    public class PaginatedMessagesControlViewModel : ViewModelBase
    {
        private readonly TimeSpan maxMarkerDistanceTime = TimeSpan.FromMinutes(15);
        private IQueryable<Message> messages;
        private MessageControlViewModelBase selectedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedMessagesControlViewModel"/> class.
        /// </summary>
        public PaginatedMessagesControlViewModel()
        {
            this.CurrentPage = new ObservableCollection<MessageControlViewModelBase>();
            this.MessagesPerPage = 50;

            this.GoBackCommand = new RelayCommand(this.GoBack, this.CanGoBack);
            this.GoForwardCommand = new RelayCommand(this.GoForward, this.CanGoForward);
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
        /// Gets the currently displayed page number.
        /// </summary>
        public int CurrentPageNumber { get; private set; }

        /// <summary>
        /// Gets the total number of messages displayed in all pages.
        /// </summary>
        public int TotalMessagesCount { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Group"/> or <see cref="Chat"/> the displayed messages are associated with.
        /// </summary>
        public IMessageContainer AssociateWith { get; set; }

        /// <summary>
        /// Gets the action to be performed when the back button is clicked.
        /// </summary>
        public RelayCommand GoBackCommand { get; }

        /// <summary>
        /// Gets the action to be performed when the forward/next button is clicked.
        /// </summary>
        public RelayCommand GoForwardCommand { get; }

        /// <summary>
        /// Gets or sets the action to be performed when a <see cref="Message"/> is selected.
        /// </summary>
        public ICommand MessageSelectedCommand { get; set; }

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
                    var startingMessageNum = (this.CurrentPageNumber * this.MessagesPerPage) + 1;
                    var endingMessageNum = Math.Min(
                        (this.CurrentPageNumber * this.MessagesPerPage) + this.MessagesPerPage,
                        this.TotalMessagesCount);

                    return $"Showing {startingMessageNum}-{endingMessageNum} of {this.TotalMessagesCount} Results";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the complete set of <see cref="Message"/>s to paginate and display.
        /// </summary>
        public IQueryable<Message> Messages
        {
            get
            {
                return this.messages;
            }

            set
            {
                this.DisposeClearPage();

                this.Set(() => this.Messages, ref this.messages, value);

                this.TotalMessagesCount = this.Messages?.Count() ?? 0;
                this.LastMarkerTime = DateTime.MaxValue;
                this.ChangePage();
            }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Message"/>.
        /// </summary>
        public MessageControlViewModelBase SelectedMessage
        {
            get { return this.selectedMessage; }
            set { this.Set(() => this.SelectedMessage, ref this.selectedMessage, value); }
        }

        private DateTime LastMarkerTime { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// Ensures the page containing a specific <see cref="Message"/> is displayed.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void EnsureVisible(Message message)
        {
            var temp = this.Messages.ToList();
            var index = temp.FindIndex(m => m.Id == message.Id);

            int pageNumber = (int)Math.Floor((double)index / this.MessagesPerPage);
            this.ChangePage(pageNumber);
            this.SelectedMessage = this.CurrentPage.First(m => m.Id == message.Id);
        }

        private void ChangePage(int pageNumber = 0)
        {
            this.DisposeClearPage();
            this.CurrentPageNumber = pageNumber;

            if (this.Messages == null)
            {
                return;
            }

            var range = this.Messages.Skip(pageNumber * this.MessagesPerPage).Take(this.MessagesPerPage);

            foreach (var msg in range)
            {
                if (this.AssociateWith is Group g)
                {
                    msg.AssociateWithGroup(g);
                }
                else if (this.AssociateWith is Chat c)
                {
                    msg.AssociateWithChat(c);
                }

                var msgVm = new MessageControlViewModel(msg);

                // add an inline timestamp if needed
                if (this.LastMarkerTime.Subtract(msg.CreatedAtTime) > this.maxMarkerDistanceTime)
                {
                    this.CurrentPage.Add(new InlineTimestampControlViewModel(msg.CreatedAtTime, "id-not-used", msgVm.MessageColor));
                    this.LastMarkerTime = msg.CreatedAtTime;
                }

                this.CurrentPage.Add(msgVm);
            }

            this.GoBackCommand.RaiseCanExecuteChanged();
            this.GoForwardCommand.RaiseCanExecuteChanged();
            this.RaisePropertyChanged(nameof(this.Title));
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
            this.ChangePage(this.CurrentPageNumber - 1);
        }

        private bool CanGoBack()
        {
            return this.CurrentPageNumber > 0;
        }

        private void GoForward()
        {
            this.ChangePage(this.CurrentPageNumber + 1);
        }

        private bool CanGoForward()
        {
            return ((this.CurrentPageNumber * this.MessagesPerPage) + this.MessagesPerPage) < this.TotalMessagesCount;
        }
    }
}