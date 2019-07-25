using System.Collections.Generic;

namespace GroupMeClientPlugin.MessageCompose
{
    /// <summary>
    /// <see cref="MessageSuggestions"/> contains the results generated from a <see cref="IMessageComposePlugin"/>,
    /// which are returned to, and displayed in, the GroupMe Desktop Client.
    /// </summary>
    public class MessageSuggestions
    {
        /// <summary>
        /// Gets or sets the listing of text messages that are suggested.
        /// </summary>
        public List<string> TextOptions { get; set; }

        /// <summary>
        /// Gets or sets the listing of images that are suggested.
        /// </summary>
        public List<byte[]> ImageOptions { get; set; }
    }
}
