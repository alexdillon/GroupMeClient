using Newtonsoft.Json;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class MessagesList
    {
        public class MessageListResponse
        {
            [JsonProperty("count")]
            public int Count { get; set; }

            [JsonProperty("messages")]
            public IList<Message> Messages { get; set; }
        }

        [JsonProperty("response")]
        public MessageListResponse Response { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
