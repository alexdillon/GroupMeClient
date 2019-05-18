using Newtonsoft.Json;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class Member
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("muted")]
        public bool Muted { get; set; }

        [JsonProperty("autokicked")]
        public bool Autokicked { get; set; }

        [JsonProperty("roles")]
        public IList<string> Roles { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
