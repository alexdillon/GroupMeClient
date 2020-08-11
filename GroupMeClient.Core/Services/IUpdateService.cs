using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GroupMeClient.Core.Services
{
    public interface IUpdateService
    {
        /// <summary>
        /// Gets a value indicating whether the application can be safely closed.
        /// Terminating during an update operation can result in a crash.
        /// </summary>
        bool CanShutdown { get; }

        /// <summary>
        /// Gets a value indicating whether the application running from the installation path.
        /// If false, this is typically caused by running a debug copy from a build directory, indicating that
        /// updates cannot be accurately installed.
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Gets an awaitable object to monitor the status of an ongoing update operation.
        /// </summary>
        TaskCompletionSource<bool?> UpdateMonitor { get; }

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
        /// Begins checking for updates, and automatically installing and applicable updates in the background.
        /// </summary>
        void BeginCheckForUpdates();
    }
}
