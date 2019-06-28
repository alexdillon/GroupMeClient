namespace GroupMeClientApi.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// <see cref="Message"/> represents a message in a GroupMe <see cref="Group"/> or <see cref="Chat"/>.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// Gets the message identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets the GUID assigned by the sender.
        /// </summary>
        [JsonProperty("source_guid")]
        public string SourceGuid { get; internal set; }

        /// <summary>
        /// Gets the identifier for the <see cref="Member"/> a Direct Message was sent to.
        /// </summary>
        [JsonProperty("recipient_id")]
        public string RecipientId { get; internal set; }

        /// <summary>
        /// Gets the Unix Timestamp when the message was created.
        /// </summary>
        [JsonProperty("created_at", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int CreatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the identifier for a <see cref="Member"/> who sent a Group Message.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; internal set; }

        /// <summary>
        /// Gets the identifier for a <see cref="Group"/> where this message was sent.
        /// If this <see cref="Message"/> represents a Direct Message, this field will be null.
        /// </summary>
        [JsonProperty("group_id")]
        public string GroupId { get; internal set; }

        /// <summary>
        /// Gets the conversation identifier for a <see cref="Chat"/> where this message was sent.
        /// If this <see cref="Message"/> represents a Group Message, this field will be null.
        /// </summary>
        [JsonProperty("conversation_id")]
        public string ConversationId { get; internal set; }

        /// <summary>
        /// Gets the name of the <see cref="Member"/> who sent the message.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the Url of the avatar or profile picture for the <see cref="Member"/> who sent the message.
        /// </summary>
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; internal set; }

        /// <summary>
        /// Gets the message contents.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; internal set; }

        /// <summary>
        /// Gets a list of identifiers for <see cref="Member"/> who 'liked' this message.
        /// </summary>
        [JsonProperty("favorited_by")]
        public IList<string> FavoritedBy { get; internal set; }

        /// <summary>
        /// Gets the type of sender who sent this message.
        /// </summary>
        [JsonProperty("sender_type")]
        public string SenderType { get; internal set; }

        /// <summary>
        /// Gets the platform this message was sent from.
        /// </summary>
        [JsonProperty("platform")]
        public string Platform { get; internal set; }

        /// <summary>
        /// Gets a list of <see cref="Attachments"/> attached to this <see cref="Message"/>.
        /// </summary>
        [JsonProperty("attachments")]
        public IList<Attachments.Attachment> Attachments { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Chat"/> this message belongs to.
        /// If this message is a Group Message, this field will be null.
        /// </summary>
        public virtual Chat Chat { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Group"/> this messages belongs to.
        /// If this message is a Direct message, this field will be null.
        /// </summary>
        public virtual Group Group { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ImageDownloader" /> that can be used
        /// to download attachments.
        /// </summary>
        [NotMapped]
        public ImageDownloader ImageDownloader
        {
            get
            {
                if (this.Group != null)
                {
                    return this.Group.Client.ImageDownloader;
                }
                else if (this.Chat != null)
                {
                    return this.Chat.Client.ImageDownloader;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="Message"/> that can be sent to a <see cref="Group"/>.
        /// </summary>
        /// <param name="body">The message contents.</param>
        /// <param name="attachments">A list of attachments to be included with the message.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static Message CreateMessage(string body, IEnumerable<Attachments.Attachment> attachments = null)
        {
            if (attachments == null)
            {
                attachments = Enumerable.Empty<Attachments.Attachment>();
            }

            var msg = new Message()
            {
                SourceGuid = Guid.NewGuid().ToString(),
                Text = body,
                Attachments = new List<Attachments.Attachment>(attachments),
            };

            return msg;
        }

        /// <summary>
        /// Likes this <see cref="Message"/>.
        /// </summary>
        /// <returns>True if successful.</returns>
        public async Task<bool> LikeMessage()
        {
            var conversationId = this.ConversationId ?? this.GroupId;
            var groupmeClient = this.Chat?.Client ?? this.Group?.Client;

            var request = groupmeClient.CreateRestRequest($"/messages/{conversationId}/{this.Id}/like", Method.POST);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await groupmeClient.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return restResponse.StatusCode == System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// Unlikes this <see cref="Message"/>.
        /// </summary>
        /// <returns>True if successful.</returns>
        public async Task<bool> UnlikeMessage()
        {
            var conversationId = this.ConversationId ?? this.GroupId;
            var groupmeClient = this.Chat?.Client ?? this.Group?.Client;

            var request = groupmeClient.CreateRestRequest($"/messages/{conversationId}/{this.Id}/unlike", Method.POST);

            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await groupmeClient.ApiClient.ExecuteTaskAsync(request, cancellationTokenSource.Token);

            return restResponse.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
