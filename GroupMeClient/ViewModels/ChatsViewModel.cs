using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupMeClient.ViewModels
{
    public class ChatsViewModel : ViewModelBase
    {
        public ChatsViewModel()
        {
            LoadedCommand = new RelayCommand(async () => await Loaded(), () => true);
            
            this.AllGroupsChats = new ObservableCollection<Controls.GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<Controls.GroupContentsControlViewModel>();

            //ExampleValue = 0;
            //IncrementValue = new RelayCommand(() => IncrementValueExecute(), () => true);
        }

        public ICommand LoadedCommand { get; private set; }

        public ObservableCollection<Controls.GroupControlViewModel> AllGroupsChats { get; set; }
        public ObservableCollection<Controls.GroupContentsControlViewModel> ActiveGroupsChats { get; set; }

        //public ICommand IncrementValue { get; private set; }

        //private void IncrementValueExecute()
        //{
        //    ExampleValue += 1;
        //}

        private async Task Loaded()
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            var groupMeClient = new GroupMeClientCached.GroupMeCachedClient(token, "cache.db");

            var groups = await groupMeClient.GetGroupsAsync();
            var chats = await groupMeClient.GetChatsAsync();

            this.AllGroupsChats.Clear();

            foreach (var group in groupMeClient.Groups())
            {
                var groupVm = new Controls.GroupControlViewModel(group);
                groupVm.GroupSelected = new RelayCommand<Controls.GroupControlViewModel>((g) => OpenNewGroupChat(g), (g) => true);
                this.AllGroupsChats.Add(groupVm);
            }

            foreach (Chat chat in groupMeClient.Chats())
            {
                var groupVm = new Controls.GroupControlViewModel(chat);
                groupVm.GroupSelected = new RelayCommand<Controls.GroupControlViewModel>((g) => OpenNewGroupChat(g), (g) => true);
                this.AllGroupsChats.Add(groupVm);
            }
        }

        private void OpenNewGroupChat(Controls.GroupControlViewModel group)
        {
            if (this.ActiveGroupsChats.Any(g => g.Id == group.Id))
            {
                // this group or chat is already open, we just need to move it to the front
            }
            else
            {
                // open a new group or chat
                if (group.Group != null)
                {
                    var groupContentsDisplay = new Controls.GroupContentsControlViewModel(group.Group);
                    this.ActiveGroupsChats.Add(groupContentsDisplay);
                }
                else
                {
                    var groupContentsDisplay = new Controls.GroupContentsControlViewModel(group.Chat);
                    this.ActiveGroupsChats.Add(groupContentsDisplay);
                }
            }
        }
    }
}
