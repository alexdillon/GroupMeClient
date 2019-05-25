using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibGroupMe.Models
{
    public class Group
    {
        [JsonProperty("group_id")]
        public string Id { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; internal set; }

        [JsonProperty("type")]
        public string Type { get; internal set; }

        [JsonProperty("description")]
        public string Description { get; internal set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; internal set; }

        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; internal set; }

        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("office_mode")]
        public bool OfficeMode { get; internal set; }

        [JsonProperty("share_url")]
        public string ShareUrl { get; internal set; }

        [JsonProperty("members")]
        public IList<Member> Members { get; internal set; }

        [JsonProperty("max_members")]
        public int MaxMembers { get; internal set; }
        
        [JsonProperty("messages")]
        public MessagesPreview MsgPreview { get; internal set; }

        public class MessagesPreview
        {
            [JsonProperty("count")]
            public int Count { get; internal set; }

            [JsonProperty("last_message_id")]
            public string LastMessageId { get; internal set; }

            [JsonProperty("last_message_created_at")]
            public int LastMessageCreatedAtUnixTime { get; internal set; }

            public DateTime LastMessageCreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.LastMessageCreatedAtUnixTime).ToLocalTime().DateTime;

            public class PreviewContents
            {
                [JsonProperty("nickname")]
                public string Nickname { get; internal set; }

                [JsonProperty("text")]
                public string Text { get; internal set; }

                [JsonProperty("image_url")]
                public string ImageUrl { get; internal set; }

                [JsonProperty("attachments")]
                public IList<Attachments.Attachment> Attachments { get; internal set; }
            }

            [JsonProperty("preview")]
            public PreviewContents Preview { get; internal set; }
        }

        /// <summary>
        /// The <see cref="GroupMeClient"/> that manages this <see cref="Group"/>
        /// </summary>
        internal GroupMeClient Client { get; set; }

        /// <summary>
        /// Returns a set of messages from a this Group Chat
        /// </summary>
        /// <param name="limit">Number of messages that should be returned. GroupMe allows a range of 20 to 100 messages at a time.</param>
        /// <param name="mode">The method that should be used to determine the set of messages returned </param>
        /// <param name="messageId">The Message Id that will be used by the sorting mode set in <paramref name="mode"/></param>
        /// <returns>A list of <see cref="Message"/></returns>
        public async Task<IList<Message>> GetMessagesAsync(int limit = 20, MessageRetreiveMode mode = MessageRetreiveMode.None, string messageId = "")
        {
            var request = new RestRequest($"/groups/{this.Id}/messages", Method.GET);
            request.AddParameter("token", this.Client.AuthToken);
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
        /// Sends a new message to this <see cref="Group"/>
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns></returns>
        public async Task<bool> SendMessage(Message message)
        {
            var request = new RestRequest($"/groups/{this.Id}/messages", Method.POST);
            request.AddParameter("token", this.Client.AuthToken);

            request.AddJsonBody(message);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return (restResponse.StatusCode == System.Net.HttpStatusCode.OK);
        }

    }
}
