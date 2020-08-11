namespace GroupMeClient.Core.Settings
{
    /// <summary>
    /// <see cref="CoreSettings"/> defines the settings needed for basic operation.
    /// </summary>
    public class CoreSettings
    {
        /// <summary>
        /// Gets or sets the authorization token used for GroupMe Api Operations.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the time interval, in minutes, for how frequently the application should check for available
        /// updates.
        /// </summary>
        public int ApplicationUpdateFrequencyMinutes { get; set; } = 60;
    }
}
