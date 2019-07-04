using Newtonsoft.Json;

namespace GroupMeClientApi.Models
{
    /// <summary>
    /// <see cref="Meta"/> represents additional status information provided by the GroupMe API.
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// Gets the HTTP Status Code for an API Operation.
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; internal set; }
    }
}