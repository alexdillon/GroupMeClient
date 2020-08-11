using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace GroupMeClient.Core.Plugins
{
    /// <summary>
    /// <see cref="PluginInstaller"/> provides support for downloading and installing <see cref="Repository.AvailablePlugin"/>s from a <see cref="Repository"/>.
    /// </summary>
    public sealed class PluginInstaller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginInstaller"/> class.
        /// </summary>
        /// <param name="pluginRoot">The root path for the plugin library.</param>
        public PluginInstaller(string pluginRoot)
        {
            this.PluginRoot = pluginRoot;
            this.LoadPluginSettings();
        }

        /// <summary>
        /// Gets the suffix that is applied to the end of a filename to indicate that it is being staged for installation.
        /// </summary>
        public static string StagingSuffix => "--staging";

        /// <summary>
        /// Gets the name of the subdirectory that houses plugin packages.
        /// </summary>
        public static string PackagesDirectory => "packages";

        /// <summary>
        /// Gets a listing of the known <see cref="Repository"/>s that have been added and are available.
        /// </summary>
        public IReadOnlyCollection<Repository> AddedRepositories => this.PluginSettings.Repositories;

        /// <summary>
        /// Gets a listing of all the currently installed <see cref="PluginSettings.InstalledPlugin"/>s.
        /// </summary>
        public IReadOnlyCollection<PluginSettings.InstalledPlugin> InstalledPlugins => this.PluginSettings.InstalledPlugins;

        private string PluginSettingsFile => Path.Combine(this.PluginRoot, "plugins.json");

        private string PluginRoot { get; }

        private PluginSettings PluginSettings { get; } = new PluginSettings();

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
                Version = plugin.Version,
            };

            await this.UnpackAndCopyPackage(installedPlugin.InstallationGuid, string.Empty, plugin.BinaryUrl);

            this.PluginSettings.InstalledPlugins.Add(installedPlugin);
            this.SavePluginSettings();
            Messenger.Default.Send(new GroupMeClient.Core.Messaging.RebootRequestMessage($"Reboot to Finish Installing {plugin.Name}"));
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

            installedPlugin.Version = plugin.Version;

            await this.UnpackAndCopyPackage(installedPlugin.InstallationGuid, StagingSuffix, plugin.BinaryUrl);

            this.SavePluginSettings();
            Messenger.Default.Send(new Core.Messaging.RebootRequestMessage($"Reboot to Finish Updating {plugin.Name}"));
        }

        /// <summary>
        /// Uninstalls the specified plugin.
        /// </summary>
        /// <param name="plugin">The plugin to uninstall.</param>
        public void UninstallPlugin(PluginSettings.InstalledPlugin plugin)
        {
            var stagingDeleteFilename = Path.Combine(this.PluginRoot, PackagesDirectory, $"{plugin.InstallationGuid}{StagingSuffix}.rm");
            File.WriteAllBytes(stagingDeleteFilename, new byte[] { });

            this.PluginSettings.InstalledPlugins.Remove(plugin);
            this.SavePluginSettings();
            Messenger.Default.Send(new Core.Messaging.RebootRequestMessage($"Reboot to Finish Uninstalling {plugin.PluginName}"));
        }

        private async Task UnpackAndCopyPackage(string guid, string suffix, string binaryUrl)
        {
            var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(binaryUrl);
            using (var stream = new MemoryStream(data))
            {
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var targetDirectory = Path.Combine(this.PluginRoot, PackagesDirectory, $"{guid}{suffix}");
                    zip.ExtractToDirectory(targetDirectory);
                }
            }
        }

        private void LoadPluginSettings()
        {
            if (File.Exists(this.PluginSettingsFile))
            {
                string json = File.ReadAllText(this.PluginSettingsFile);

                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                };
                settings.Converters.Add(new Newtonsoft.Json.Converters.VersionConverter());

                JsonConvert.PopulateObject(json, this.PluginSettings, settings);
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

                serializer.Converters.Add(new Newtonsoft.Json.Converters.VersionConverter());

                serializer.Serialize(file, this.PluginSettings);
            }
        }
    }
}
