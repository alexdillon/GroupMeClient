using System;
using System.Threading.Tasks;
using Squirrel;

namespace GroupMeClient.Updates
{
    /// <summary>
    /// <see cref="UpdateAssist"/> provides automatic upgrade functionality with Squirrel.
    /// </summary>
    public class UpdateAssist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAssist"/> class.
        /// </summary>
        public UpdateAssist()
        {
            UpdateManager.GitHubUpdateManager(this.GitHubUpdateUrl).ContinueWith(this.LoadComplete);
        }

        /// <summary>
        /// Gets a value indicating whether the application can be safely closed.
        /// Terminating during an update operation can result in a crash.
        /// </summary>
        public bool CanShutdown { get; private set; }

        /// <summary>
        /// Gets an awaitable object to monitor the status of an ongoing update operation.
        /// </summary>
        public TaskCompletionSource<bool> UpdateMonitor { get; private set; } = new TaskCompletionSource<bool>();

        private UpdateManager UpdateManager { get; set; }

        private string GitHubUpdateUrl => "https://github.com/alexdillon/GroupMeClient";

        private void LoadComplete(Task<UpdateManager> manager)
        {
            try
            {
                this.UpdateManager = manager.Result;
                this.UpdateManager.UpdateApp().ContinueWith(this.UpdateCompleted);
                this.CanShutdown = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failure while checking for updates. Message: {ex.Message}");
                this.CanShutdown = true;
            }
        }

        private void UpdateCompleted(Task<ReleaseEntry> releaseEntry)
        {
            var result = releaseEntry.Result;
            this.UpdateMonitor.SetResult(result != null);
            this.CanShutdown = true;

            this.UpdateManager.Dispose();
        }
    }
}
