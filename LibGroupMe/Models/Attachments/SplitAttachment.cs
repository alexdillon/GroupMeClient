namespace LibGroupMe.Models.Attachments
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an attachment to a GroupMe <see cref="Message"/> containing a bill-split.
    /// </summary>
    public class SplitAttachment : Attachment
    {
        /// <inheritdoc/>
        public override string Type { get; } = "split";

        /// <summary>
        /// Gets the token associated with this Split.
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; internal set; }
    }
}
