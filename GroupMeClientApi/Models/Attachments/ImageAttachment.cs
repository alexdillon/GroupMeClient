using Newtonsoft.Json;

namespace GroupMeClientApi.Models.Attachments
{
    /// <summary>
    /// Represents an attachment to a GroupMe <see cref="Message"/> containing an image.
    /// </summary>
    public class ImageAttachment : Attachment
    {
        /// <inheritdoc/>
        public override string Type { get; } = "image";

        /// <summary>
        /// Gets the URL of the image attachment.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; internal set; }
    }
}