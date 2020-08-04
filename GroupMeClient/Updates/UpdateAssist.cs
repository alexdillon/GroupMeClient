using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GroupMeClient.Core.Services;
using Octokit;
using Squirrel;

namespace GroupMeClient.Wpf.Updates
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
        public TaskCompletionSource<bool?> UpdateMonitor { get; private set; }

        private UpdateManager UpdateManager { get; set; }

        private bool HasAlreadyUpdated { get; set; } = false;

        private Timer UpdateTimer { get; set; }

        private Semaphore UpdateSem { get; } = new Semaphore(1, 1);

        private string GMDCRepoUser => "alexdillon";

        private string GMDCRepoName => "GroupMeClient";

        private string GMDCGitHubRepoUrl => $"https://github.com/{this.GMDCRepoUser}/{this.GMDCRepoName}";

        /// <inheritdoc/>
        public async Task<IEnumerable<ReleaseInfo>> GetVersionsAsync()
        {
            var header = new ProductHeaderValue("GroupMeDesktopClient", GroupMeClient.Core.GlobalAssemblyInfo.SimpleVersion);
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

        /// <inheritdoc/>
        public void StartUpdateTimer(TimeSpan interval)
        {
            this.UpdateTimer = new Timer(
                (l) => this.BeginCheckForUpdates(),
                null,
                TimeSpan.Zero,
                interval);
        }

        /// <inheritdoc/>
        public void CancelUpdateTimer()
        {
            this.UpdateTimer = null;
        }

        /// <inheritdoc/>
        public void BeginCheckForUpdates()
        {
            // Ensure exclusive updater access.
            this.UpdateSem.WaitOne();

            this.UpdateMonitor = new TaskCompletionSource<bool?>();
            if (this.IsInstalled && !this.HasAlreadyUpdated)
            {
                // Only install updates if this is running as an installed copy
                // (i.e., not a portable installation or running under Visual Studio)
                this.CanShutdown = false;
                UpdateManager.GitHubUpdateManager(this.GMDCGitHubRepoUrl).ContinueWith(this.LoadComplete);
            }
            else
            {
                this.SetUpdateStatus(updateSucceeded: null);
            }
        }

        private void LoadComplete(Task<UpdateManager> manager)
        {
            try
            {
                this.UpdateManager = manager.Result;
                this.UpdateManager.UpdateApp().ContinueWith(this.UpdateCompleted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failure while checking for updates. Message: {ex.Message}");
                this.SetUpdateStatus(updateSucceeded: null);
            }
        }

        private void UpdateCompleted(Task<ReleaseEntry> releaseEntry)
        {
            bool? updateSucceeded = false;

            try
            {
                var result = releaseEntry.Result;
                updateSucceeded = result != null;
            }
            catch (Exception)
            {
                updateSucceeded = null;
            }
            finally
            {
                this.UpdateManager.Dispose();
                this.SetUpdateStatus(updateSucceeded);
            }
        }

        private void SetUpdateStatus(bool? updateSucceeded)
        {
            this.CanShutdown = true;
            this.UpdateMonitor.SetResult(updateSucceeded);

            if (updateSucceeded == true)
            {
                // No need to re-check for updates in the future if they are already applied and waiting for a reboot
                this.HasAlreadyUpdated = true;
            }

            this.UpdateSem.Release();
        }
    }
}
