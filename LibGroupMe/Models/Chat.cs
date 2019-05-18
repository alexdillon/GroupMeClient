using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibGroupMe.Models
{
    public class Chat
    {
        [JsonProperty("other_user")]
        public Member OtherUser { get; set; }

        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; set; }

        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; set; }

        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("last_message")]
        public Message LatestMessage { get; set; }
    }
}
