using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace GroupMeClientApi.Models
{
    /// <summary>
    /// <see cref="Message"/> represents a message in a GroupMe <see cref="Group"/> or <see cref="Chat"/>.
    /// </summary>
    public class Message : IAvatarSource
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
        /// Gets the time when the message was created.
        /// </summary>
        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

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
        /// Gets the conversation identifier for a <see cref="Chat"/> where this message was sent.
        /// This property is ONLY used with with <see cref="Push.Notifications.DirectMessageCreateNotification"/>.
        /// If this <see cref="Message"/> represents a Group Message, this field will be null.
        /// This property should be the same as <see cref="ConversationId"/>.
        /// </summary>
        [JsonProperty("chat_id")]
        public string ChatId { get; internal set; }

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
        public ICollection<Attachments.Attachment> Attachments { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Chat"/> this message belongs to.
        /// If this message is a Group Message, this field will be null.
        /// </summary>
        public Chat Chat { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Group"/> this messages belongs to.
        /// If this message is a Direct message, this field will be null.
        /// </summary>
        public Group Group { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ImageDownloader" /> that can be used
        /// to download attachments.
        /// </summary>
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

        /// <inheritdoc />
        string IAvatarSource.ImageOrAvatarUrl => this.AvatarUrl;

        /// <inheritdoc />
        bool IAvatarSource.IsRoundedAvatar => true;

        /// <summary>
        /// Creates a new <see cref="Message"/> that can be sent to a <see cref="Group"/>.
        /// </summary>
        /// <param name="body">The message contents.</param>
        /// <param name="attachments">A list of attachments to be included with the message.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static Message CreateMessage(string body, IEnumerable<Attachments.Attachment> attachments = null)
        {
            var msg = new Message()
            {
                SourceGuid = Guid.NewGuid().ToString(),
                Text = body,
                Attachments = new List<Attachments.Attachment>(attachments ?? Enumerable.Empty<Attachments.Attachment>()),
                FavoritedBy = new List<string>(),
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

        /// <summary>
        /// Associates this <see cref="Message"/> with a <see cref="Group"/>.
        /// This will enable API operations on a message that was not retreived directly from a <see cref="GroupMeClient"/>.
        /// Unassociated messages may be loaded or deserialized from a database or other persistant storage.
        /// </summary>
        /// <param name="group">The group to associate this message with.</param>
        public void AssociateWithGroup(Group group)
        {
            this.Group = group;
        }

        /// <summary>
        /// Associates this <see cref="Message"/> with a <see cref="Chat"/>.
        /// This will enable API operations on a message that was not retreived directly from a <see cref="GroupMeClient"/>.
        /// Unassociated messages may be loaded or deserialized from a database or other persistant storage.
        /// </summary>
        /// <param name="chat">The chat to associate this message with.</param>
        public void AssociateWithChat(Chat chat)
        {
            this.Chat = chat;
        }
    }
}
