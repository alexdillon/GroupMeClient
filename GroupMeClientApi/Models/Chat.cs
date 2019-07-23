using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace GroupMeClientApi.Models
{
    /// <summary>
    /// <see cref="Chat"/> represents a GroupMe Direct Message (or Chat) with another user.
    /// </summary>
    public class Chat : IMessageContainer, IAvatarSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chat"/> class.
        /// </summary>
        public Chat()
        {
            this.Messages = new List<Message>();
        }

        /// <summary>
        /// Gets the <see cref="Member"/> that this chat is being held with.
        /// </summary>
        [JsonProperty("other_user")]
        public virtual Member OtherUser { get; internal set; }

        /// <summary>
        /// Gets the name of the <see cref="Member"/> that this <see cref="Chat"/> is with.
        /// <seealso cref="OtherUser"/>
        /// </summary>
        public string Name => this.OtherUser?.Name ?? string.Empty;

        /// <summary>
        /// Gets the Unix Timestamp for when this chat was created.
        /// </summary>
        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the Date and Time when this chat was created.
        /// </summary>
        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets the Unix Timestamp for when this chat was last updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the Date and Time when this chat was last updated.
        /// </summary>
        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets a copy of the latest message for preview purposes.
        /// Note that API Operations, like <see cref="Message.LikeMessage"/> cannot be performed.
        /// See <see cref="Messages"/> list instead for full message objects.
        /// </summary>
        [JsonProperty("last_message")]
        public Message LatestMessage { get; internal set; }

        /// <summary>
        /// Gets a copy of the latest message for preview purposes.
        /// Note that API Operations, like <see cref="Message.LikeMessage"/> cannot be performed.
        /// See <see cref="Messages"/> list instead for full message objects.
        /// </summary>
        [JsonProperty("messages_count")]
        public int TotalMessageCount { get; internal set; }

        /// <summary>
        /// Gets the Identifier of this Chat. See <seealso cref="OtherUser"/> for more information.
        /// </summary>
        /// <remarks>
        /// This key is required for EF. It must always match OtherUser.Id.
        /// </remarks>
        [Key]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets a list of <see cref="Message"/>s in this <see cref="Chat"/>.
        /// </summary>
        [InverseProperty("Chat")]
        public virtual List<Message> Messages { get; internal set; }

        /// <summary>
        /// Gets a unique value to determine if the internal state of the Group has changed.
        /// If two accesses to this property return a different string, a state change has occured.
        /// </summary>
        public string InternalStateChanged { get; internal set; }

        /// <summary>
        /// Gets the <see cref="GroupMeClient"/> that manages this <see cref="Chat"/>.
        /// </summary>
        [NotMapped]
        public GroupMeClient Client { get; internal set; }

        /// <inheritdoc />
        public string ImageOrAvatarUrl => ((IAvatarSource)this.OtherUser).ImageOrAvatarUrl;

        /// <inheritdoc />
        public bool IsRoundedAvatar => ((IAvatarSource)this.OtherUser).IsRoundedAvatar;

        /// <summary>
        /// Returns a set of messages from a this Direct Message / Chat.
        /// </summary>
        /// <param name="mode">The method that should be used to determine the set of messages returned.</param>
        /// <param name="messageId">The Message Id that will be used by the sorting mode set in <paramref name="mode"/>.</param>
        /// <returns>A list of <see cref="Message"/>.</returns>
        public async Task<ICollection<Message>> GetMessagesAsync(MessageRetreiveMode mode = MessageRetreiveMode.None, string messageId = "")
        {
            var request = this.Client.CreateRestRequest($"/direct_messages", Method.GET);
            request.AddParameter("other_user_id", this.OtherUser.Id);
            switch (mode)
            {
                case MessageRetreiveMode.BeforeId:
                    request.AddParameter("before_id", messageId);
                    break;

                case MessageRetreiveMode.SinceId:
                    request.AddParameter("since_id", messageId);
                    break;

                case MessageRetreiveMode.AfterId:
                    throw new NotSupportedException("GroupMe doesn't support AfterId for Direct Messages");
            }

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<ChatMessagesList>(restResponse.Content);

                foreach (var message in results.Response.Messages)
                {
                    // ensure every Message has a reference to the parent Chat (this)
                    message.Chat = this;

                    var oldMessage = this.Messages.Find(m => m.Id == message.Id);
                    if (oldMessage == null)
                    {
                        this.Messages.Add(message);
                    }
                    else
                    {
                        DataMerger.MergeMessage(oldMessage, message);
                    }
                }

                this.InternalStateChanged = Guid.NewGuid().ToString();
                await this.Client.Update();

                return results.Response.Messages;
            }
            else if (restResponse.StatusCode == System.Net.HttpStatusCode.NotModified)
            {
                return new List<Message>();
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving Messages from Chat. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Sends a message to this <see cref="Chat"/>.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="bool"/> indicating the success of the send operation.</returns>
        public async Task<bool> SendMessage(Message message)
        {
            var request = this.Client.CreateRestRequest($"/direct_messages", Method.POST);

            // Add the Recipient ID into the message, as GroupMe's API requires for DM's
            message.RecipientId = this.OtherUser.Id;

            var payload = new
            {
                direct_message = message,
            };

            request.AddJsonBody(payload);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return restResponse.StatusCode == System.Net.HttpStatusCode.Created;
        }

        /// <summary>
        /// Returns the authenticated user.
        /// </summary>
        /// <returns>A <see cref="Member"/>.</returns>
        public Member WhoAmI()
        {
            return this.Client.WhoAmI();
        }
    }
}
