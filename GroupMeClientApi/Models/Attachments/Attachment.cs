using JsonSubTypes;
using Newtonsoft.Json;

namespace GroupMeClientApi.Models.Attachments
{
    /// <summary>
    /// Generic type to represent an attachment to a GroupMe <see cref="Message"/>.
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(EmojiAttachment), "emoji")]
    [JsonSubtypes.KnownSubType(typeof(ImageAttachment), "image")]
    [JsonSubtypes.KnownSubType(typeof(LinkedImageAttachment), "linked_image")]
    [JsonSubtypes.KnownSubType(typeof(VideoAttachment), "video")]
    [JsonSubtypes.KnownSubType(typeof(LocationAttachment), "location")]
    [JsonSubtypes.KnownSubType(typeof(SplitAttachment), "split")]
    [JsonSubtypes.KnownSubType(typeof(MentionsAttachment), "mentions")]
    public class Attachment
    {
        /// <summary>
        /// Gets the attachment type.
        /// </summary>
        public virtual string Type { get; }
    }
}
