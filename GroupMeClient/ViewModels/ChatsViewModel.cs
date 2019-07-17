﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push.Notifications;
using System.Threading;
using GroupMeClient.Notifications;
using GroupMeClientApi.Push;

namespace GroupMeClient.ViewModels
{
    public class ChatsViewModel : ViewModelBase, INotificationSink
    {
        public ChatsViewModel(GroupMeClientApi.GroupMeClient groupMeClient)
        {
            this.GroupMeClient = groupMeClient;

            this.AllGroupsChats = new ObservableCollection<Controls.GroupControlViewModel>();
            this.ActiveGroupsChats = new ObservableCollection<Controls.GroupContentsControlViewModel>();
       
            _ = Loaded();
        }

        public ObservableCollection<Controls.GroupControlViewModel> AllGroupsChats { get; set; }
        public ObservableCollection<Controls.GroupContentsControlViewModel> ActiveGroupsChats { get; set; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }
        private PushClient PushClient { get; set; }

        private SemaphoreSlim ReloadGroupsSem { get; } = new SemaphoreSlim(1, 1);

        private async Task Loaded()
        {
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

                _ = this.PushClient.SubscribeAsync(group.MessageContainer);
            }

            // limit to three multi-chats at a time
            while (this.ActiveGroupsChats.Count > 3)
            {
                var removeGroup = this.ActiveGroupsChats.Last();
                this.PushClient.Unsubscribe(group.MessageContainer);

                this.ActiveGroupsChats.Remove(removeGroup);
            }
        }

        private void CloseChat(Controls.GroupContentsControlViewModel groupContentsControlViewModel)
        {
            this.ActiveGroupsChats.Remove(groupContentsControlViewModel);
            this.PushClient.Unsubscribe(groupContentsControlViewModel.MessageContainer);

            ((IDisposable)groupContentsControlViewModel).Dispose();
        }

        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification)
        {
            _ = this.LoadGroupsAndChats();

            var groupId = notification.Message.GroupId;
            var groupVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == groupId);

            await groupVm?.LoadNewMessages();
        }

        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification)
        {
            _ = this.LoadGroupsAndChats();

            var me = GroupMeClient.WhoAmI();

            // Chat IDs are formatted as UserID+UserID. Find the other user's ID
            var chatId = notification.Message.ChatId;
            var users = chatId.Split('+');
            var otherUser = users.First(u => u != me.Id);

            var chatVm = this.ActiveGroupsChats.FirstOrDefault(c => c.Id == otherUser);

            await chatVm?.LoadNewMessages();
        }

        void INotificationSink.MessageUpdated(Message message)
        {
            string id = "";
            if (!string.IsNullOrEmpty(message.GroupId))
            {
                id = message.GroupId;
            }
            else if (!string.IsNullOrEmpty(message.ChatId))
            {
                var me = GroupMeClient.WhoAmI();

                // Chat IDs are formatted as UserID+UserID. Find the other user's ID
                var chatId = message.ChatId;
                var users = chatId.Split('+');
                var otherUser = users.First(u => u != me.Id);

                id = otherUser;
            }
            else
            {
                // must be malformed, silently ignore and continue
                return;
            }

            var groupChatVm = this.ActiveGroupsChats.FirstOrDefault(g => g.Id == id);

            groupChatVm?.UpdateMessageLikes(message);
        }

        void INotificationSink.HeartbeatReceived()
        {
        }

        void INotificationSink.RegisterPushSubscriptions(PushClient pushClient)
        {
            // Save the PushClient for Subscribing/Unsubscribing from sources later
            this.PushClient = pushClient;
        }
    }
}
