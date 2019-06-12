namespace GroupMeClientCached.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using GroupMeClientApi.Models;

    /// <summary>
    /// Provides a cachable copy of the <see cref="Chat"/> class.
    /// </summary>
    public class CachedChat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedChat"/> class.
        /// </summary>
        public CachedChat()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedChat"/> class.
        /// </summary>
        /// <param name="chat">Chat object to copy metadata from.</param>
        public CachedChat(Chat chat)
        {
            this.OtherUser = chat.OtherUser;
            this.CreatedAtUnixTime = chat.CreatedAtUnixTime;
            this.UpdatedAtUnixTime = chat.UpdatedAtUnixTime;

            this.CachedMessages = new List<Message>();
        }

        /// <summary>
        /// Gets the Identifier of this Chat. See <seealso cref="OtherUser"/> for more information.
        /// </summary>
        [Key]
        public string Id
        {
            get
            {
                return this.OtherUser.Id;
            }

            internal set
            {
                // Does nothing. A property must have gets and sets to be a PrimaryKey
            }
        }

        /// <summary>
        /// Gets the <see cref="Member"/> that this chat is being held with.
        /// </summary>
        [Key]
        public Member OtherUser { get; internal set; }

        /// <summary>
        /// Gets the Unix Timestamp for when this chat was created.
        /// </summary>
        public int CreatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets the Unix Timestamp for when this chat was last updated.
        /// </summary>
        public int UpdatedAtUnixTime { get; internal set; }

        /// <summary>
        /// Gets a list of <see cref="Message"/> that have been cached.
        /// </summary>
        public IList<Message> CachedMessages { get; internal set; }
    }
}
