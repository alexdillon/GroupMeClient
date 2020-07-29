using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GroupMeClient.Updates;

namespace GroupMeClient.ViewModels.Controls
{
    /// <summary>
    /// <see cref="ViewReleaseNotesControlViewModel"/> provides a ViewModel for the <see cref="Views.Controls.ViewReleaseNotesControl"/> control.
    /// </summary>
    public class ViewReleaseNotesControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewReleaseNotesControlViewModel"/> class.
        /// </summary>
        /// <param name="updateAssist">The update assistant that should be used.</param>
        public ViewReleaseNotesControlViewModel(UpdateAssist updateAssist)
        {
            this.UpdateAssist = updateAssist;
            this.Releases = new ObservableCollection<UpdateAssist.ReleaseInfo>();
            _ = this.LoadReleases();
        }

        /// <summary>
        /// Gets a collection of available GMDC Releases.
        /// </summary>
        public ObservableCollection<UpdateAssist.ReleaseInfo> Releases { get; }

        private UpdateAssist UpdateAssist { get; }

        private async Task LoadReleases()
        {
            var releases = await this.UpdateAssist.GetVersionsAsync();
            this.Releases.Clear();

            foreach (var release in releases)
            {
                this.Releases.Add(release);
            }
        }
    }
}
