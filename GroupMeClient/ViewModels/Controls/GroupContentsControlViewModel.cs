using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GroupMeClient.ViewModels.Controls
{
    public class GroupContentsControlViewModel : ViewModelBase
    {
        public GroupContentsControlViewModel()
        {
            Messages = new ObservableCollection<MessageControlViewModel>();
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

        public ICommand CloseGroup { get; set; }

        public ObservableCollection<MessageControlViewModel> Messages { get; }

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

        public AvatarControlViewModel TopBarAvatar
        {
            get { return this.topBarAvatar; }
            set { Set(() => this.TopBarAvatar, ref topBarAvatar, value); }
        }

        public string Title
        {
            get
            {
                var title = this.Group?.Name ?? this.Chat?.OtherUser.Name;
                return title;
            }
        }

        private async Task Loaded()
        {
            // for the initial load, call ignore the return from the GetMessage call
            // and bind everything from the Messages list instead. New ones will be automatically added

            if (this.Group != null)
            {
                await group.GetMessagesAsync();
                foreach (var msg in group.Messages)
                {
                    if (!this.Messages.Any(m => m.Id == msg.Id))
                    {
                        this.Messages.Add(new MessageControlViewModel(msg));
                    }
                }
            }
            else if (this.Chat != null)
            {
                await chat.GetMessagesAsync();
                foreach (var msg in chat.Messages)
                {
                    if (!this.Messages.Any(m => m.Id == msg.Id))
                    {
                        this.Messages.Add(new MessageControlViewModel(msg));
                    }
                }
            }
        }

        public string Id
        {
            get
            {
                return this.Group?.Id ?? this.Chat?.Id;
            }
        }
    }
}
