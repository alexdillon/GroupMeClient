using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GroupMeClientApi.Models;
using Newtonsoft.Json;
using RestSharp;

namespace GroupMeClientApi
{
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

            this.GroupsList = new List<Group>();
            this.ChatsList = new List<Chat>();
        }

        /// <summary>
        /// Gets or sets the <see cref="ImageDownloader"/> that is used for image downloads.
        /// </summary>
        public virtual ImageDownloader ImageDownloader { get; set; } = new ImageDownloader();

        /// <summary>
        /// Gets the <see cref="RestClient"/> that is used to perform GroupMe API calls.
        /// </summary>
        internal RestClient ApiClient { get; } = new RestClient(GroupMeAPIUrl);

        /// <summary>
        /// Gets or sets the authenticated user.
        /// </summary>
        internal Member Me { get; set; }

        /// <summary>
        /// Gets the Auth Token used to authenticate a GroupMe API Call.
        /// </summary>
        internal string AuthToken { get; }

        /// <summary>
        /// Gets or sets the client used to subscribe to push notifications, if enabled.
        /// </summary>
        private Push.PushClient PushClient { get; set; }

        private List<Group> GroupsList { get; set; }

        private List<Chat> ChatsList { get; set; }

        /// <summary>
        /// Gets a enumeration of <see cref="Group"/>s controlled by the API Client.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> for the <see cref="Group"/>.
        /// </returns>
        public virtual IEnumerable<Group> Groups()
        {
            foreach (var group in this.GroupsList)
            {
                yield return group;
            }
        }

        /// <summary>
        /// Gets a enumeration of <see cref="Chat"/>s controlled by the API Client.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> for the <see cref="Chat"/>.
        /// </returns>
        public virtual IEnumerable<Chat> Chats()
        {
            foreach (var chat in this.ChatsList)
            {
                yield return chat;
            }
        }

        /// <summary>
        /// Returns a listing of all Group Chats a user is a member of.
        /// </summary>
        /// <returns>A list of <see cref="Group"/>.</returns>
        public virtual async Task<ICollection<Group>> GetGroupsAsync()
        {
            var request = this.CreateRestRequest($"/groups", Method.GET);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);
            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<GroupsList>(restResponse.Content);

                foreach (var group in results.Groups)
                {
                    // ensure every Group has a reference to the parent client (this)
                    group.Client = this;

                    var oldGroup = this.GroupsList.Find(g => g.Id == group.Id);

                    if (oldGroup == null)
                    {
                        this.GroupsList.Add(group);
                    }
                    else
                    {
                        DataMerger.MergeGroup(oldGroup, group);
                    }
                }

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
        public virtual async Task<ICollection<Chat>> GetChatsAsync()
        {
            var request = this.CreateRestRequest($"/chats", Method.GET);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await this.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<ChatsList>(restResponse.Content);

                foreach (var chat in results.Chats)
                {
                    // ensure every Chat has a reference to the parent client (this)
                    chat.Client = this;

                    // required to establish a constant, non-foreign-key Primary Key for Chat
                    chat.Id = chat.OtherUser.Id;

                    var oldChat = this.ChatsList.Find(g => g.Id == chat.Id);

                    if (oldChat == null)
                    {
                        this.ChatsList.Add(chat);
                    }
                    else
                    {
                        DataMerger.MergeChat(oldChat, chat);
                    }
                }

                return results.Chats;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving /Groups. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Returns the authenticated user. A cached copy
        /// will be returned unless an update is forced.
        /// </summary>
        /// <param name="forceUpdate">Force an API refresh.</param>
        /// <returns>A <see cref="Member"/>.</returns>
        public virtual Member WhoAmI(bool forceUpdate = false)
        {
            if (this.Me != null && !forceUpdate)
            {
                return this.Me;
            }

            var request = this.CreateRestRequest($"/users/me", Method.GET);

            var restResponse = this.ApiClient.Execute(request);

            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var results = JsonConvert.DeserializeObject<MemberResponse>(restResponse.Content);
                this.Me = results.Member;

                return this.Me;
            }
            else
            {
                throw new System.Net.WebException($"Failure retreving /Users/Me. Status Code {restResponse.StatusCode}");
            }
        }

        /// <summary>
        /// Updates the internal state of the GroupMe Client.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual Task Update()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enables subscribing to push notifications for all <see cref="Groups"/>
        /// and <see cref="Chat"/> controlled by this client.
        /// This must be enabled before calling <see cref="GetChatsAsync"/> or <see cref="GetGroupsAsync"/>.
        /// </summary>
        /// <returns>The <see cref="PushClient"/> that used for push operations.</returns>
        public virtual Push.PushClient EnablePushNotifications()
        {
            this.PushClient = new Push.PushClient(this);

            this.PushClient.Connect();

            return this.PushClient;
        }

        /// <summary>
        /// Creates a new <see cref="RestRequest"/> object to perform a GroupMe API Call including the Authorization Token.
        /// </summary>
        /// <param name="resource">The GroupMe API resource to call.</param>
        /// <param name="method">The method used for the API Call.</param>
        /// <returns>A <see cref="RestRequest"/> with the user's access token.</returns>
        internal RestRequest CreateRestRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method)
            {
                JsonSerializer = JsonAdapter.Default,
            };

            request.AddHeader("X-Access-Token", this.AuthToken);

            return request;
        }
    }
}
