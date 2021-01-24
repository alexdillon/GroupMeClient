using System;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IMessageRendererService"/> provides a service for displaying platform dependent Message Boxes.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Displays a message box.
        /// </summary>
        /// <param name="parameters">The parameters for the message box to display.</param>
        void ShowMessageBox(MessageBoxParams parameters);
    }
}
