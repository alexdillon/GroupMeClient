using System.Threading.Tasks;
using GroupMeClientApi;
using GroupMeClientApi.Models;

namespace GroupMeClientPlugin.GroupChat
{
    /// <summary>
    /// <see cref="IGroupChatPlugin"/> defines a plugin that can provide additional features for
    /// a Group or Chat.
    /// </summary>
    public interface IGroupChatPlugin
    {
        /// <summary>
        /// Gets the name of this plugin, which will be displayed
        /// in the Group Options Menu in the Desktop Client.
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Runs the plugin after a user has selected it from the Group Options Menu.
        /// </summary>
        /// <param name="groupOrChat">The Group or Chat selected by the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task Activated(IMessageContainer groupOrChat);
    }
}
