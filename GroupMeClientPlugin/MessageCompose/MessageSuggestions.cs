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
        /// Initializes a new instance of the <see cref="MessageSuggestions"/> class.
        /// </summary>
        public MessageSuggestions()
        {
            this.TextOptions = new List<string>();
            this.ImageOptions = new List<byte[]>();
        }

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
