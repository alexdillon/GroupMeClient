using System;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IWindowService"/> provides platform agnostic support for dispaying standalone and modal windows.
    /// </summary>
    public interface IWindowService
    {
        /// <summary>
        /// Displays a platform specific window.
        /// </summary>
        /// <param name="windowParams">The parameters for the window to show.</param>
        void ShowWindow(WindowParams windowParams);

        /// <summary>
        /// Displays a platform specific dialog window.
        /// </summary>
        /// <param name="windowParams">The parameters for the window to show.</param>
        void ShowDialog(WindowParams windowParams);
    }
}
