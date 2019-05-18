using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibGroupMe.Models;
using Newtonsoft.Json;
using RestSharp;

namespace LibGroupMe
{
    public class GroupMeClient
    {
        private const string GroupMeAPIUrl = "https://api.groupme.com/v3";

        private string AuthToken { get; }

        private RestClient Client { get; } = new RestClient(GroupMeAPIUrl);

        public GroupMeClient(string authToken)
        {
            this.AuthToken = authToken;
        }

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
                var results = JsonConvert.DeserializeObject<MessagesList>(restResponse.Content);
                return results.Response.Messages;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving Messages from Group. Status Code {restResponse.StatusCode}");
            }
        }

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

        public enum MessageRetreiveMode
        {
            None,
            BeforeId,
            AfterId,
            SinceId
        }
    }
}
