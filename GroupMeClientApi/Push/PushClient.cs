using System;
using System.Collections.Generic;
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

        private GroupMeClient Client { get; }

        private BayeuxClient BayeuxClient { get; }

        private string GroupMePushServerUrl => "https://push.groupme.com/faye";

        /// <summary>
        /// Connects to the GroupMe Server to prepare for receiving notifications.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task Connect()
        {
            await this.BayeuxClient.Connect();
        }

        /// <summary>
        /// Subscribes to push notifications for a specific <see cref="Group"/>.
        /// </summary>
        /// <param name="group">The Group to receive notifications for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SubscribeGroup(Group group)
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

        /// <summary>
        /// Subscribes to push notifications for a specific <see cref="Chat"/>.
        /// </summary>
        /// <param name="chat">The Chat to receive notifications for.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SubscribeChat(Chat chat)
        {
            var extensions = new Dictionary<string, object>
            {
                { "access_token", this.Client.AuthToken },
                { "timestamp", DateTime.Now.ToUniversalTime().ToBinary().ToString() },
            };

            // Subscribe to channel.
            await this.BayeuxClient.Subscribe(
                $"/user/{chat.Id}",
                (m) => this.ChatCallback(chat, m),
                extensions);
        }

        /// <summary>
        /// Subscribes to all push notifications for the current user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task SubscribeMe()
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

        private void GroupCallback(Group group, IBayeuxMessage message)
        {
            Console.WriteLine($"Received {message.Data.ToString()} for {group.Name}");
        }

        private void ChatCallback(Chat chat, IBayeuxMessage message)
        {
            Console.WriteLine($"Received {message.Data.ToString()} for {chat.OtherUser.Name}");
        }

        private void MeCallback(IBayeuxMessage message)
        {
            var jsonString = message.Data.ToString();
            var notification = JsonConvert.DeserializeObject<Notifications.Notification>(jsonString);

            Console.WriteLine($"Received {message.Data.ToString()} for ME!");
            Console.WriteLine(notification.GetType());
        }
    }
}
