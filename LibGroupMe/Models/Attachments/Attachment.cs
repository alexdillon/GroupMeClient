using JsonSubTypes;
using Newtonsoft.Json;

namespace LibGroupMe.Models.Attachments
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(EmojiAttachment), "emoji")]
    [JsonSubtypes.KnownSubType(typeof(ImageAttachment), "image")]
    [JsonSubtypes.KnownSubType(typeof(LocationAttachment), "location")]
    [JsonSubtypes.KnownSubType(typeof(SplitAttachment), "split")]
    public class Attachment
    {
        public virtual string Type { get; }
    }
}
