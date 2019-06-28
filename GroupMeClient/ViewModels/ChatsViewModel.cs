using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels
{
    public class ChatsViewModel : ViewModelBase
    {
        public ChatsViewModel()
        {
            this.AllGroupsChats = new ObservableCollection<Controls.GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<Controls.GroupContentsControlViewModel>();

            _ = Loaded();
        }

        public ObservableCollection<Controls.GroupControlViewModel> AllGroupsChats { get; set; }
        public ObservableCollection<Controls.GroupContentsControlViewModel> ActiveGroupsChats { get; set; }

        private async Task Loaded()
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            var groupMeClient = new GroupMeClientCached.GroupMeCachedClient(token, "cache.db");

            var groups = await groupMeClient.GetGroupsAsync();
            var chats = await groupMeClient.GetChatsAsync();

            this.AllGroupsChats.Clear();

            foreach (var group in groupMeClient.Groups())
            {
                var groupVm = new Controls.GroupControlViewModel(group)
                {
                    GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(OpenNewGroupChat, (g) => true)
                };
                this.AllGroupsChats.Add(groupVm);
            }

            foreach (Chat chat in groupMeClient.Chats())
            {
                var groupVm = new Controls.GroupControlViewModel(chat)
                {
                    GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(OpenNewGroupChat, (g) => true)
                };
                this.AllGroupsChats.Add(groupVm);
            }
        }

        private void OpenNewGroupChat(Controls.GroupControlViewModel group)
        {
            if (this.ActiveGroupsChats.Any(g => g.Id == group.Id))
            {
                // this group or chat is already open, we just need to move it to the front
                var openGroup = this.ActiveGroupsChats.First(g => g.Id == group.Id);
                var indexOpenGroup = this.ActiveGroupsChats.IndexOf(openGroup);
                this.ActiveGroupsChats.Move(indexOpenGroup, 0);
            }
            else
            {
                // open a new group or chat
                Controls.GroupContentsControlViewModel groupContentsDisplay;

                if (group.Group != null)
                {

                    groupContentsDisplay = new Controls.GroupContentsControlViewModel(group.Group);
                }
                else
                {
                    groupContentsDisplay = new Controls.GroupContentsControlViewModel(group.Chat);
                }

                groupContentsDisplay.CloseGroup =
                    new RelayCommand<Controls.GroupContentsControlViewModel>(CloseChat, (g) => true);
                this.ActiveGroupsChats.Insert(0, groupContentsDisplay);
            }

            // limit to three multi-chats at a time
            while (this.ActiveGroupsChats.Count > 3)
            {
                this.ActiveGroupsChats.RemoveAt(this.ActiveGroupsChats.Count - 1);
            }
        }

        private void CloseChat(Controls.GroupContentsControlViewModel groupContentsControlViewModel)
        {
            this.ActiveGroupsChats.Remove(groupContentsControlViewModel);
        }
    }
}
