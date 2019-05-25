using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    /// <summary>
    /// <see cref="Message"/> represents a message in a GroupMe <see cref="Group"/> or <see cref="Chat"/>
    /// </summary>
    public class Message
    {
        [JsonProperty("id")]
        public string Id { get; internal set; }

        [JsonProperty("source_guid")]
        public string SourceGuid { get; internal set; }

        [JsonProperty("recipient_id")]
        public string RecipientId { get; internal set; }

        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; internal set; }

        [JsonProperty("user_id")]
        public int UserId { get; internal set; }

        [JsonProperty("group_id")]
        public int GroupId { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; internal set; }

        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("system")]
        public bool System { get; internal set; }

        [JsonProperty("favorited_by")]
        public IList<string> FavoritedBy { get; internal set; }

        [JsonProperty("sender_type")]
        public string SenderType { get; internal set; }

        [JsonProperty("platform")]
        public string Platform { get; internal set; }

        [JsonProperty("attachments")]
        public IList<Attachments.Attachment> Attachments { get; internal set; }

        internal Message()
        {
        }

        public Message CreateGroupMessage(Group group, string body, IEnumerable<Attachments.Attachment> attachments)
        {
            var msg = new Message()
            {
                SourceGuid = Guid.NewGuid().ToString(),
                Text = body,
                Attachments = new List<Attachments.Attachment>(attachments)
            };

            return msg;

        }

        public Message CreateDirectMessage(Member otherUser, string body, IEnumerable<Attachments.Attachment> attachments)
        {
            var msg = new Message()
            {
                SourceGuid = Guid.NewGuid().ToString(),
                RecipientId = otherUser.Id,
                Text = body,
                Attachments = new List<Attachments.Attachment>(attachments)
            };

            return msg;
        }
    }
}
