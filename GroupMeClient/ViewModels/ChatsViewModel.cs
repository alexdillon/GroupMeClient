using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
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
            ShowPopUp = new RelayCommand(() => ShowPopUpExecute(), () => true);
            IncrementValue = new RelayCommand(() => IncrementValueExecute(), () => true);
            LoadedCommand = new RelayCommand(async () => await Loaded(), () => true);
            ExampleValue = 0;

            this.ActiveGroups = new ObservableCollection<Controls.GroupControlViewModel>();
        }

        public ICommand ShowPopUp { get; private set; }

        public ICommand IncrementValue { get; private set; }

        public ICommand LoadedCommand { get; private set; }

        private static void ShowPopUpExecute()
        {
            MessageBox.Show("Hello World!");
        }

        private void IncrementValueExecute()
        {
            ExampleValue += 1;
        }

        private async Task Loaded()
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            var groupMeClient = new GroupMeClientCached.GroupMeCachedClient(token, "cache.db");

            var groups = await groupMeClient.GetGroupsAsync();
            //var messagesInFirstGroup = await groups[0].GetMessagesAsync();

            //var chats = await groupMeClient.GetChatsAsync();
            //var messagesInFirstChat = await chats[0].GetMessagesAsync();

            this.ActiveGroups.Clear();
            foreach (var group in groupMeClient.Groups)
            {
                this.ActiveGroups.Add(new Controls.GroupControlViewModel(group));
            }
        }

        int _exampleValue;

        public ObservableCollection<ViewModels.Controls.GroupControlViewModel> ActiveGroups { get; set; }

        public int ExampleValue
        {
            get
            {
                return _exampleValue;
            }
            set
            {
                if (_exampleValue == value)
                    return;
                _exampleValue = value;
                RaisePropertyChanged("ExampleValue");
            }
        }
    }
}
