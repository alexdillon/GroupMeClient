using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using GroupMeClient.Core.Services;

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

        /// <inheritdoc/>
        public IObservable<bool> CanShutdown => this.CanShutdownSource;

        /// <inheritdoc/>
        public bool IsInstalled { get; private set; }

        private BehaviorSubject<bool> CanShutdownSource { get; } = new BehaviorSubject<bool>(true);

        /// <inheritdoc/>
        public Task<IEnumerable<ReleaseInfo>> GetVersionsAsync()
        {
            return Task.FromResult(Enumerable.Empty<ReleaseInfo>());
        }

        /// <inheritdoc/>
        public Task<bool?> CheckForUpdatesAsync()
        {
            return Task.FromResult<bool?>(null);
        }

        /// <inheritdoc/>
        public void StartUpdateTimer(TimeSpan interval)
        {
        }

        /// <inheritdoc/>
        public void CancelUpdateTimer()
        {
        }
    }
}
