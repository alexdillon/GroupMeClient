namespace LibGroupMe.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Represents a GroupMe Group Chat.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Gets the group identifier.
        /// </summary>
        [JsonProperty("group_id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the group name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the phone number that can be used to interact with this group over SMS.
        /// </summary>
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; internal set; }

        /// <summary>
        /// Gets the group type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; internal set; }

        /// <summary>
        /// Gets the group description text.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the Url for the Group avatar or image.
        /// </summary>
        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        /// <summary>
        /// Gets the Unix Timestamp when the group was created.
        /// </summary>
        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the time when the group was created.
        /// </summary>
        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets the Unix Timestamp when the group was last updated.
        /// </summary>
        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the time when the group was last updated.
        /// </summary>
        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets a value indicating whether the group is in office mode.
        /// </summary>
        [JsonProperty("office_mode")]
        public bool OfficeMode { get; internal set; }

        /// <summary>
        /// Gets a Url to share the group.
        /// </summary>
        [JsonProperty("share_url")]
        public string ShareUrl { get; internal set; }

        /// <summary>
        /// Gets a list of <see cref="Members"/> in the group.
        /// </summary>
        [JsonProperty("members")]
        public IList<Member> Members { get; internal set; }

        /// <summary>
        /// Gets the maximum number of members who can be in this group.
        /// </summary>
        [JsonProperty("max_members")]
        public int MaxMembers { get; internal set; }

        /// <summary>
        /// Gets a preview of the messages in this group.
        /// </summary>
        [JsonProperty("messages")]
        public MessagesPreview MsgPreview { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="GroupMeClient"/> that manages this <see cref="Group"/>.
        /// </summary>
        internal GroupMeClient Client { get; set; }

        /// <summary>
        /// Returns a set of messages from a this Group Chat.
        /// </summary>
        /// <param name="limit">Number of messages that should be returned. GroupMe allows a range of 20 to 100 messages at a time.</param>
        /// <param name="mode">The method that should be used to determine the set of messages returned. </param>
        /// <param name="messageId">The Message Id that will be used by the sorting mode set in <paramref name="mode"/>.</param>
        /// <returns>A list of <see cref="Message"/>.</returns>
        public async Task<IList<Message>> GetMessagesAsync(int limit = 20, MessageRetreiveMode mode = MessageRetreiveMode.None, string messageId = "")
        {
            var request = this.Client.CreateRestRequest($"/groups/{this.Id}/messages", Method.GET);
            request.AddParameter("limit", limit);
            switch (mode)
            {
                case MessageRetreiveMode.AfterId:
                    request.AddParameter("after_id", messageId);
                    break;

                case MessageRetreiveMode.BeforeId:
                    request.AddParameter("before_id", messageId);
                    break;

                case MessageRetreiveMode.SinceId:
                    request.AddParameter("since_id", messageId);
                    break;

            }

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<GroupMessagesList>(restResponse.Content);
                return results.Response.Messages;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving Messages from Group. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Sends a new message to this <see cref="Group"/>.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="bool"/> indicating the success of the send operation.</returns>
        public async Task<bool> SendMessage(Message message)
        {
            var request = this.Client.CreateRestRequest($"/groups/{this.Id}/messages", Method.POST);
            var payload = new
            {
                message,
            };

            request.AddJsonBody(payload);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return restResponse.StatusCode == System.Net.HttpStatusCode.Created;
        }

        /// <summary>
        /// Preview of the most recent message in a <see cref="Group"/> and information about when it was last updated.
        /// </summary>
        public class MessagesPreview
        {
            /// <summary>
            /// Gets the total number of messages in a <see cref="Group"/>.
            /// </summary>
            [JsonProperty("count")]
            public int Count { get; internal set; }

            /// <summary>
            /// Gets the identifier for the most recent message in a <see cref="Group"/>.
            /// </summary>
            [JsonProperty("last_message_id")]
            public string LastMessageId { get; internal set; }

            /// <summary>
            /// Gets the Unix Timestamp when the last message was sent.
            /// </summary>
            [JsonProperty("last_message_created_at")]
            public int LastMessageCreatedAtUnixTime { get; internal set; }

            /// <summary>
            /// Gets the time when the last message was sent.
            /// </summary>
            public DateTime LastMessageCreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.LastMessageCreatedAtUnixTime).ToLocalTime().DateTime;

            /// <summary>
            /// Gets a preview of the most recent message's content.
            /// </summary>
            [JsonProperty("preview")]
            public PreviewContents Preview { get; internal set; }

            /// <summary>
            /// Brief preview of a message and its sender.
            /// </summary>
            public class PreviewContents
            {
                /// <summary>
                /// Gets the sender's nickname.
                /// </summary>
                [JsonProperty("nickname")]
                public string Nickname { get; internal set; }

                /// <summary>
                /// Gets the message text.
                /// </summary>
                [JsonProperty("text")]
                public string Text { get; internal set; }

                /// <summary>
                /// Gets the sender's avatar Url.
                /// </summary>
                [JsonProperty("image_url")]
                public string ImageUrl { get; internal set; }

                /// <summary>
                /// Gets a list of <see cref="Attachments.Attachment"/> contained with the message.
                /// </summary>
                [JsonProperty("attachments")]
                public IList<Attachments.Attachment> Attachments { get; internal set; }
            }
        }
    }
}
