using System;
using System.Collections.Generic;
using System.Linq;
using GroupMeClient.Core.Services;
using Microsoft.Win32;

namespace GroupMeClient.WpfUI.Services
{
    /// <summary>
    /// <see cref="WpfFileDialogService"/> provides file-dialog services as an implementation of <see cref="IFileDialogService"/> for
    /// the Windows/WPF platform.
    /// </summary>
    public class WpfFileDialogService : IFileDialogService
    {
        /// <inheritdoc/>
        public string ShowOpenFileDialog(string title, IEnumerable<FileFilter> filters)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = this.MakeWin32Filters(filters),
                Title = title,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public string ShowSaveFileDialog(string title, IEnumerable<FileFilter> filters, string defaultFileName = "")
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = this.MakeWin32Filters(filters),
                Title = title,
                FileName = defaultFileName,
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        private string MakeWin32Filters(IEnumerable<FileFilter> filters)
        {
            var win32Filters = new List<string>();

            foreach (var filter in filters)
            {
                var extensionsFilter = string.Join(";", filter.Extensions.Select(x => "*" + x));
                var extensionsDisplay = string.Join(", ", filter.Extensions.Select(x => "*" + x));
                win32Filters.Add($"{filter.Name} ({extensionsDisplay})|{extensionsFilter}");
            }

            return string.Join("|", win32Filters);
        }
    }
}
