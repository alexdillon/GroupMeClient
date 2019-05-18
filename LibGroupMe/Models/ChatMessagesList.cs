using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class ChatMessagesList
    {
        public class MessageListResponse
        {
            [JsonProperty("count")]
            public int Count { get; set; }

            [JsonProperty("direct_messages")]
            public IList<Message> Messages { get; set; }

            public class ReadReceipt
            {
                [JsonProperty("id")]
                public string Id { get; set; }

                [JsonProperty("chat_id")]
                public string ChatId { get; set; }

                [JsonProperty("message_id")]
                public string MessageId { get; set; }

                [JsonProperty("user_id")]
                public string UserId { get; set; }

                [JsonProperty("read_at")]
                public int ReadAtUnixTime { get; set; }

                public DateTime ReadAtTime => DateTimeOffset.FromUnixTimeSeconds(this.ReadAtUnixTime).ToLocalTime().DateTime;
            }

            [JsonProperty("read_receipt")]
            public ReadReceipt LastReadReceipt { get; set; }
        }

        [JsonProperty("response")]
        public MessageListResponse Response { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
