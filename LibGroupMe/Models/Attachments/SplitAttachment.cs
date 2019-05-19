using Newtonsoft.Json;

namespace LibGroupMe.Models.Attachments
{
    public class SplitAttachment : Attachment
    {
        public override string Type { get; } = "split";

        [JsonProperty("token")]
        public string Token { get; internal set; }
    }
}
