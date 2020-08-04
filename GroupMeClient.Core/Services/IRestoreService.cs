namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IRestoreService"/> provides a platform-independent service for restoring the state
    /// of the GroupMe Desktop Client, as well as performing soft and hard application reboots.
    /// </summary>
    public interface IRestoreService
    {
        /// <summary>
        /// Gets a value indicating whether the application should restore the prior state on initialization.
        /// </summary>
        bool ShouldRestoreState { get; }

        /// <summary>
        /// Performs a soft application restart, which will allow
        /// the client to reopen in a state similiar to how it was prior to shutdown.
        /// </summary>
        void SoftApplicationRestart();

        /// <summary>
        /// Performs a hard application restart. Any unsaved data will be lost.
        /// </summary>
        void HardApplicationRestart();
    }
}
