using System;
using System.Threading.Tasks;
using System.Linq;
using GalaSoft.MvvmLight;
using LinqToTwitter;
using System.IO;
using System.Windows.Input;

namespace GroupMeClient.ViewModels.Controls
{
    public class TwitterAttachmentControlViewModel : ViewModelBase
    {
        public TwitterAttachmentControlViewModel(string tweetUrl)
        {
            this.TweetUrl = tweetUrl;

            _ = LoadTweet();
        }

        private string TweetUrl { get; set; }

        public ICommand ClickAction { get; }


        private Stream renderedImage;

        /// <summary>
        /// Gets the rendered image.
        /// </summary>
        public Stream RenderedImage
        {
            get { return renderedImage; }
            set { Set(() => this.RenderedImage, ref renderedImage, value); }
        }

        public async Task LoadTweet()
        {
            if (Uri.TryCreate(this.TweetUrl, UriKind.Absolute, out var uri))
            {
                var tweetId = uri.Segments.Last();
                var tweetIdLong = ulong.Parse(tweetId);

                var auth = new SingleUserAuthorizer
                {
                    CredentialStore = new SingleUserInMemoryCredentialStore
                    {
                        // the OEmbedded API is open, so no auth needed for now
                        ConsumerKey = "consumerKey",
                        ConsumerSecret = "consumerSecret",
                        AccessToken = "accessToken",
                        AccessTokenSecret = "accessTokenSecret"
                    }
                };

                var twitterContext = new TwitterContext(auth);

                try
                {
                    var embeddedStatus =
                        await
                        (from tweet in twitterContext.Status
                         where tweet.Type == StatusType.Oembed &&
                                 tweet.ID == tweetIdLong
                         select tweet.EmbeddedStatus)
                        .SingleOrDefaultAsync();


                    if (embeddedStatus != null)
                    {
                        //this.RenderedImage = await htmlRenderer.RenderHtmlAsync(embeddedStatus.Html);
                    }
                }
                catch (Exception ex)
                {
                    //this.RenderedImage = await htmlRenderer.RenderHtmlAsync(ex.Message);
                }
            }
        }
    }
}
