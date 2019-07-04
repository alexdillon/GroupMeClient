using System.Collections.Generic;
using System.Linq;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;

namespace GroupMeClientApi
{
    /// <summary>
    /// Provides methods to merge a new GroupMe object with an existing
    /// cached database entry.
    /// </summary>
    internal class DataMerger
    {
        /// <summary>
        /// Merges a cached Group from the database with updated information
        /// from GroupMe.
        /// </summary>
        /// <param name="dest">The Group to copy into.</param>
        /// <param name="source">The Group to copy from.</param>
        internal static void MergeGroup(Group dest, Group source)
        {
            dest.CreatedAtUnixTime = source.CreatedAtUnixTime;
            dest.Description = source.Description;
            dest.Id = source.Id;
            dest.ImageUrl = source.ImageUrl;
            dest.MaxMembers = source.MaxMembers;
            dest.MsgPreview = source.MsgPreview;
            dest.Name = source.Name;
            dest.OfficeMode = source.OfficeMode;
            dest.PhoneNumber = source.PhoneNumber;
            dest.ShareUrl = source.ShareUrl;
            dest.Type = source.Type;
            dest.UpdatedAtUnixTime = source.UpdatedAtUnixTime;

            foreach (var msg in source.Messages)
            {
                if (!dest.Messages.Any(m => m.Id == msg.Id))
                {
                    msg.Group = dest;
                    dest.Messages.Add(msg);
                }
            }

            source.Members.Clear();
            foreach (var member in source.Members)
            {
                dest.Members.Add(member);
            }
        }

        /// <summary>
        /// Merges a cached Chat from the database with updated information
        /// from GroupMe.
        /// </summary>
        /// <param name="dest">The Chat to copy into.</param>
        /// <param name="source">The Chat to copy from.</param>
        internal static void MergeChat(Chat dest, Chat source)
        {
            dest.CreatedAtUnixTime = source.CreatedAtUnixTime;
            dest.Id = source.Id;
            dest.LatestMessage = source.LatestMessage;
            dest.UpdatedAtUnixTime = source.UpdatedAtUnixTime;

            MergeMember(dest.OtherUser, source.OtherUser);

            foreach (var msg in source.Messages)
            {
                if (!dest.Messages.Any(m => m.Id == msg.Id))
                {
                    msg.Chat = dest;
                    dest.Messages.Add(msg);
                }
            }
        }

        /// <summary>
        /// Merges a cached Member from the database with updated information
        /// from GroupMe.
        /// </summary>
        /// <param name="dest">The Member to copy into.</param>
        /// <param name="source">The Member to copy from.</param>
        internal static void MergeMember(Member dest, Member source)
        {
            dest.Autokicked = source.Autokicked;
            dest.AvatarUrl = source.AvatarUrl;
            dest.Id = source.Id;
            dest.ImageUrl = source.ImageUrl;
            dest.Muted = source.Muted;
            dest.Name = source.Name;
            dest.Nickname = source.Nickname;
            dest.UserId = source.UserId;

            if (dest.Roles == null && source.Roles != null)
            {
                dest.Roles = new List<string>(source.Roles);
            }
            else if (dest.Roles != null)
            {
                dest.Roles.Clear();
                foreach (var role in source.Roles)
                {
                    dest.Roles.Add(role);
                }
            }
        }

        /// <summary>
        /// Merges a cached Member from the database with updated information
        /// from GroupMe.
        /// </summary>
        /// <param name="dest">The Message to copy from.</param>
        /// <param name="source">The Message to into.</param>
        internal static void MergeMessage(Message dest, Message source)
        {
            dest.AvatarUrl = source.AvatarUrl;
            dest.ConversationId = source.ConversationId;
            dest.CreatedAtUnixTime = source.CreatedAtUnixTime;
            dest.GroupId = source.GroupId;
            dest.Id = source.Id;
            dest.Name = source.Name;
            dest.Platform = source.Platform;
            dest.RecipientId = source.RecipientId;
            dest.SenderType = source.SenderType;
            dest.SourceGuid = source.SourceGuid;
            dest.Text = source.Text;
            dest.UserId = source.UserId;

            dest.Attachments = new List<Attachment>();
            (dest.Attachments as List<Attachment>).AddRange(source.Attachments);

            dest.FavoritedBy = new List<string>();
            (dest.FavoritedBy as List<string>).AddRange(source.FavoritedBy);
        }
    }
}
