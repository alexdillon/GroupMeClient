namespace LibGroupMe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using LibGroupMe.Models;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// <see cref="GroupMeClient"/> allows for interaction with the GroupMe API for messaging functionality.
    /// </summary>
    public class GroupMeClient
    {
        private const string GroupMeAPIUrl = "https://api.groupme.com/v3";

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeClient"/> class to perform GroupMe API Operations.
        /// </summary>
        /// <param name="authToken">The OAuth Token used to authenticate the client.</param>
        public GroupMeClient(string authToken)
        {
            this.AuthToken = authToken;
        }

        /// <summary>
        /// Gets the Auth Token used to authenticate a GroupMe API Call.
        /// </summary>
        internal string AuthToken { get; }

        /// <summary>
        /// Gets the <see cref="RestClient"/> that is used to perform GroupMe API calls.
        /// </summary>
        internal RestClient ApiClient { get; } = new RestClient(GroupMeAPIUrl);

        /// <summary>
        /// Returns a listing of all Group Chats a user is a member of.
        /// </summary>
        /// <returns>A list of <see cref="Group"/>.</returns>
        public async Task<IList<Group>> GetGroupsAsync()
        {
            var request = new RestRequest($"/groups", Method.GET);
            request.AddParameter("token", this.AuthToken);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<GroupsList>(restResponse.Content);
                results.Groups.All(g =>
                {
                    // ensure every Group has a reference to the parent client (this)
                    g.Client = this;
                    return true;
                });

                return results.Groups;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving /Groups. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Returns a listing of all Direct Messages / Chats a user is a member of.
        /// </summary>
        /// <returns>A list of <see cref="Chat"/>.</returns>
        public async Task<IList<Chat>> GetChatsAsync()
        {
            var request = new RestRequest($"/chats", Method.GET);
            request.AddParameter("token", this.AuthToken);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<ChatsList>(restResponse.Content);
                results.Chats.All(c =>
                {
                    // ensure every Chat has a reference to the parent client (this)
                    c.Client = this;
                    return true;
                });
                return results.Chats;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving /Groups. Status Code {restResponse.StatusCode}");
            }
        }
    }
}
