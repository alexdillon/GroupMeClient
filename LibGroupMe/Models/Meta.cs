using Newtonsoft.Json;

namespace LibGroupMe.Models
{
    public class Meta
    {
        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
