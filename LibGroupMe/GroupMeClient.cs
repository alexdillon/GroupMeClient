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
    }
}
