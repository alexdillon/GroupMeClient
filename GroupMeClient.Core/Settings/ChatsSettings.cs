using System.Collections.Generic;

namespace GroupMeClient.Core.Settings
{
    /// <summary>
    /// <see cref="ChatsSettings"/> defines the settings and persistant storage needed for Groups and Chats displays.
    /// </summary>
    public class ChatsSettings
    {
        /// <summary>
        /// Gets or sets a list of the Group or Chat IDs for all currently opened chats.
        /// </summary>
        public List<string> OpenChats { get; set; } = new List<string>();
    }
}
