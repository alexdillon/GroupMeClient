using Newtonsoft.Json;

namespace LibGroupMe.Models.Attachments
{
    public class ImageAttachment : Attachment
    {
        public override string Type { get; } = "image";

        [JsonProperty("url")]
        public string Url { get; internal set; }
    }
}
