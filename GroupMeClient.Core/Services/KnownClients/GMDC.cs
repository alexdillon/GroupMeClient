namespace GroupMeClient.Core.Services.KnownClients
{
    /// <summary>
    /// <see cref="GMDC"/> defines the application identity characteristics for the GroupMe Desktop Client / Wpf application.
    /// </summary>
    public class GMDC : IClientIdentityService
    {
        /// <summary>
        /// Gets the friendly display name for GMDC.
        /// </summary>
        public static string GMDCFriendlyName => "GroupMe Desktop Client";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent from GMDC.
        /// </summary>
        public static string GMDCGuidPrefix => "gmdc";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent as responses from GMDC.
        /// </summary>
        public static string GMDCGuidReplyPrefix => "gmdc-r";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent from GMDC Toast Notifications.
        /// </summary>
        public static string GMDCGuidQuickResponsePrefix => "gmdctoast";

        /// <summary>
        /// Gets the friendly display name for GMDC Quick Response messages.
        /// </summary>
        public static string GMDCQuickResponseFriendlyName => "GroupMe Desktop Client (Quick Reply)";

        /// <inheritdoc/>
        public string ClientFriendlyName => GMDCFriendlyName;

        /// <inheritdoc/>
        public string ClientGuidPrefix => GMDCGuidPrefix;

        /// <inheritdoc/>
        public string ClientGuidReplyPrefix => GMDCGuidReplyPrefix;

        /// <inheritdoc/>
        public string ClientGuidQuickResponsePrefix => GMDCGuidQuickResponsePrefix;

        /// <inheritdoc/>
        public string ClientQuickResponseFriendlyName => GMDCQuickResponseFriendlyName;
    }
}
