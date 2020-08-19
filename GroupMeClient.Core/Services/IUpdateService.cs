using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IUpdateService"/> defines a platform-independent service for updating the GMDC Application.
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Gets a observable value indicating whether the application can be safely closed.
        /// Terminating during an update operation can result in a crash.
        /// </summary>
        IObservable<bool> CanShutdown { get; }

        /// <summary>
        /// Gets a value indicating whether the application running from the installation path.
        /// If false, this is typically caused by running a debug copy from a build directory, indicating that
        /// updates cannot be accurately installed.
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Gets all versions of the GMDC application that are currently published.
        /// </summary>
        /// <returns>A collection of <see cref="ReleaseInfo"/> for all published releases.</returns>
        Task<IEnumerable<ReleaseInfo>> GetVersionsAsync();

        /// <summary>
        /// Starts a background task that will run and check for updates on a regular interval.
        /// </summary>
        /// <param name="interval">The interval on which to check for updates.</param>
        void StartUpdateTimer(TimeSpan interval);

        /// <summary>
        /// Cancels the background update timer.
        /// </summary>
        void CancelUpdateTimer();

        /// <summary>
        /// Checks for updates and automatically installs any applicable updates.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// A return value of true indicates that updates were successfully installed.
        /// A return value of false indicates that updates were not required because the current version is up-to-date.
        /// A return value of null indicates that the update service was unavailable.</returns>
        Task<bool?> CheckForUpdatesAsync();
    }
}
