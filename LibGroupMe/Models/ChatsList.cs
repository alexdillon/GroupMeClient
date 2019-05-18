using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibGroupMe.Models
{
    public class ChatsList
    {
        [JsonProperty("response")]
        public IList<Chat> Chats { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
