namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IOperatingSystemUIService"/> provides a platform-independent interface for accessing operating
    /// system shell functionality.
    /// </summary>
    public interface IOperatingSystemUIService
    {
        /// <summary>
        /// Opens the user's default browser and navigates to a URL.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        void OpenWebBrowser(string url);

        /// <summary>
        /// Opens a file using the user's default application handler.
        /// </summary>
        /// <param name="filePath">The file to open.</param>
        void OpenFile(string filePath);

        /// <summary>
        /// Opens a file system browser shell window and highlights a specific file.
        /// </summary>
        /// <param name="filePath">The path of the file to highlight.</param>
        void ShowFileInExplorer(string filePath);
    }
}
