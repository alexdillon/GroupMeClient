using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibGroupMe.Models
{
    public class MessagesList
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("last_message_id")]
        public string LastMessageId { get; set; }

        [JsonProperty("last_message_created_at")]
        public int LastMessageCreatedAtUnixTime { get; set; }

        public DateTime LastMessageCreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.LastMessageCreatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("preview")]
        public Message PreviewMessage { get; set; }

    }
}
