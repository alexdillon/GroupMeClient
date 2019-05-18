using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LibGroupMe.Models
{
    public class Group
    {
        [JsonProperty("group_id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("created_at")]
        public int CreatedAtUnixTime { get; set; }

        public DateTime CreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.CreatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("updated_at")]
        public int UpdatedAtUnixTime { get; set; }

        public DateTime UpdatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.UpdatedAtUnixTime).ToLocalTime().DateTime;

        [JsonProperty("office_mode")]
        public bool OfficeMode { get; set; }

        [JsonProperty("share_url")]
        public string ShareUrl { get; set; }

        [JsonProperty("members")]
        public IList<Member> Members { get; set; }

        [JsonProperty("max_members")]
        public int MaxMembers { get; set; }
        
        [JsonProperty("messages")]
        public MessagesPreview MsgPreview { get; set; }

        public class MessagesPreview
        {
            [JsonProperty("count")]
            public int Count { get; set; }

            [JsonProperty("last_message_id")]
            public string LastMessageId { get; set; }

            [JsonProperty("last_message_created_at")]
            public int LastMessageCreatedAtUnixTime { get; set; }

            public DateTime LastMessageCreatedAtTime => DateTimeOffset.FromUnixTimeSeconds(this.LastMessageCreatedAtUnixTime).ToLocalTime().DateTime;

            [JsonProperty("preview")]
            public PreviewContents Preview { get; set; }

            public class PreviewContents
            {
                [JsonProperty("nickname")]
                public string Nickname { get; set; }

                [JsonProperty("text")]
                public string Text { get; set; }

                [JsonProperty("image_url")]
                public string ImageUrl { get; set; }

                [JsonProperty("attachments")]
                public IList<Attachments.Attachment> Attachments { get; set; }
            }
        }
    }
}
