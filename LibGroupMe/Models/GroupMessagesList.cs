using Newtonsoft.Json;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class GroupMessagesList
    {
        public class MessageListResponse
        {
            [JsonProperty("count")]
            public int Count { get; internal set; }

            [JsonProperty("messages")]
            public IList<Message> Messages { get; internal set; }
        }

        [JsonProperty("response")]
        public MessageListResponse Response { get; internal set; }

        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }
    }
}
