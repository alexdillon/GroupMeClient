using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
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
            var assembly = Assembly.GetEntryAssembly();

            var updateDotExe = Path.Combine(
              Path.GetDirectoryName(assembly.Location),
              "..",
              "Update.exe");

            var isVsDebug = updateDotExe.Contains(Path.Combine("Debug", "..", "Update.exe"));

            this.IsInstalled = File.Exists(updateDotExe) && !isVsDebug;
            this.CanShutdown = true;
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
        public TaskCompletionSource<bool> UpdateMonitor { get; private set; }

        private UpdateManager UpdateManager { get; set; }

        private bool HasAlreadyUpdated { get; set; } = false;

        private Timer UpdateTimer { get; set; }

        private string GMDCRepoUser => "alexdillon";

        private string GMDCRepoName => "GroupMeClient";

        private string GMDCGitHubRepoUrl => $"https://github.com/{this.GMDCRepoUser}/{this.GMDCRepoName}";

        /// <summary>
        /// Gets all versions of the GMDC application that are currently published.
        /// </summary>
        /// <returns>A collection of <see cref="ReleaseInfo"/> for all published releases.</returns>
        public async Task<IEnumerable<ReleaseInfo>> GetVersionsAsync()
        {
            var header = new ProductHeaderValue("GroupMeDesktopClient", ThisAssembly.SimpleVersion);
            var github = new GitHubClient(header);
            var releases = await github.Repository.Release.GetAll(this.GMDCRepoUser, this.GMDCRepoName);

            var results = new List<ReleaseInfo>();
            var isNewest = true;
            foreach (var release in releases)
            {
                results.Add(new ReleaseInfo() { Version = release.Name, ReleaseNotes = release.Body, PreRelease = release.Prerelease, IsLatest = isNewest });
                isNewest = false;
            }

            return results;
        }

        /// <summary>
        /// Starts a background task that will run and check for updates on a regular interval.
        /// </summary>
        /// <param name="interval">The interval on which to check for updates.</param>
        public void StartUpdateTimer(TimeSpan interval)
        {
            this.UpdateTimer = new Timer(
                (l) => this.BeginCheckForUpdates(),
                null,
                TimeSpan.Zero,
                interval);
        }

        /// <summary>
        /// Cancels the background update timer.
        /// </summary>
        public void CancelUpdateTimer()
        {
            this.UpdateTimer = null;
        }

        /// <summary>
        /// Begins checking for updates, and automatically installing and applicable updates in the background.
        /// </summary>
        public void BeginCheckForUpdates()
        {
            this.UpdateMonitor = new TaskCompletionSource<bool>();
            if (this.IsInstalled && !this.HasAlreadyUpdated)
            {
                // Only install updates if this is running as an installed copy
                // (i.e., not a portable installation or running under Visual Studio)
                this.CanShutdown = false;
                UpdateManager.GitHubUpdateManager(this.GMDCGitHubRepoUrl).ContinueWith(this.LoadComplete);
            }
            else
            {
                this.UpdateMonitor.SetResult(false);
                this.CanShutdown = true;
            }
        }

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

            if (result != null)
            {
                this.HasAlreadyUpdated = true;
            }

            this.UpdateManager.Dispose();
        }

        /// <summary>
        /// <see cref="ReleaseInfo"/> describes a specific version of th GMDC application.
        /// </summary>
        public class ReleaseInfo
        {
            /// <summary>
            /// Gets or sets the version string for this release.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Gets or sets the release notes for this release.
            /// </summary>
            public string ReleaseNotes { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this version is a pre-release.
            /// </summary>
            public bool PreRelease { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this release is the newest.
            /// </summary>
            public bool IsLatest { get; set; }
        }
    }
}
