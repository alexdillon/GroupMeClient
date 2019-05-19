using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class Group
    {
        [JsonProperty("group_id")]
        public string Id { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; internal set; }

        [JsonProperty("type")]
        public string Type { get; internal set; }

        [JsonProperty("description")]
        public string Description { get; internal set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; internal set; }

        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; internal set; }

        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; internal set; }

        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("office_mode")]
        public bool OfficeMode { get; internal set; }

        [JsonProperty("share_url")]
        public string ShareUrl { get; internal set; }

        [JsonProperty("members")]
        public IList<Member> Members { get; internal set; }

        [JsonProperty("max_members")]
        public int MaxMembers { get; internal set; }
        
        [JsonProperty("messages")]
        public MessagesPreview MsgPreview { get; internal set; }

        public class MessagesPreview
        {
            [JsonProperty("count")]
            public int Count { get; internal set; }

            [JsonProperty("last_message_id")]
            public string LastMessageId { get; internal set; }

            [JsonProperty("last_message_created_at")]
            public int LastMessageCreatedAtUnixTime { get; internal set; }

            public DateTime LastMessageCreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.LastMessageCreatedAtUnixTime).ToLocalTime().DateTime;

            public class PreviewContents
            {
                [JsonProperty("nickname")]
                public string Nickname { get; internal set; }

                [JsonProperty("text")]
                public string Text { get; internal set; }

                [JsonProperty("image_url")]
                public string ImageUrl { get; internal set; }

                [JsonProperty("attachments")]
                public IList<Attachments.Attachment> Attachments { get; internal set; }
            }

            [JsonProperty("preview")]
            public PreviewContents Preview { get; internal set; }
        }
    }
}
