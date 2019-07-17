using System.Collections.Generic;
using GroupMeClient.Notifications;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient
{
    class NotificationRouter
    {
        public NotificationRouter(GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
            Subscribers = new List<INotificationSink>();

            PushClient = this.GroupMeClient.EnablePushNotifications();
            PushClient.NotificationReceived += PushNotificationReceived;
        }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; }
        private GroupMeClientApi.Push.PushClient PushClient { get; }
        private List<INotificationSink> Subscribers { get; }

        public void RegisterNewSubscriber(INotificationSink subscriber)
        {
            if (!this.Subscribers.Contains(subscriber))
            {
                this.Subscribers.Add(subscriber);
                subscriber.RegisterPushSubscriptions(this.PushClient);
            }
        }

        private void PushNotificationReceived(object sender, Notification notification)
        {
            foreach (var observer in Subscribers)
            {
                switch (notification)
                {
                    case LikeCreateNotification likeCreate:
                        observer.MessageUpdated(likeCreate.FavoriteSubject.Message);
                        break;

                    case FavoriteUpdate likeUpdate:
                        observer.MessageUpdated(likeUpdate.FavoriteSubject.Message);
                        break;

                    case LineMessageCreateNotification lineCreate:
                        observer.GroupUpdated(lineCreate);
                        break;

                    case DirectMessageCreateNotification directCreate:
                        observer.ChatUpdated(directCreate);
                        break;

                    case PingNotification _:
                        observer.HeartbeatReceived();
                        break;

                    default:
                        break;

                }
            }
        }
    }
}
