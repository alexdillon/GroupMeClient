using System.Collections.Generic;
using Newtonsoft.Json;

namespace GroupMeClientApi.Models.Attachments
{
    /// <summary>
    /// Represents an attachment to a GroupMe <see cref="Message"/> containing mentions (ex, @Username).
    /// </summary>
    public class MentionsAttachment : Attachment
    {
        /// <inheritdoc/>
        public override string Type { get; } = "mentions";

        /// <summary>
        /// Gets the user ids of the mentioned users.
        /// </summary>
        [JsonProperty("user_ids")]
        public string[] UserIds { get; internal set; }

        /// <summary>
        /// Gets the text starting index and length of each mention string.
        /// </summary>
        [JsonProperty("loci")]
        public List<int[]> LocationIndicies { get; internal set; }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> for each mention contained in this attachment.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/>.</returns>
        public IEnumerable<(string id, int startIndex, int length)> Mentions()
        {
            for (int i = 0; i < this.UserIds.Length; i++)
            {
                yield return (
                    id: this.UserIds[i],
                    startIndex: this.LocationIndicies[i][0],
                    length: this.LocationIndicies[i][1]);
            }
        }
    }
}
