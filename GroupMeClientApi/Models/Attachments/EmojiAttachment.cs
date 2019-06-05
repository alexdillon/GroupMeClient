namespace GroupMeClientApi.Models.Attachments
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an attachment to a GroupMe <see cref="Message"/> containing an Emoji.
    /// </summary>
    public class EmojiAttachment : Attachment
    {
        /// <inheritdoc/>
        public override string Type { get; } = "emoji";

        /// <summary>
        /// Gets the placeholder character being substituted in the <see cref="Message.Text"/> for the emoji.
        /// </summary>
        [JsonProperty("placeholder")]
        public string Placeholder { get; internal set; }

        /// <summary>
        /// Gets the Charmap describing the emoji.
        /// </summary>
        [JsonProperty("charmap")]
        public IList<IList<int>> Charmap { get; internal set; }
    }
}
