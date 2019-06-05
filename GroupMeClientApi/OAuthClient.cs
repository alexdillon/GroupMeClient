namespace GroupMeClientApi
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// <see cref="OAuthClient"/> provides methods to authenticate a user and retreive an auth token.
    /// </summary>
    public class OAuthClient
    {
        private const int CallbackPort = 16924;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        /// <param name="clientName">The application name to display on the success webpage.</param>
        /// <param name="clientApiId">The application identifer assigned by GroupMe.</param>
        public OAuthClient(string clientName, string clientApiId)
        {
            this.OAuthServer.Prefixes.Add($"http://localhost:{CallbackPort}/");
            this.OAuthServer.Start();

            Task.Run(this.ConnectionLoop, this.CancellationTokenSource.Token);
            this.ClientName = clientName;
            this.ClientApiId = clientApiId ?? throw new ArgumentNullException(nameof(clientApiId));
        }

        /// <summary>
        /// Gets the URL where clients should navigate to begin obtaining an OAuth Token.
        /// </summary>
        public string GroupMeOAuthUrl => "https://oauth.groupme.com/oauth/authorize?client_id=" + this.ClientApiId;

        private HttpListener OAuthServer { get; } = new HttpListener();

        private TaskCompletionSource<string> TokenReady { get; } = new TaskCompletionSource<string>();

        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        private string ClientName { get; set; }

        private string ClientApiId { get; set; }

        /// <summary>
        /// Waits for the user to complete the OAuth login online and returns an access token if successful.
        /// </summary>
        /// <returns>A GroupMe API access token.</returns>
        public Task<string> GetAuthToken()
        {
            return this.TokenReady.Task;
        }

        /// <summary>
        /// Shuts down the OAuth server.
        /// </summary>
        public void Stop()
        {
            this.CancellationTokenSource.Cancel();
            this.OAuthServer.Stop();
        }

        private async Task ConnectionLoop()
        {
            this.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            while (!this.CancellationTokenSource.IsCancellationRequested)
            {
                var context = await this.OAuthServer.GetContextAsync();

                if (!string.IsNullOrEmpty(context.Request.QueryString["access_token"]))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(context.Response.OutputStream))
                    {
                        string successPage = GroupMeClientApi.Properties.Resources.SuccessPage;
                        successPage = successPage.Replace("{CLIENTNAME}", this.ClientName);

                        sw.WriteLine(successPage);
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.Close();

                    this.TokenReady.SetResult(context.Request.QueryString["access_token"]);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                }

                this.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }
    }
}
