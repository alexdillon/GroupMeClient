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
        public static string GMDCFriendlyName => "GroupMe Desktop Client Avalonia";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent from GMDC.
        /// </summary>
        public static string GMDCGuidPrefix => "gmdca-";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent as responses from GMDC.
        /// </summary>
        public static string GMDCGuidReplyPrefix => "gmdca-r";

        /// <summary>
        /// Gets the guid prefix applied to all messages sent from GMDC Toast Notifications.
        /// </summary>
        public static string GMDCGuidQuickResponsePrefix => "gmdcatoast-";

        /// <summary>
        /// Gets the friendly display name for GMDC Quick Response messages.
        /// </summary>
        public static string GMDCQuickResponseFriendlyName => "GroupMe Desktop Client Avalonia (Quick Reply)";

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
