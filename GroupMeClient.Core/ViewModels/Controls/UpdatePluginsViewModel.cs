﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GroupMeClient.Core.Plugins;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="UpdatePluginsViewModel"/> provides a ViewModel for the <see cref="Views.Controls.UpdatePlugins"/> control.
    /// </summary>
    public class UpdatePluginsViewModel : ObservableObject
    {
        private bool isUpdatingPlugins;
        private Repository.AvailablePlugin selectedToUpdate;
        private PluginSettings.InstalledPlugin selectedToUninstall;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePluginsViewModel"/> class.
        /// </summary>
        public UpdatePluginsViewModel()
        {
            this.AvailableUpdates = new ObservableCollection<Repository.AvailablePlugin>();
            this.AvailablePlugins = new ObservableCollection<PluginSettings.InstalledPlugin>();

            this.UpdateSelectedCommand = new AsyncRelayCommand(this.UpdateSelectedPlugin);
            this.UpdateAllCommand = new AsyncRelayCommand(this.UpdateAllPlugins);
            this.UninstallPluginCommand = new RelayCommand(this.UninstallSelectedPlugin);

            this.PluginInstaller = Ioc.Default.GetService<PluginInstaller>();

            foreach (var plugin in this.PluginInstaller.InstalledPlugins)
            {
                this.AvailablePlugins.Add(plugin);
            }

            _ = this.UpdateAvailablePlugins();
        }

        /// <summary>
        /// Gets a list of updates that can be installed.
        /// </summary>
        public ObservableCollection<Repository.AvailablePlugin> AvailableUpdates { get; }

        /// <summary>
        /// Gets a list of <see cref="AvailablePlugins"/> from the <see cref="AddedRepos"/>.
        /// </summary>
        public ObservableCollection<PluginSettings.InstalledPlugin> AvailablePlugins { get; }

        /// <summary>
        /// Gets the command to update the selected <see cref="SelectedToUpdate"/> plugin.
        /// </summary>
        public ICommand UpdateSelectedCommand { get; }

        /// <summary>
        /// Gets the command to update all available plugins.
        /// </summary>s
        public ICommand UpdateAllCommand { get; }

        /// <summary>
        /// Gets the command to uninstall the <see cref="SelectedToUninstall"/> plugin.
        /// </summary>
        public ICommand UninstallPluginCommand { get; }

        /// <summary>
        /// Gets the command used to finish adding a new GitHub Repo.
        /// </summary>
        public ICommand FinishAddGitHubRepoCommand { get; }

        /// <summary>
        /// Gets the command to close and cancel the process of adding a new GitHub repo.
        /// </summary>
        public ICommand CloseGitHubRepoCommand { get; }

        /// <summary>
        /// Gets a value indicating whether the list of available online plugins is currently being updated.
        /// </summary>
        public bool IsUpdatingPlugins
        {
            get => this.isUpdatingPlugins;
            private set => this.SetProperty(ref this.isUpdatingPlugins, value);
        }

        /// <summary>
        /// Gets or sets the currently selected <see cref="Repository.AvailablePlugin"/> to update..
        /// </summary>
        public Repository.AvailablePlugin SelectedToUpdate
        {
            get => this.selectedToUpdate;
            set => this.SetProperty(ref this.selectedToUpdate, value);
        }

        /// <summary>
        /// Gets or sets the currently selected plugin to uninstall.
        /// </summary>
        public PluginSettings.InstalledPlugin SelectedToUninstall
        {
            get => this.selectedToUninstall;
            set => this.SetProperty(ref this.selectedToUninstall, value);
        }

        private PluginInstaller PluginInstaller { get; }

        private async Task UpdateAvailablePlugins()
        {
            this.AvailableUpdates.Clear();
            this.IsUpdatingPlugins = true;

            foreach (var repo in this.PluginInstaller.AddedRepositories)
            {
                foreach (var plugin in await repo.GetAvailablePlugins())
                {
                    var installed = this.PluginInstaller.InstalledPlugins
                        .FirstOrDefault(p => p.PluginName == plugin.Name &&
                                             p.RepositoryUrl == repo.Url);

                    // Ensure the plugin is currently installed
                    if (installed != null)
                    {
                        if (plugin.Version > installed.Version)
                        {
                            this.AvailableUpdates.Add(plugin);
                        }
                    }
                }
            }

            this.IsUpdatingPlugins = false;
        }

        private async Task UpdateSelectedPlugin()
        {
            if (this.SelectedToUpdate != null)
            {
                this.IsUpdatingPlugins = true;

                await this.PluginInstaller.UpdatePlugin(this.SelectedToUpdate);
                this.AvailableUpdates.Remove(this.SelectedToUpdate);

                this.IsUpdatingPlugins = false;
            }
        }

        private async Task UpdateAllPlugins()
        {
            this.IsUpdatingPlugins = true;

            foreach (var update in this.AvailableUpdates)
            {
                await this.PluginInstaller.UpdatePlugin(update);
            }

            this.AvailableUpdates.Clear();

            this.IsUpdatingPlugins = false;
        }

        private void UninstallSelectedPlugin()
        {
            if (this.SelectedToUninstall != null)
            {
                this.PluginInstaller.UninstallPlugin(this.SelectedToUninstall);
                this.AvailablePlugins.Remove(this.SelectedToUninstall);
            }
        }
    }
}
