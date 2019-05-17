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
        public IList<int> FavoritedBy { get; set; }
    }
}
