namespace LibGroupMe
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

        private const string SuccessWebResponse = " <!DOCTYPE html><html><head><meta charset=\"UTF-8\"><title>Success</title><style> body { padding-top: 80px; text-align: center; font-family: Arial,Helvetica Neue,Helvetica,sans-serif; color: #555; }h1, h2 { display: inline-block; background: #fff;}h1 { font-size: 30px}h2 { font-size: 20px;}span { background: #fd0;}</style></head><body><img src=\"data:image/png;base64,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\"><br>Success! <br><br><strong>The GroupMe Desktop Client has successfully linked to your GroupMe Account</strong><br><br>You can close this window now</body></html>";

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient"/> class.
        /// </summary>
        public OAuthClient()
        {
            this.OAuthServer.Prefixes.Add($"http://localhost:{CallbackPort}/");
            this.OAuthServer.Start();

            Task.Run(this.ConnectionLoop, this.CancellationTokenSource.Token);
        }

        /// <summary>
        /// Gets the URL where clients should navigate to begin obtaining an OAuth Token
        /// </summary>
        public string GroupMeOAuthUrl => "https://oauth.groupme.com/oauth/authorize?client_id=e6zucCgRruqYZmEUMFF9uC4OoaCxmb54MuSOt3yFGtVloQcM";

        private HttpListener OAuthServer { get; } = new HttpListener();

        private TaskCompletionSource<string> TokenReady { get; } = new TaskCompletionSource<string>();

        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

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
                    this.TokenReady.SetResult(context.Request.QueryString["access_token"]);

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(context.Response.OutputStream))
                    {
                        sw.WriteLine(SuccessWebResponse);
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.Close();
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
