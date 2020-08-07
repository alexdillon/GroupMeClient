using System;
using System.Collections.Generic;
using System.Text;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IClientIdentityService"/> defines an interface to allow client implementations to set the identity of
    /// the GroupMe Desktop Client Core.
    /// </summary>
    public interface IClientIdentityService
    {
        /// <summary>
        /// Gets the friendly name of the application that should be shown in the UI.
        /// </summary>
        string ClientFriendlyName { get; }

        /// <summary>
        /// Gets the GUID prefix used for messages being sent.
        /// </summary>
        string ClientGuidPrefix { get; }

        /// <summary>
        /// Gets the GUID prefix used for reply messages.
        /// </summary>
        string ClientGuidReplyPrefix { get; }

        /// <summary>
        /// Gets the GUID prefix used for messages being quickly sent through interactive notifications.
        /// </summary>
        string ClientGuidQuickResponsePrefix { get; }

        /// <summary>
        /// Gets the friendly name identifying name for the a client that has sent a Quick Response message.
        /// </summary>
        string ClientQuickResponseFriendlyName { get; }
    }
}
