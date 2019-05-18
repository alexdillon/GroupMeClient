using Newtonsoft.Json;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class Message
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source_guid")]
        public string SourceGuid { get; set; }

        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("system")]
        public bool System { get; set; }

        [JsonProperty("favorited_by")]
        public IList<string> FavoritedBy { get; set; }

        [JsonProperty("sender_type")]
        public string SenderType { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("attachments")]
        public IList<Attachments.Attachment> Attachments { get; set; }
    }
}
