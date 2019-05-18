using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LibGroupMe.Models.Attachments
{
    public class EmojiAttachment : Attachment
    {
        public override string Type { get; } = "emoji";

        [JsonProperty("placeholder")]
        public string Placeholder { get; set; }

        [JsonProperty("charmap")]
        public IList<IList<int>> Charmap { get; set; }
    }
}
