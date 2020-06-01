using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GroupMeClient.Plugins;
using GroupMeClient.Plugins.Repositories;
using GroupMeClient.Settings;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ManageReposViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ManageRepos"/> control.
    /// </summary>
    public class ManageReposViewModel : ViewModelBase
    {
        private bool isUpdatingPlugins;
        private bool showAddRepoTextbox;
        private string enteredRepoUrl;
        private Repository selectedRepo;
        private Repository.AvailablePlugin selectedPlugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageReposViewModel"/> class.
        /// </summary>
        public ManageReposViewModel()
        {
            this.AddedRepos = new ObservableCollection<Repository>();
            this.AvailablePlugins = new ObservableCollection<Repository.AvailablePlugin>();

            this.BeginAddingGitHubRepoCommand = new RelayCommand(this.BeginAddingGitHubRepo);
            this.RemoveSelectedRepoCommand = new RelayCommand(this.RemoveSelectedRepo);
            this.InstallPluginsCommand = new RelayCommand(async () => await this.InstallPlugins(), true);
            this.CloseGitHubRepoCommand = new RelayCommand(this.CancelAddingGitHubRepo);
            this.FinishAddGitHubRepoCommand = new RelayCommand(this.FinishAddingGitHubRepo);

            foreach (var repo in PluginInstaller.Instance.AddedRepositories)
            {
                this.AddedRepos.Add(repo);
            }

            _ = this.UpdateAvailablePlugins();
        }

        /// <summary>
        /// Gets a list of <see cref="Repository"/>s that have been added.
        /// </summary>
        public ObservableCollection<Repository> AddedRepos { get; }

        /// <summary>
        /// Gets a list of <see cref="AvailablePlugins"/> from the <see cref="AddedRepos"/>.
        /// </summary>
        public ObservableCollection<Repository.AvailablePlugin> AvailablePlugins { get; }

        /// <summary>
        /// Gets the command to begin adding a new GitHub Repo.
        /// </summary>
        public ICommand BeginAddingGitHubRepoCommand { get; }

        /// <summary>
        /// Gets the command to remove the selected repo from the repo list.
        /// </summary>s
        public ICommand RemoveSelectedRepoCommand { get; }

        /// <summary>
        /// Gets the command to install the selected plugin.
        /// </summary>
        public ICommand InstallPluginsCommand { get; }

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
            private set => this.Set(() => this.IsUpdatingPlugins, ref this.isUpdatingPlugins, value);
        }

        /// <summary>
        /// Gets a value indicating whether the textbox for entering the URL of a new repo should be shown.
        /// </summary>
        public bool ShowAddRepoTextbox
        {
            get => this.showAddRepoTextbox;
            private set => this.Set(() => this.ShowAddRepoTextbox, ref this.showAddRepoTextbox, value);
        }

        /// <summary>
        /// Gets or sets a value containing the entered repository URL.
        /// </summary>
        public string EnteredRepoUrl
        {
            get => this.enteredRepoUrl;
            set => this.Set(() => this.EnteredRepoUrl, ref this.enteredRepoUrl, value);
        }

        /// <summary>
        /// Gets or sets the currently selected repo.
        /// </summary>
        public Repository SelectedRepo
        {
            get => this.selectedRepo;
            set => this.Set(() => this.SelectedRepo, ref this.selectedRepo, value);
        }

        /// <summary>
        /// Gets or sets the currently selected plugin.
        /// </summary>
        public Repository.AvailablePlugin SelectedPlugin
        {
            get => this.selectedPlugin;
            set => this.Set(() => this.SelectedPlugin, ref this.selectedPlugin, value);
        }

        private async Task UpdateAvailablePlugins()
        {
            this.AvailablePlugins.Clear();
            this.IsUpdatingPlugins = true;

            foreach (var repo in this.AddedRepos)
            {
                foreach (var plugin in await repo.GetAvailablePlugins())
                {
                    var installed = PluginInstaller.Instance.InstalledPlugins
                        .FirstOrDefault(p => p.PluginName == plugin.Name &&
                                             p.RepositoryUrl == repo.Url);

                    // Only show plugins for installation that are not already installed
                    if (installed == null)
                    {
                        this.AvailablePlugins.Add(plugin);
                    }
                }
            }

            this.IsUpdatingPlugins = false;
        }

        private void BeginAddingGitHubRepo()
        {
            this.ShowAddRepoTextbox = true;
        }

        private void RemoveSelectedRepo()
        {
            if (this.SelectedRepo != null)
            {
                var settingsRepo = PluginInstaller.Instance.AddedRepositories.FirstOrDefault(r => r.Url == this.SelectedRepo.Url);
                PluginInstaller.Instance.RemoveRepository(settingsRepo);
                this.AddedRepos.Remove(this.SelectedRepo);
                this.SelectedRepo = null;
                _ = this.UpdateAvailablePlugins();
            }
        }

        private async Task InstallPlugins()
        {
            if (this.SelectedPlugin != null)
            {
                this.IsUpdatingPlugins = true;
                await PluginInstaller.Instance.InstallPlugin(this.SelectedPlugin);
                await this.UpdateAvailablePlugins();
                this.IsUpdatingPlugins = false;
            }
        }

        private void FinishAddingGitHubRepo()
        {
            if (!string.IsNullOrEmpty(this.EnteredRepoUrl))
            {
                var existingRepoSource = this.AddedRepos.FirstOrDefault(r => r.Url == this.EnteredRepoUrl);
                if (existingRepoSource == null)
                {
                    var repo = new GitHubRepository(this.EnteredRepoUrl);
                    PluginInstaller.Instance.AddRepository(repo);
                    this.AddedRepos.Add(repo);
                    _ = this.UpdateAvailablePlugins();
                }
            }

            this.CancelAddingGitHubRepo();
        }

        private void CancelAddingGitHubRepo()
        {
            this.EnteredRepoUrl = string.Empty;
            this.ShowAddRepoTextbox = false;
        }
    }
}
