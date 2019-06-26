namespace GroupMeClientApi.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a GroupMe User.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Gets the Member's Global Id for GroupMe. Not used in <see cref="Chat"/>.
        /// </summary>
        [JsonProperty("user_id")]
        public string UserId { get; internal set; }

        /// <summary>
        /// Gets the user's nickname.
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; internal set; }

        /// <summary>
        /// Gets the Url to the user's profile picture.
        /// </summary>
        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        /// <summary>
        /// Gets the Url to the user's avatar picture.
        /// </summary>
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; internal set; }

        /// <summary>
        /// Gets the Url to the user's avatar or group profile picture.
        /// </summary>
        public string ImageOrAvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(this.ImageUrl))
                {
                    return this.AvatarUrl;
                }
                else
                {
                    return this.ImageUrl;
                }
            }
        }

        /// <summary>
        /// Gets the Member's unique identifier within a <see cref="Group"/>, or global identifier within a <see cref="Chat"/>.
        /// </summary>
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
