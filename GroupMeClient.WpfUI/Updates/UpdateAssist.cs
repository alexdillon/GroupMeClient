using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GroupMeClient.Core.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Octokit;

namespace GroupMeClient.WpfUI.Updates
{
    /// <summary>
    /// <see cref="UpdateAssist"/> provides automatic upgrade functionality with Squirrel.
    /// </summary>
    /// <remarks>
    /// Manually running Squirrel adapted from https://gist.github.com/anaisbetts/9015f33a95c523a133bb624e3c77213d.
    /// </remarks>
    public class UpdateAssist : IUpdateService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAssist"/> class.
        /// </summary>
        public UpdateAssist()
        {
            var assembly = Assembly.GetEntryAssembly();

            this.UpdateDotExe = Path.Combine(
              Path.GetDirectoryName(assembly.Location),
              "..",
              "Update.exe");

            var isVsDebug = this.UpdateDotExe.Contains(Path.Combine("Debug", "..", "Update.exe"));

            this.IsInstalled = File.Exists(this.UpdateDotExe) && !isVsDebug;
        }

        /// <inheritdoc/>
        public IObservable<bool> CanShutdown => this.CanShutdownSource;

       /// <inheritdoc/>
        public bool IsInstalled { get; private set; }

        private bool HasAlreadyUpdated { get; set; } = false;

        private Timer UpdateTimer { get; set; }

        private Semaphore UpdateSem { get; } = new Semaphore(1, 1);

        private BehaviorSubject<bool> CanShutdownSource { get; } = new BehaviorSubject<bool>(true);

        private string GMDCRepoUser => "alexdillon";

        private string GMDCRepoName => "GroupMeClient";

        private string GMDCGitHubRepoUrl => $"https://github.com/{this.GMDCRepoUser}/{this.GMDCRepoName}";

        private string UpdateDotExe { get; }

        /// <inheritdoc/>
        public async Task<IEnumerable<ReleaseInfo>> GetVersionsAsync()
        {
            var github = this.GetGitHubClient();
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
                (l) => _ = this.CheckForUpdatesAsync(),
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
        public async Task<bool?> CheckForUpdatesAsync()
        {
            // Ensure exclusive updater access.
            this.UpdateSem.WaitOne();

            bool? updateSucceeded = null;
            try
            {
                if (this.IsInstalled && !this.HasAlreadyUpdated)
                {
                    // Only install updates if this is running as an installed copy
                    // (i.e., not a portable installation or running under Visual Studio)
                    this.CanShutdownSource.OnNext(false);
                    updateSucceeded = await this.CheckForSquirrelUpdates();
                }
                else
                {
                    // Update service not available for non-installed apps
                    updateSucceeded = null;
                }
            }
            catch (Exception)
            {
                // Update service not available due to unknown error
                updateSucceeded = null;
            }
            finally
            {
                this.CanShutdownSource.OnNext(true);
                this.UpdateSem.Release();
            }

            if (updateSucceeded == true)
            {
                // No need to re-check for updates in the future if they are already applied and waiting for a reboot
                this.HasAlreadyUpdated = true;

                // Show the update notification in the UI
                var uiDispatchService = Ioc.Default.GetService<IUserInterfaceDispatchService>();
                uiDispatchService.Invoke(() =>
                {
                    WeakReferenceMessenger.Default.Send(new Core.Messaging.RebootRequestMessage($"Reboot to Finish Updating GMDC"));
                });
            }

            // Return the final update status.
            // True - updated. False - no updates needed. Null - Could not update.
            return updateSucceeded;
        }

        private GitHubClient GetGitHubClient()
        {
            var header = new ProductHeaderValue("GroupMeDesktopClient", Core.GlobalAssemblyInfo.SimpleVersion);
            return new GitHubClient(header);
        }

        private async Task<Release> GetLatestVersionAsync()
        {
            var github = this.GetGitHubClient();
            var releases = await github.Repository.Release.GetAll(this.GMDCRepoUser, this.GMDCRepoName);
            return releases.FirstOrDefault(r => r.Prerelease == false);
        }

        private async Task<bool?> CheckForSquirrelUpdates()
        {
            try
            {
                var newestRelease = await this.GetLatestVersionAsync();
                if (newestRelease == null)
                {
                    // An error occured retreiving the newest release
                    return null;
                }

                var releasesAsset = newestRelease.Assets.FirstOrDefault(r => r.Name.ToLower() == "releases");
                if (releasesAsset == null)
                {
                    // An error occured retreiving the RELEASES file
                    return null;
                }

                var (exitCode, output) = await this.RunSquirrel($"--update {releasesAsset.BrowserDownloadUrl.Replace("/RELEASES", string.Empty)}");
                if (exitCode != 0)
                {
                    Debug.WriteLine($"Failed to update! {output}");
                    return null;
                }
                else
                {
                    // Success. Figure out if this means up-to-date, or successfully updated....
                    var newestReleaseVersion = Version.Parse(newestRelease.TagName.Replace("v", string.Empty));
                    var currentVersion = Version.Parse(Core.GlobalAssemblyInfo.SimpleVersion);

                    if (newestReleaseVersion > currentVersion)
                    {
                        // Claim we updated
                        return true;
                    }
                    else
                    {
                        // Claim we are already up-to-date
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, $"Failed to invoke Update.exe");
                return null;
            }
        }

        private Task<(int exitCode, string output)> RunSquirrel(string args)
        {
            return this.InvokeProcessAsync(this.UpdateDotExe, args, CancellationToken.None, Path.GetDirectoryName(this.UpdateDotExe));
        }

        private Task<(int exitCode, string output)> InvokeProcessAsync(string fileName, string arguments, CancellationToken ct, string workingDirectory = "")
        {
            var psi = new ProcessStartInfo(fileName, arguments)
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory,
            };

            return this.InvokeProcessAsync(psi, ct);
        }

        private async Task<(int exitCode, string output)> InvokeProcessAsync(ProcessStartInfo psi, CancellationToken ct)
        {
            var pi = Process.Start(psi);
            await Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    if (pi.WaitForExit(2000))
                    {
                        return;
                    }
                }

                if (ct.IsCancellationRequested)
                {
                    pi.Kill();
                    ct.ThrowIfCancellationRequested();
                }
            });

            string textResult = await pi.StandardOutput.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(textResult) || pi.ExitCode != 0)
            {
                textResult = (textResult ?? string.Empty) + "\n" + await pi.StandardError.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(textResult))
                {
                    textResult = string.Empty;
                }
            }

            return (pi.ExitCode, textResult.Trim());
        }
    }
}
