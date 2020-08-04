using System;
using System.IO;

namespace GroupMeClient.Core.Utilities
{
    /// <summary>
    /// <see cref="TempFileUtils"/> provides support for working with temporary storage in GroupMe Desktop Client.
    /// </summary>
    public class TempFileUtils
    {
        /// <summary>
        /// Gets the default temporary folder in which data should be stored.
        /// </summary>
        public static string GroupMeDesktopClientTempFolder => Path.Combine(Path.GetTempPath(), "GroupMeDesktopClient");

        /// <summary>
        /// Gets a temporary file name that resides within the GroupMe Desktop Client temp directory.
        /// Entries in this folder are automatically cleaned up by the application.
        /// </summary>
        /// <param name="originalFileName">A filename to base the temporary file extension on.</param>
        /// <returns>A temporary file name. The temp file is not created on disk.</returns>
        public static string GetTempFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var tempFileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
            var tempFile = Path.Combine(Path.GetTempPath(), "GroupMeDesktopClient", tempFileName + extension);

            return tempFile;
        }

        /// <summary>
        /// Initializes the temporary storage repository for use. Any existing entries in the temp
        /// folder are deleted. If the temp folder does not exist, it will be created.
        /// </summary>
        public static void InitializeTempStorage()
        {
            if (Directory.Exists(GroupMeDesktopClientTempFolder))
            {
                foreach (var file in Directory.EnumerateFiles(GroupMeDesktopClientTempFolder))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            Directory.CreateDirectory(GroupMeDesktopClientTempFolder);
        }
    }
}
