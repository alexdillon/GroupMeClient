using System.Collections.Generic;
using System.Linq;
using GroupMeClient.Notifications;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient
{
    public class NotificationRouter
    {
        public NotificationRouter(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
            this.Subscribers = new List<INotificationSink>();

            this.PushClient = this.GroupMeClient.EnablePushNotifications();
            this.PushClient.NotificationReceived += this.PushNotificationReceived;
        }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }
        private GroupMeClientApi.Push.PushClient PushClient { get; }
        private List<INotificationSink> Subscribers { get; }

        public void RegisterNewSubscriber(INotificationSink subscriber)
        {
            if (!this.Subscribers.Contains(subscriber))
            {
                this.Subscribers.Add(subscriber);
                subscriber.RegisterPushSubscriptions(this.PushClient, this.GroupMeClient);
            }
        }

        private void PushNotificationReceived(object sender, Notification notification)
        {
            foreach (var observer in this.Subscribers)
            {
                switch (notification)
                {
                    case LikeCreateNotification likeCreate:
                        observer.MessageUpdated(
                            likeCreate.FavoriteSubject.Message,
                            likeCreate.Alert,
                            this.FindMessageContainer(likeCreate.FavoriteSubject.Message));
                        break;

                    case FavoriteUpdate likeUpdate:
                        observer.MessageUpdated(
                            likeUpdate.FavoriteSubject.Message,
                            likeUpdate.Alert,
                            this.FindMessageContainer(likeUpdate.FavoriteSubject.Message));
                        break;

                    case LineMessageCreateNotification lineCreate:
                        observer.GroupUpdated(
                            lineCreate,
                            this.FindMessageContainer(lineCreate.Message));
                        break;

                    case DirectMessageCreateNotification directCreate:
                        observer.ChatUpdated(
                            directCreate,
                            this.FindMessageContainer(directCreate.Message));
                        break;

                    case PingNotification _:
                        observer.HeartbeatReceived();
                        break;

                    default:
                        break;
                }
            }
        }

        private IMessageContainer FindMessageContainer(Message message)
        {
            string id;
            if (!string.IsNullOrEmpty(message.GroupId))
            {
                id = message.GroupId;
            }
            else if (!string.IsNullOrEmpty(message.ChatId))
            {
                var me = this.GroupMeClient.WhoAmI();

                // Chat IDs are formatted as UserID+UserID. Find the other user's ID
                var chatId = message.ChatId;
                var users = chatId.Split('+');
                var otherUser = users.First(u => u != me.Id);

                id = otherUser;
            }
            else
            {
                return null;
            }

            foreach (var group in this.GroupMeClient.Groups())
            {
                if (group.Id == id)
                {
                    return group;
                }
            }

            foreach (var chat in this.GroupMeClient.Chats())
            {
                if (chat.Id == id)
                {
                    return chat;
                }
            }

            return null;
        }
    }
}
