using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;
using Windows.Networking.NetworkOperators;

namespace GroupMeClient.Plugins
{
    /// <summary>
    /// <see cref="PluginInstaller"/> provides support for downloading and installing <see cref="Repository.AvailablePlugin"/>s from a <see cref="Repository"/>.
    /// </summary>
    public sealed class PluginInstaller
    {
        private static readonly Lazy<PluginInstaller> LazyPluginInstaller = new Lazy<PluginInstaller>(() => new PluginInstaller());
        private static string pluginRoot;

        private PluginInstaller()
        {
            this.LoadPluginSettings();
        }

        /// <summary>
        /// Gets the instance of the <see cref="PluginInstaller"/> for the current application.
        /// </summary>
        public static PluginInstaller Instance => LazyPluginInstaller.Value;

        /// <summary>
        /// Gets the suffix that is applied to the end of a filename to indicate that it is being staged for installation.
        /// </summary>
        public static string StagingSuffix => "--staging";

        /// <summary>
        /// Gets a listing of the known <see cref="Repository"/>s that have been added and are available.
        /// </summary>
        public IReadOnlyCollection<Repository> AddedRepositories => this.PluginSettings.Repositories;

        /// <summary>
        /// Gets a listing of all the currently installed <see cref="PluginSettings.InstalledPlugin"/>s.
        /// </summary>
        public IReadOnlyCollection<PluginSettings.InstalledPlugin> InstalledPlugins => this.PluginSettings.InstalledPlugins;

        private string PluginSettingsFile => Path.Combine(pluginRoot, "plugins.json");

        private PluginSettings PluginSettings { get; } = new PluginSettings();

        /// <summary>
        /// Configures the <see cref="PluginInstaller"/> for operation. This method must be called
        /// before any <see cref="PluginInstaller"/> services can be accessed.
        /// </summary>
        /// <param name="pluginRoot">The root path of the Plugins folder.</param>
        public static void SetupPluginInstaller(string pluginRoot)
        {
            PluginInstaller.pluginRoot = pluginRoot;
        }

        /// <summary>
        /// Adds a new <see cref="Repository"/> to the list of <see cref="AddedRepositories"/>.
        /// </summary>
        /// <param name="repo">The <see cref="Repository"/> to add.</param>
        public void AddRepository(Repository repo)
        {
            this.PluginSettings.Repositories.Add(repo);
            this.SavePluginSettings();
        }

        /// <summary>
        /// Removes a <see cref="Repository"/> from the list of <see cref="AddedRepositories"/>.
        /// </summary>
        /// <param name="repo">The <see cref="Repository"/> to add.</param>
        public void RemoveRepository(Repository repo)
        {
            this.PluginSettings.Repositories.Remove(repo);
            this.SavePluginSettings();
        }

        /// <summary>
        /// Installs a new <see cref="Repository.AvailablePlugin"/>.
        /// </summary>
        /// <param name="plugin">The <see cref="Repository.AvailablePlugin"/> to install.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task InstallPlugin(Repository.AvailablePlugin plugin)
        {
            var installedPlugin = new PluginSettings.InstalledPlugin()
            {
                RepositoryUrl = plugin.ParentRepo.Url,
                InstallationGuid = Guid.NewGuid().ToString(),
                PluginName = plugin.Name,
            };

            await this.UnpackAndCopyPackage(installedPlugin.InstallationGuid, string.Empty, plugin.BinaryUrl);

            this.PluginSettings.InstalledPlugins.Add(installedPlugin);
            this.SavePluginSettings();
        }

        /// <summary>
        /// Updates a <see cref="Repository.AvailablePlugin"/>.
        /// </summary>
        /// <param name="plugin">The <see cref="Repository.AvailablePlugin"/> to install.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task UpdatePlugin(Repository.AvailablePlugin plugin)
        {
            var installedPlugin = this.PluginSettings.InstalledPlugins.First(p => p.RepositoryUrl == plugin.ParentRepo.Url &&
                                                                                  p.PluginName == plugin.Name);

            await this.UnpackAndCopyPackage(installedPlugin.InstallationGuid, StagingSuffix, plugin.BinaryUrl);

            this.PluginSettings.InstalledPlugins.Add(installedPlugin);
            this.SavePluginSettings();
        }

        private async Task UnpackAndCopyPackage(string guid, string suffix, string binaryUrl)
        {
            var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(binaryUrl);
            using (var stream = new MemoryStream(data))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (Path.GetExtension(entry.Name).ToLower() == ".dll")
                        {
                            entry.ExtractToFile(Path.Combine(pluginRoot, $"{guid}{suffix}.dll"));
                        }
                        else
                        {
                            entry.ExtractToFile(Path.Combine(pluginRoot, entry.Name));
                        }
                    }
                }
            }
        }

        private void LoadPluginSettings()
        {
            if (File.Exists(this.PluginSettingsFile))
            {
                string json = File.ReadAllText(this.PluginSettingsFile);
                JsonConvert.PopulateObject(json, this.PluginSettings, new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                });
            }
        }

        private void SavePluginSettings()
        {
            using (StreamWriter file = File.CreateText(this.PluginSettingsFile))
            {
                var serializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                serializer.Serialize(file, this.PluginSettings);
            }
        }
    }
}
