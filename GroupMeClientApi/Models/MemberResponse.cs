namespace GroupMeClientApi.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Response wrapping a GroupMe User.
    /// </summary>
    internal class MemberResponse
    {
        /// <summary>
        /// Gets or sets the metadata for the response.
        /// </summary>
        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }

        /// <summary>
        /// Gets or sets the member data from the response.
        /// </summary>
        [JsonProperty("response")]
        public Member Member { get; internal set; }
    }
}
