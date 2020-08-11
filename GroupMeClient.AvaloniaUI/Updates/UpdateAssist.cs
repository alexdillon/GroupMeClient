using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GroupMeClient.Core.Services;
using Octokit;

namespace GroupMeClient.AvaloniaUI.Updates
{
    /// <summary>
    /// <see cref="UpdateAssist"/> provides automatic upgrade functionality with Squirrel.
    /// </summary>
    public class UpdateAssist : IUpdateService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAssist"/> class.
        /// </summary>
        public UpdateAssist()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the application can be safely closed.
        /// Terminating during an update operation can result in a crash.
        /// </summary>
        public bool CanShutdown { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the application running from the installation path.
        /// If false, this is typically caused by running a debug copy from a build directory, indicating that
        /// updates cannot be accurately installed.
        /// </summary>
        public bool IsInstalled { get; private set; }

        /// <summary>
        /// Gets an awaitable object to monitor the status of an ongoing update operation.
        /// </summary>
        public TaskCompletionSource<bool?> UpdateMonitor { get; private set; }

        /// <inheritdoc/>
        public Task<IEnumerable<ReleaseInfo>> GetVersionsAsync()
        {
            return Task.FromResult(Enumerable.Empty<ReleaseInfo>());
        }

        /// <inheritdoc/>
        public void StartUpdateTimer(TimeSpan interval)
        {
        }

        /// <inheritdoc/>
        public void CancelUpdateTimer()
        {
        }

        /// <inheritdoc/>
        public void BeginCheckForUpdates()
        {
        }
    }
}
