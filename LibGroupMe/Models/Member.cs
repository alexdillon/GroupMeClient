using Newtonsoft.Json;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class Member
    {
        [JsonProperty("user_id")]
        public int UserId { get; internal set; }

        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        [JsonProperty("id")]
        public int Id { get; internal set; }

        [JsonProperty("muted")]
        public bool Muted { get; internal set; }

        [JsonProperty("autokicked")]
        public bool Autokicked { get; internal set; }

        [JsonProperty("roles")]
        public IList<string> Roles { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
