using System.Threading.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;

namespace GroupMeClient.Notifications
{
    /// <summary>
    /// <see cref="INotificationSink"/> provides an interface to allow subscribing to GroupMe events.
    /// By registering an <see cref="INotificationSink"/> with <see cref="NotificationRouter"/>, push events can be received.
    /// </summary>
    public interface INotificationSink
    {
        /// <summary>
        /// Executed when a new message is posted to a <see cref="Group"/>.
        /// </summary>
        /// <param name="notification">The raw GroupMe notification.</param>
        /// <param name="container">The Group that was modified"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container);

        /// <summary>
        /// Executed when a new message is posted to a <see cref="Chat"/>.
        /// </summary>
        /// <param name="notification">The raw GroupMe notification.</param>
        /// <param name="container">The Chat that was modified"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container);

        /// <summary>
        /// Exectuted when a <see cref="Message"/> is updated.
        /// This can occur when a <see cref="Message"/> is 'Liked'.
        /// </summary>
        /// <param name="message">The message that was updated.</param>
        /// <param name="alert">The raw GroupMe Notification.</param>
        /// <param name="container">The Group or Chat containing the updated message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task MessageUpdated(Message message, string alert, IMessageContainer container);

        /// <summary>
        /// Executed periodically to indicate the GroupMe Push Connection is alive.
        /// </summary>
        void HeartbeatReceived();

        /// <summary>
        /// Executed when an observer is registered with <see cref="NotificationRouter"/>.
        /// Observers gain access the the <see cref="PushClient"/> for subscribing and unsubscribing to channels.
        /// The <see cref="GroupMeClientApi.GroupMeClient"/> is also returned for API operations.
        /// </summary>
        /// <param name="pushClient">The push client in use.</param>
        /// <param name="client">The API client in use.</param>
        void RegisterPushSubscriptions(PushClient pushClient, GroupMeClientApi.GroupMeClient client);
    }
}
