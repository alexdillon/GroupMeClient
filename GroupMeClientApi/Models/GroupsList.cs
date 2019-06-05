namespace GroupMeClientApi.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// <see cref="GroupsList"/> provides a list of Groups along with additional status information.
    /// </summary>
    public class GroupsList
    {
        /// <summary>
        /// Gets the list of Groups.
        /// </summary>
        [JsonProperty("response")]
        public IList<Group> Groups { get; internal set; }

        /// <summary>
        /// Gets the Metadata for the API Call.
        /// </summary>
        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }
    }
}