using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;

namespace GroupMeClient.Plugins.Repositories
{
    /// <summary>
    /// <see cref="GitHubRepository"/> defines a plugin repository that is hosted on GitHub.
    /// </summary>
    public class GitHubRepository : Repository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubRepository"/> class.
        /// </summary>
        /// <param name="url">The URL to the GitHub page for the repo.</param>
        public GitHubRepository(string url)
            : base(url)
        {
        }

        /// <inheritdoc/>
        public override async Task<List<AvailablePlugin>> GetAvailablePlugins()
        {
            var urlParts = this.Url.Split('/');
            var gitUser = urlParts[3];
            var gitRepoName = urlParts[4];

            var httpClient = new HttpClient();
            var header = new ProductHeaderValue("GroupMeDesktopClient", ThisAssembly.SimpleVersion);
            var github = new GitHubClient(header);
            var latestRelease = await github.Repository.Release.GetLatest(gitUser, gitRepoName);

            var releasesFile = latestRelease.Assets.FirstOrDefault(a => a.Name == "RELEASES.txt");

            if (releasesFile != null)
            {
                var data = await httpClient.GetStringAsync(releasesFile.BrowserDownloadUrl);
                var assetUrl = releasesFile.BrowserDownloadUrl.Replace("RELEASES.txt", string.Empty);
                var availablePlugins = this.ParseReleases(data, assetUrl, $"{gitUser}/{gitRepoName}");
                return availablePlugins;
            }

            return new List<AvailablePlugin>();
        }

        private List<AvailablePlugin> ParseReleases(string releasesData, string assetUrl, string repoName)
        {
            var results = new List<AvailablePlugin>();

            var reader = new System.IO.StringReader(releasesData);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(new char[] { ' ' }, 3);
                var sha1Hash = parts[0];
                var fileName = parts[1];
                var displayName = parts[2];

                var versionString = fileName.Split('-').Last().Replace(".zip", string.Empty);
                var version = new Version(versionString);

                var downloadUrl = assetUrl + fileName;

                results.Add(new AvailablePlugin(displayName, downloadUrl, sha1Hash, version, repoName, this));
            }

            return results;
        }
    }
}
