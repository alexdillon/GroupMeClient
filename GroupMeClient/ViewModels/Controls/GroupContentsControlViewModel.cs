using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;

namespace GroupMeClient.ViewModels.Controls
{
    public class GroupContentsControlViewModel : ViewModelBase
    {
        public GroupContentsControlViewModel()
        {
            Messages = new ObservableCollection<MessageControlViewModel>();
            LoadedCommand = new RelayCommand(async() => await Loaded(), () => true);
        }

        public GroupContentsControlViewModel(Group group) : this()
        {
            this.group = group;
        }

        public GroupContentsControlViewModel(Chat chat) : this()
        {
            this.chat = chat;
        }

        private Group group;
        private Chat chat;

        public ICommand LoadedCommand { get; } 

        public ObservableCollection<MessageControlViewModel> Messages { get; } 

        public Group Group
        {
            get
            {
                return this.group;
            }

            set
            {
                if (this.group == value)
                {
                    return;
                }

                this.group = value;
                RaisePropertyChanged("Group");
            }
        }

        public Chat Chat
        {
            get
            {
                return this.chat;
            }

            set
            {
                if (this.chat == value)
                {
                    return;
                }

                this.chat = value;
                RaisePropertyChanged("Chat");
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
                    this.Messages.Add(new MessageControlViewModel(msg));
                }
            } 
            else if (this.Chat != null)
            {
                await chat.GetMessagesAsync();
                foreach (var msg in chat.Messages)
                {
                    this.Messages.Add(new MessageControlViewModel(msg));
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
