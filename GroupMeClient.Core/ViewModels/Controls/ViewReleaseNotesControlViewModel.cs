using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GroupMeClient.Core.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GroupMeClient.Core.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ViewReleaseNotesControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ViewReleaseNotesControl"/> control.
    /// </summary>
    public class ViewReleaseNotesControlViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewReleaseNotesControlViewModel"/> class.
        /// </summary>
        /// <param name="updateService">The update service that should be used.</param>
        public ViewReleaseNotesControlViewModel(IUpdateService updateService)
        {
            this.UpdateService = updateService;
            this.Releases = new ObservableCollection<ReleaseInfo>();
            _ = this.LoadReleases();
        }

        /// <summary>
        /// Gets a collection of available GMDC Releases.
        /// </summary>
        public ObservableCollection<ReleaseInfo> Releases { get; }

        private IUpdateService UpdateService { get; }

        private async Task LoadReleases()
        {
            var releases = await this.UpdateService.GetVersionsAsync();
            this.Releases.Clear();

            foreach (var release in releases)
            {
                this.Releases.Add(release);
            }
        }
    }
}
