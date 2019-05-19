using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibGroupMe.Models;
using Newtonsoft.Json;
using RestSharp;

namespace LibGroupMe
{
    /// <summary>
    /// <see cref="GroupMeClient"/> allows for interaction with the GroupMe API for messaging functionality
    /// </summary>
    public class GroupMeClient
    {
        private const string GroupMeAPIUrl = "https://api.groupme.com/v3";

        private string AuthToken { get; }

        private RestClient Client { get; } = new RestClient(GroupMeAPIUrl);

        /// <summary>
        /// Creates a new client to perform GroupMe API Operations
        /// </summary>
        /// <param name="authToken">The OAuth Token used to authenticate the client</param>
        public GroupMeClient(string authToken)
        {
            this.AuthToken = authToken;
        }

        /// <summary>
        /// Returns a listing of all Group Chats a user is a member of
        /// </summary>
        /// <returns>A list of <see cref="Group"/></returns>
        public async Task<IList<Group>> GetGroupsAsync()
        {
            var request = new RestRequest($"/groups", Method.GET);
            request.AddParameter("token", AuthToken);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<GroupsList>(restResponse.Content);
                return results.Groups;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving /Groups. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Returns a set of messages from a specific Group Chat
        /// </summary>
        /// <param name="group">Group Chat the messages should be retreived from</param>
        /// <param name="limit">Number of messages that should be returned. GroupMe allows a range of 20 to 100 messages at a time.</param>
        /// <param name="mode">The method that should be used to determine the set of messages returned </param>
        /// <param name="messageId">The Message Id that will be used by the sorting mode set in <paramref name="mode"/></param>
        /// <returns>A list of <see cref="Message"/></returns>
        public async Task<IList<Message>> GetGroupMessagesAsync(Group group, int limit = 20, MessageRetreiveMode mode = MessageRetreiveMode.None, string messageId = "")
        {
            var request = new RestRequest($"/groups/{group.Id}/messages", Method.GET);
            request.AddParameter("token", AuthToken);
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
            var restResponse = await this.Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

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
        /// Returns a listing of all Direct Messages / Chats a user is a member of
        /// </summary>
        /// <returns>A list of <see cref="Chat"/></returns>
        public async Task<IList<Chat>> GetChatsAsync()
        {
            var request = new RestRequest($"/chats", Method.GET);
            request.AddParameter("token", AuthToken);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<ChatsList>(restResponse.Content);
                return results.Chats;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving /Groups. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Returns a set of messages from a specific Direct Message
        /// </summary>
        /// <param name="chat">Direct Chat the messages should be retreived from</param>
        /// <param name="mode">The method that should be used to determine the set of messages returned </param>
        /// <param name="messageId">The Message Id that will be used by the sorting mode set in <paramref name="mode"/></param>
        /// <returns>A list of <see cref="Message"/></returns>
        public async Task<IList<Message>> GetChatMessagesAsync(Chat chat, MessageRetreiveMode mode = MessageRetreiveMode.None, string messageId = "")
        {
            var request = new RestRequest($"/direct_messages", Method.GET);
            request.AddParameter("token", AuthToken);
            request.AddParameter("other_user_id", chat.OtherUser.Id);
            switch (mode)
            {
                case MessageRetreiveMode.AfterId:
                    request.AddParameter("after_id", messageId);
                    break;

                case MessageRetreiveMode.SinceId:
                    request.AddParameter("since_id", messageId);
                    break;

            }

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<ChatMessagesList>(restResponse.Content);
                return results.Response.Messages;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving Messages from Chat. Status Code {restResponse.StatusCode}");
            }
        }

        public async Task<bool> SendMessage(Group group, Message message)
        {
            var request = new RestRequest($"/groups/{group.Id}/messages", Method.POST);
            request.AddParameter("token", AuthToken);

            request.AddJsonBody(message);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return (restResponse.StatusCode == System.Net.HttpStatusCode.OK);
        }

        public async Task<bool> SendMessage(Chat chat, Message message)
        {
            var request = new RestRequest($"/direct_messages", Method.POST);
            request.AddParameter("token", AuthToken);

            request.AddJsonBody(message);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.Client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return (restResponse.StatusCode == System.Net.HttpStatusCode.OK);
        }


    }
}
