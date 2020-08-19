using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.Utilities
{
    /// <summary>
    /// <see cref="MessageUtils"/> provides support utilities for parsing <see cref="Message"/>s.
    /// </summary>
    public class MessageUtils
    {
        /// <summary>
        /// Regular Expression to match a message as being one containing a GMDC Reply Extension attachment, Generation 1.
        /// </summary>
        public const string RepliedMessageRegex = @"\n\/rmid:[0-9]*";

        /// <summary>
        /// Determines if a specific <see cref="Message"/> is sent using the GMDC-specific Reply extension Generation 1 method.
        /// Generation 1 replies contain an identifiying tag on a newline at the end of the message body in the format
        /// \rmid:[numerical-message-id].
        /// </summary>
        /// <param name="message">The message to check for being a reply.</param>
        /// <returns>True if the given <see cref="Message"/> is a reply, otherwise, false.</returns>
        public static bool IsReplyGen1(Message message)
        {
            return !string.IsNullOrEmpty(message.Text) && Regex.IsMatch(message.Text, RepliedMessageRegex);
        }

        /// <summary>
        /// Determines if a specific <see cref="Message"/> is sent using the GMDC-specific Reply extension Generation 2 method.
        /// Generation 2 includes a specific reply prefix on the <see cref="Message.SourceGuid"/> field, followed with the unique
        /// identitifer of the message being replied to.
        /// </summary>
        /// <param name="message">The message to check for being a reply.</param>
        /// <returns>True if the given <see cref="Message"/> is a reply, otherwise, false.</returns>
        public static bool IsReplyGen2(Message message)
        {
            string[] knownClientReplyPrefixes =
            {
                Services.KnownClients.GMDC.GMDCGuidReplyPrefix,
                Services.KnownClients.GMDCA.GMDCAGuidReplyPrefix,
            };

            foreach (var clientPrefix in knownClientReplyPrefixes)
            {
                if (message.SourceGuid.StartsWith(clientPrefix))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a specific <see cref="Message"/> is sent using the GMDC-specific Reply extension method.
        /// This method tests for replies sent using any generation of the GMDC client.
        /// </summary>
        /// <param name="message">The message to check for being a reply.</param>
        /// <returns>True if the given <see cref="Message"/> is a reply, otherwise, false.</returns>
        public static bool IsGMDCReply(Message message)
        {
            return
                IsReplyGen2(message) ||
                IsReplyGen1(message);
        }
    }
}
