using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibGroupMe.Models
{
    public class GroupsList
    {
        [JsonProperty("response")]
        public List<Group> Groups { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
