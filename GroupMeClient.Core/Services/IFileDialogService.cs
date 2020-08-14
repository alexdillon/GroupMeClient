using System;
using System.Collections.Generic;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IFileDialogService"/> provides platform-abstracted ways of using file dialogs.
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// Displays an open file dialog.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="filters">The filters that can be opened.</param>
        /// <returns>The name of the opened file. If no file was selected, <see cref="string.Empty"/>.</returns>
        string ShowOpenFileDialog(string title, IEnumerable<FileFilter> filters);

        /// <summary>
        /// Displays an save file dialog.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="filters">The filters that can be saved.</param>
        /// <param name="defaultFileName">The default name for the saved file.</param>
        /// <returns>The name of the saved file. If no file was selected, <see cref="string.Empty"/>.</returns>
        string ShowSaveFileDialog(string title, IEnumerable<FileFilter> filters, string defaultFileName = "");
    }
}
