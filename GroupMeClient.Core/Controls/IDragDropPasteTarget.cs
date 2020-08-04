namespace GroupMeClient.Core.Controls
{
    /// <summary>
    /// <see cref="IDragDropPasteTarget"/> enables receiving updates when data is dropped onto a control.
    /// </summary>
    /// <remarks>
    /// Adapted from https://stackoverflow.com/a/37608994.
    /// </remarks>
    public interface IDragDropPasteTarget
    {
        /// <summary>
        /// Executed when a file has been dragged onto the target.
        /// </summary>
        /// <param name="filepaths">The file name(s) dropped.</param>
        void OnFileDrop(string[] filepaths);

        /// <summary>
        /// Executed when an image has been dragged onto the target.
        /// </summary>
        /// <param name="image">The raw image data that was dropped.</param>
        void OnImageDrop(byte[] image);
    }
}
