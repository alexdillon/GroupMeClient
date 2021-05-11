using System;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// A wrapper around <see cref="GroupMeClientApi.GroupMeClient"/> that allows for automatically
    /// loading the authentication token from an instance of <see cref="Settings.SettingsManager"/>.
    /// </summary>
    public class GroupMeClientService : GroupMeClientApi.GroupMeClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMeClientService"/> class.
        /// </summary>
        /// <param name="settingsManager">The application settings instance</param>
        public GroupMeClientService(Settings.SettingsManager settingsManager)
            : base(settingsManager.CoreSettings.AuthToken)
        {
        }
    }
}
