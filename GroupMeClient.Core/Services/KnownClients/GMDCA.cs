namespace GroupMeClient.Core.Services.KnownClients
{
    /// <summary>
    /// <see cref="GMDCA"/> defines the application identity characteristics for the GroupMe Desktop Client / Avalonia application.
    /// </summary>
    public class GMDCA : IClientIdentityService
    {
        /// <summary>
        /// Gets the friendly display name for GMDC.
        /// </summary>
        public static string GMDCAFriendlyName => "GroupMe Desktop Client Avalonia";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent from GMDC.
        /// </summary>
        public static string GMDCAGuidPrefix => "gmdca";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent as responses from GMDC.
        /// </summary>
        public static string GMDCAGuidReplyPrefix => "gmdca-r";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent as responses from GMDC.
        /// </summary>
        public static string GMDCAGuidMarkdownPrefix => "gmdca-m";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent from GMDC Toast Notifications.
        /// </summary>
        public static string GMDCAGuidQuickResponsePrefix => "gmdcatoast";

        /// <summary>
        /// Gets the friendly display name for GMDC Quick Response messages.
        /// </summary>
        public static string GMDCAQuickResponseFriendlyName => "GroupMe Desktop Client Avalonia (Quick Reply)";

        /// <inheritdoc/>
        public string ClientFriendlyName => GMDCAFriendlyName;

        /// <inheritdoc/>
        public string ClientGuidPrefix => GMDCAGuidPrefix;

        /// <inheritdoc/>
        public string ClientGuidReplyPrefix => GMDCAGuidReplyPrefix;

        /// <inheritdoc/>
        public string ClientGuidMarkdownPrefix => GMDCAGuidMarkdownPrefix;

        /// <inheritdoc/>
        public string ClientGuidQuickResponsePrefix => GMDCAGuidQuickResponsePrefix;

        /// <inheritdoc/>
        public string ClientQuickResponseFriendlyName => GMDCAQuickResponseFriendlyName;
    }
}
