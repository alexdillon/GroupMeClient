using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;

namespace GroupMeClient.Core.Services
{
    /// <summary>
    /// <see cref="IMessageRendererService"/> provides an interface for a service used to render <see cref="Message"/>s into images.
    /// </summary>
    public interface IMessageRendererService
    {
        /// <summary>
        /// Creates a image representation of a <see cref="Message"/> would be rendered in the user interface.
        /// </summary>
        /// <param name="message">The message to render.</param>
        /// <param name="displayedMessage">An already displayed version of the message to use for pre-loading attachments.</param>
        /// <returns>A raw collection of bytes containing the image encoded in PNG format.</returns>
        byte[] RenderMessageToPngImage(Message message, MessageControlViewModelBase displayedMessage);
    }
}
