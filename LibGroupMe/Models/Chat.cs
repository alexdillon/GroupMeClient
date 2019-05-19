using Newtonsoft.Json;
using System;

namespace LibGroupMe.Models
{
    /// <summary>
    /// <see cref="Chat"/> represents a GroupMe Direct Message (or Chat) with another user
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Gets or Sets the <see cref="Member"/> that this chat is being held with
        /// </summary>
        [JsonProperty("other_user")]
        public Member OtherUser { get; set; }

        /// <summary>
        /// Gets or Sets the Unix Timestamp for when this chat was created
        /// </summary>
        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; set; }

        /// <summary>
        /// Gets the Date and Time when this chat was created
        /// </summary>
        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets or Sets the Unix Timestamp for when this chat was last updated
        /// </summary>
        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; set; }

        /// <summary>
        /// Gets the Date and Time when this chat was last updated
        /// </summary>
        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        /// <summary>
        /// Gets or Sets the latest message in this chat
        /// </summary>
        [JsonProperty("last_message")]
        public Message LatestMessage { get; set; }
    }
}
