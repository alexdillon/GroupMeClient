using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GroupMeClientApi.Models
{
    /// <summary>
    /// Contains a list of <see cref="Message"/>s returned from a <see cref="Chat"/>, along with status information.
    /// </summary>
    public class ChatMessagesList
    {
        /// <summary>
        /// Gets the response data containing the messages.
        /// </summary>
        [JsonProperty("response")]
        public MessageListResponse Response { get; internal set; }

        /// <summary>
        /// Gets the metadata containing additional status information from GroupMe.
        /// </summary>
        [JsonProperty("meta")]
        public Meta Meta { get; internal set; }

        /// <summary>
        /// Contains a list of messages and supporting information for a <see cref="Chat"/>.
        /// </summary>
        public class MessageListResponse
        {
            /// <summary>
            /// Gets the number of messages contained in a <see cref="Chat"/>.
            /// </summary>
            [JsonProperty("count")]
            public int Count { get; internal set; }

            /// <summary>
            /// Gets the <see cref="Message"/>s in a <see cref="Chat"/>.
            /// </summary>
            [JsonProperty("direct_messages")]
            public ICollection<Message> Messages { get; internal set; }

            /// <summary>
            /// Gets the last <see cref="ReadReceipt"/> for this <see cref="Chat"/>.
            /// </summary>
            [JsonProperty("read_receipt")]
            public ReadReceipt LastReadReceipt { get; internal set; }

            /// <summary>
            /// Represents a read receipt indicating information on whether a message has been received and read.
            /// </summary>
            public class ReadReceipt
            {
                /// <summary>
                /// Gets the identifier for the receipt.
                /// </summary>
                [JsonProperty("id")]
                public string Id { get; internal set; }

                /// <summary>
                /// Gets the identifier for the <see cref="Chat"/> the receipt is associated with.
                /// </summary>
                [JsonProperty("chat_id")]
                public string ChatId { get; internal set; }

                /// <summary>
                /// Gets the identifier for the <see cref="Message"/> the receipt is associated with.
                /// </summary>
                [JsonProperty("message_id")]
                public string MessageId { get; internal set; }

                /// <summary>
                /// Gets the identifier for <see cref="Member"/> the receipt was sent from.
                /// </summary>
                [JsonProperty("user_id")]
                public string UserId { get; internal set; }

                /// <summary>
                /// Gets the Unix Timestamp for when the associated <see cref="Message"/> was read by the <see cref="Member"/>.
                /// </summary>
                [JsonProperty("read_at")]
                public int ReadAtUnixTime { get; internal set; }

                /// <summary>
                /// Gets the Date and Time for when the associated <see cref="Message"/> was read by the <see cref="Member"/>.
                /// </summary>
                public DateTime ReadAtTime => DateTimeOffset.FromUnixTimeSeconds(this.ReadAtUnixTime).ToLocalTime().DateTime;
            }
        }
    }
}
