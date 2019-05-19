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
            public int Count { get; internal set; }

            [JsonProperty("direct_messages")]
            public IList<Message> Messages { get; internal set; }

            public class ReadReceipt
            {
                [JsonProperty("id")]
                public string Id { get; internal set; }

                [JsonProperty("chat_id")]
                public string ChatId { get; internal set; }

                [JsonProperty("message_id")]
                public string MessageId { get; internal set; }

                [JsonProperty("user_id")]
                public string UserId { get; internal set; }

                [JsonProperty("read_at")]
                public int ReadAtUnixTime { get; internal set; }

                public DateTime ReadAtTime => DateTimeOffset.FromUnixTimeSeconds(this.ReadAtUnixTime).ToLocalTime().DateTime;
            }

            [JsonProperty("read_receipt")]
            public ReadReceipt LastReadReceipt { get; internal set; }
        }

        [JsonProperty("response")]
        public MessageListResponse Response { get; internal set; }

        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }
    }
}
