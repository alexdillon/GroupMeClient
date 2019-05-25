namespace LibGroupMe.Models.Attachments
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an attachment to a GroupMe <see cref="Message"/> containing a location.
    /// </summary>
    public class LocationAttachment : Attachment
    {
        /// <inheritdoc/>
        public override string Type { get; } = "location";

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        [JsonProperty("lat")]
        public string Latitude { get; internal set; }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        [JsonProperty("lng")]
        public string Longitude { get; internal set; }

        /// <summary>
        /// Gets the location name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
