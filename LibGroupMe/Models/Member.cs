namespace LibGroupMe.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a GroupMe User.
    /// </summary>
    public class Member
    {
        [JsonProperty("user_id")]
        public string UserId { get; internal set; }

        /// <summary>
        /// Gets the user's nickname.
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }

        /// <summary>
        /// Gets the Url to the user's avatar or profile picture.
        /// </summary>
        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        [JsonProperty("id")]
        public string Id { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether gets whether the user has the <see cref="Group"/> or <see cref="Chat"/> muted.
        /// </summary>
        [JsonProperty("muted")]
        public bool Muted { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether gets whether the user has been autokicked from a <see cref="Group"/>.
        /// </summary>
        [JsonProperty("autokicked")]
        public bool Autokicked { get; internal set; }

        /// <summary>
        /// Gets a list of roles within a <see cref="Group"/> that a user has.
        /// </summary>
        [JsonProperty("roles")]
        public IList<string> Roles { get; internal set; }

        /// <summary>
        /// Gets a user's full name or username.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }
    }
}
