using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push.Notifications;
using System.Threading;
using System;

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

        private GroupMeClientApi.Push.PushClient pushClient;

        private GroupMeClientCached.GroupMeCachedClient GroupMeClient { get; set; }

        private SemaphoreSlim ReloadGroupsSem { get; } = new SemaphoreSlim(1, 1);

        private async Task Loaded()
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            this.GroupMeClient = new GroupMeClientCached.GroupMeCachedClient(token, "cache.db");

            pushClient = this.GroupMeClient.EnablePushNotifications();
            pushClient.NotificationReceived += PushNotificationReceived;

            this.AllGroupsChats.Clear();

            await LoadGroupsAndChats();
        }

        private async Task LoadGroupsAndChats()
        {
            await this.ReloadGroupsSem.WaitAsync();

            try
            {
                await GroupMeClient.GetGroupsAsync();
                await GroupMeClient.GetChatsAsync();

                foreach (var group in GroupMeClient.Groups())
                {
                    var existingVm = this.AllGroupsChats.FirstOrDefault(g => g.Id == group.Id);

                    if (existingVm == null)
                    {
                        // create a new GroupControl ViewModel for this Group
                        var groupVm = new Controls.GroupControlViewModel(group)
                        {
                            GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(OpenNewGroupChat, (g) => true)
                        };
                        this.AllGroupsChats.Add(groupVm);
                    }
                    else
                    {
                        // Update the existing Group
                        existingVm.MessageContainer = group;
                    }
                }

                foreach (Chat chat in GroupMeClient.Chats())
                {
                    var existingVm = this.AllGroupsChats.FirstOrDefault(g => g.Id == chat.Id);

                    if (existingVm == null)
                    {
                        // create a new GroupControl ViewModel for this Chat
                        var chatVm = new Controls.GroupControlViewModel(chat)
                        {
                            GroupSelected = new RelayCommand<Controls.GroupControlViewModel>(OpenNewGroupChat, (g) => true)
                        };
                        this.AllGroupsChats.Add(chatVm);
                    }
                    else
                    {
                        // Update the existing Chat
                        existingVm.MessageContainer = chat;
                    }
                }

                RaisePropertyChanged("AllGroupsChats");

                await this.GroupMeClient.Update();
            }
            finally
            {
                this.ReloadGroupsSem.Release();
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
                var groupContentsDisplay = new Controls.GroupContentsControlViewModel(group.MessageContainer)
                {
                    CloseGroup = new RelayCommand<Controls.GroupContentsControlViewModel>(CloseChat, (g) => true)
                };

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
            ((IDisposable)groupContentsControlViewModel).Dispose();
            
        }

        private void PushNotificationReceived(object sender, Notification notification)
        {
            switch (notification)
            {
                case LikeCreateNotification likeCreate:
                    break;

                case LineMessageCreateNotification lineCreate:
                    _ = LoadGroupsAndChats();
                    _ = RouteUpdateSignalGroup(lineCreate);
                    break;

                case DirectMessageCreateNotification directCreate:
                    _ = LoadGroupsAndChats();
                    _ = RouteUpdateSignalChat(directCreate);
                    break;

                case PingNotification _:
                default:
                    break;

            }
        }

        private async Task RouteUpdateSignalGroup(LineMessageCreateNotification notification)
        {
            var groupId = notification.Message.GroupId;
            var groupVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == groupId);

            await groupVm?.LoadNewMessages();
        }

        private async Task RouteUpdateSignalChat(DirectMessageCreateNotification notification)
        {
            var me = GroupMeClient.WhoAmI();

            // Chat IDs are formatted as UserID+UserID. Find the other user's ID
            var chatId = notification.Message.ChatId;
            var users = chatId.Split('+');
            var otherUser = users.First(u => u != me.Id);

            var chatVm = this.ActiveGroupsChats.FirstOrDefault(c => c.Id == otherUser);

            await chatVm?.LoadNewMessages();
        }
    }
}
