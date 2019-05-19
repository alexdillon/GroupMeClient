using Newtonsoft.Json;

namespace LibGroupMe.Models.Attachments
{
    public class LocationAttachment : Attachment
    {
        public override string Type { get; } = "location";

        [JsonProperty("lat")]
        public string Latitude { get; internal set; }

        [JsonProperty("lng")]
        public string Longitude { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
