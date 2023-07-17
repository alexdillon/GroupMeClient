using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using GroupMeClient.AvaloniaUI;
using GroupMeClient.Core.Services;

namespace GroupMeClient.AvaloniaUI.Services
{
    /// <summary>
    /// <see cref="AvaloniaFileDialogService"/> provides file-dialog services as an implementation of <see cref="IFileDialogService"/> for
    /// the Windows/WPF platform.
    /// </summary>
    public class AvaloniaFileDialogService : IFileDialogService
    {
        /// <inheritdoc/>
        public string ShowOpenFileDialog(string title, IEnumerable<FileFilter> filters)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = title;
            openFileDialog.Filters = this.MakeAvaloniaFilters(filters);

            return Task.Run(async () => await openFileDialog.ShowAsync(Program.GMDCMainWindow)).Result.First();
        }

        /// <inheritdoc/>
        public string ShowSaveFileDialog(string title, IEnumerable<FileFilter> filters, string defaultFileName = "")
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = title;
            saveFileDialog.Filters = this.MakeAvaloniaFilters(filters);
            saveFileDialog.InitialFileName = defaultFileName;

            return Task.Run(async () => await saveFileDialog.ShowAsync(Program.GMDCMainWindow).ConfigureAwait(false)).Result;
        }

        private List<FileDialogFilter> MakeAvaloniaFilters(IEnumerable<FileFilter> filters)
        {
            var avaloniaFilters = new List<FileDialogFilter>();

            foreach (var filter in filters)
            {
                avaloniaFilters.Add(new FileDialogFilter() { Name = filter.Name, Extensions = filter.Extensions.ToList() });
            }

            return avaloniaFilters;
        }
    }
}
