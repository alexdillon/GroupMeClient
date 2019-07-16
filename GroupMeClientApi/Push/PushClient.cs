using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bayeux;
using GroupMeClientApi.Models;
using Newtonsoft.Json;

namespace GroupMeClientApi.Push
{
    /// <summary>
    /// PushClient allows for subscribing to push notification and updates
    /// from GroupMe for both Direct Messages/Chats and Groups.
    /// </summary>
    public class PushClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PushClient"/> class.
        /// </summary>
        /// <param name="client">
        /// The GroupMe Client that manages the Groups and Chats
        /// being monitored.
        /// </param>
        public PushClient(GroupMeClient client)
        {
            this.Client = client;

            var endpoint = new Uri(this.GroupMePushServerUrl);
            var settings = new BayeuxClientSettings(endpoint)
            {
                Logger = new Bayeux.Diagnostics.ConsoleLogger(),
            };

            // Create the client.
            this.BayeuxClient = new BayeuxClient(settings);
        }

        /// <summary>
        /// The event that is raised when a new push notification is received.
        /// </summary>
        public event EventHandler<Notifications.Notification> NotificationReceived;

        private GroupMeClient Client { get; }

        private BayeuxClient BayeuxClient { get; set; }

        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        private Task MonitorConnectionTask { get; set; }

        private Timer RenewalTimer { get; set; }

        private string GroupMePushServerUrl => "https://push.groupme.com/faye";

        private TimeSpan MaxConnectionInterval => TimeSpan.FromMinutes(50);

        private List<Group> SubscribedGroups { get; } = new List<Group>();

        /// <summary>
        /// Connects to the GroupMe Server to prepare for receiving notifications.
        /// </summary>
        public void Connect()
        {
            if (this.MonitorConnectionTask == null || this.MonitorConnectionTask.IsCanceled || this.MonitorConnectionTask.IsCompleted)
            {
                this.MonitorConnectionTask = Task.Run(this.MonitorConnection, this.CancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Subscribes to push notifications for a specific <see cref="IMessageContainer"/>.
        /// </summary>
        /// <param name="container">The container to receive notifications for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SubscribeAsync(IMessageContainer container)
        {
            if (container is Group g)
            {
                await this.SubscribeGroupAsync(g);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Subscribe not supported for name={container.Name}");
            }
        }

        /// <summary>
        /// Subscribes to push notifications for a specific <see cref="Group"/>.
        /// </summary>
        /// <param name="group">The Group to receive notifications for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SubscribeGroupAsync(Group group)
        {
            if (!this.SubscribedGroups.Contains(group))
            {
                this.SubscribedGroups.Add(group);
            }

            await this.SendGroupSubscriptionRequestAsync(group);
        }

        /// <summary>
        /// Unsubscribes from push notifications for a specific <see cref="IMessageContainer"/>.
        /// </summary>
        /// <param name="container">The container to unsubscribe from receive notifications for.</param>
        public void Unsubscribe(IMessageContainer container)
        {
            if (container is Group g)
            {
                this.UnsubscribeGroup(g);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unsubscribe not supported for name={container.Name}");
            }
        }

        /// <summary>
        /// Unsubscribes from push notifications for a specific <see cref="Group"/>.
        /// </summary>
        /// <param name="group">The Group to unsubscribe from receiving notifications for.</param>
        public void UnsubscribeGroup(Group group)
        {
            if (this.SubscribedGroups.Contains(group))
            {
                this.SubscribedGroups.Remove(group);
            }
        }

        /// <summary>
        /// Subscribes to all push notifications for the current user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task SubscribeMeAsync()
        {
            var extensions = new Dictionary<string, object>
            {
                { "access_token", this.Client.AuthToken },
                { "timestamp", DateTime.Now.ToUniversalTime().ToBinary().ToString() },
            };

            var me = this.Client.WhoAmI();

            // Subscribe to channel.
            await this.BayeuxClient.Subscribe(
                $"/user/{me.Id}",
                this.MeCallback,
                extensions);
        }

        /// <summary>
        /// Sends a Bayeux command requesting push notifications for a specific <see cref="Group"/>.
        /// </summary>
        /// <param name="group">The Group to receive notifications for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task SendGroupSubscriptionRequestAsync(Group group)
        {
            var extensions = new Dictionary<string, object>
            {
                { "access_token", this.Client.AuthToken },
                { "timestamp", DateTime.Now.ToUniversalTime().ToBinary().ToString() },
            };

            // Subscribe to channel.
            await this.BayeuxClient.Subscribe(
                $"/group/{group.Id}",
                (m) => this.GroupCallback(group, m),
                extensions);
        }

        private async Task MonitorConnection()
        {
            while (!this.CancellationTokenSource.IsCancellationRequested)
            {
                if (!this.BayeuxClient.IsHeartbeatConnected)
                {
                    try
                    {
                        this.RenewalTimer?.Dispose();

                        this.BayeuxClient?.Disconnect();
                        await this.BayeuxClient.Connect();
                        await this.SubscribeMeAsync();

                        foreach (var group in this.SubscribedGroups)
                        {
                            await this.SendGroupSubscriptionRequestAsync(group);
                        }

                        this.RenewalTimer = new Timer(
                            new TimerCallback((a) => this.BayeuxClient.Disconnect()),
                            null,
                            this.MaxConnectionInterval,
                            this.MaxConnectionInterval);
                    }
                    catch (Exception)
                    {
                        // wait a while before trying to reconnect again
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                    }
                }
                else
                {
                    // connected fine, wait a while before checking again
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }

                this.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        private void GroupCallback(Group group, IBayeuxMessage message)
        {
            var jsonString = message.Data.ToString();
            var notification = JsonConvert.DeserializeObject<Notifications.Notification>(jsonString);

            Console.WriteLine($"Received {message.Data.ToString()} for {group.Name}");

            try
            {
                var handler = this.NotificationReceived;
                handler?.Invoke(this, notification);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Error handling callback for 'Group' notification");
            }
        }

        private void MeCallback(IBayeuxMessage message)
        {
            var jsonString = message.Data.ToString();
            var notification = JsonConvert.DeserializeObject<Notifications.Notification>(jsonString);

            Console.WriteLine($"Received {message.Data.ToString()} for ME! at {System.DateTime.Now.ToShortTimeString()}");

            try
            {
                var handler = this.NotificationReceived;
                handler?.Invoke(this, notification);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("Error handling callback for 'Me' notification");
            }
        }
    }
}
