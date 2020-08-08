using System;
using System.Linq;
using System.Threading.Tasks;
using GroupMeClientApi.Models;
using GroupMeClientApi.Models.Attachments;
using GroupMeClientApi.Push;
using GroupMeClientApi.Push.Notifications;
using GroupMeClientPlugin.Notifications;
using GroupMeClientPlugin.Notifications.Display;
using Octokit;

namespace GroupMeClient.Core.Notifications.Display
{
    /// <summary>
    /// <see cref="PopupNotificationProvider"/> provides an observer to display notifications from the <see cref="NotificationRouter"/> visually.
    /// </summary>
    public class PopupNotificationProvider : INotificationSink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PopupNotificationProvider"/> class.
        /// </summary>
        public PopupNotificationProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupNotificationProvider"/> class.
        /// </summary>
        /// <param name="sink">The raw notification provider to use.</param>
        private PopupNotificationProvider(IPopupNotificationSink sink)
        {
            this.PopupNotificationSink = sink;
        }

        /// <summary>
        /// Gets or sets the <see cref="GroupMeClientApi.GroupMeClient"/> that
        /// is provided from the <see cref="Core.Notifications.NotificationRouter"/>.
        /// </summary>
        protected GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private IPopupNotificationSink PopupNotificationSink { get; }

        /// <summary>
        /// Creates a <see cref="PopupNotificationProvider"/> that does nothing with notifications.
        /// </summary>
        /// <returns>A PopupNotificationProvider.</returns>
        public static PopupNotificationProvider CreateDoNothingProvider()
        {
            return new PopupNotificationProvider(new DummyVisualSink());
        }

        /// <inheritdoc/>
        async Task INotificationSink.ChatUpdated(DirectMessageCreateNotification notification, IMessageContainer container)
        {
            if (!string.IsNullOrEmpty(notification.Alert) &&
                !this.DidISendIt(notification.Message) &&
                !this.IsGroupMuted(container))
            {
                var image = notification.Message.Attachments.FirstOrDefault(a => a is ImageAttachment);

                if (image != null)
                {
                    await this.PopupNotificationSink.ShowLikableImageMessage(
                        container.Name,
                        this.RemoveUnprintableCharacters(notification.Alert),
                        notification.Message.AvatarUrl,
                        (notification.Message as IAvatarSource).IsRoundedAvatar,
                        (image as ImageAttachment).Url,
                        container.Id,
                        notification.Message.Id);
                }
                else
                {
                    await this.PopupNotificationSink.ShowLikableMessage(
                       container.Name,
                       this.RemoveUnprintableCharacters(notification.Alert),
                       notification.Message.AvatarUrl,
                       (notification.Message as IAvatarSource).IsRoundedAvatar,
                       container.Id,
                       notification.Message.Id);
                }
            }
        }

        /// <inheritdoc/>
        async Task INotificationSink.GroupUpdated(LineMessageCreateNotification notification, IMessageContainer container)
        {
            if (!string.IsNullOrEmpty(notification.Alert) &&
                !this.DidISendIt(notification.Message) &&
                !this.IsGroupMuted(container))
            {
                var image = notification.Message.Attachments.FirstOrDefault(a => a is ImageAttachment);

                if (image != null)
                {
                    await this.PopupNotificationSink.ShowLikableImageMessage(
                        container.Name,
                        this.RemoveUnprintableCharacters(notification.Alert),
                        container.ImageOrAvatarUrl,
                        container.IsRoundedAvatar,
                        (image as ImageAttachment).Url,
                        container.Id,
                        notification.Message.Id);
                }
                else
                {
                    await this.PopupNotificationSink.ShowLikableMessage(
                        container.Name,
                        this.RemoveUnprintableCharacters(notification.Alert),
                        container.ImageOrAvatarUrl,
                        container.IsRoundedAvatar,
                        container.Id,
                        notification.Message.Id);
                }
            }
        }

        /// <inheritdoc/>
        async Task INotificationSink.MessageUpdated(Message message, string alert, IMessageContainer container)
        {
            if (!string.IsNullOrEmpty(alert) &&
                !this.IsGroupMuted(container))
            {
                await this.PopupNotificationSink.ShowNotification(
                    container.Name,
                    this.RemoveUnprintableCharacters(alert),
                    container.ImageOrAvatarUrl,
                    container.IsRoundedAvatar,
                    container.Id);
            }
        }

        /// <inheritdoc/>
        void INotificationSink.HeartbeatReceived()
        {
        }

        /// <inheritdoc/>
        void INotificationSink.RegisterPushSubscriptions(PushClient pushClient, GroupMeClientApi.GroupMeClient client)
        {
            this.GroupMeClient = client;
            this.PopupNotificationSink.RegisterClient(client);
        }

        /// <summary>
        /// Creates a <see cref="PopupNotificationProvider"/> for a specific implementation.
        /// </summary>
        /// <param name="sink">The raw notification implementation to use.</param>
        /// <returns>A PopupNotificationProvider.</returns>
        protected static PopupNotificationProvider CreateProvider(IPopupNotificationSink sink)
        {
            return new PopupNotificationProvider(sink);
        }

        private bool DidISendIt(Message message)
        {
            var me = this.GroupMeClient.WhoAmI();
            return message.UserId == me.Id;
        }

        private string RemoveUnprintableCharacters(string message)
        {
            return message.Replace("\uE008 ", string.Empty);
        }

        private bool IsGroupMuted(IMessageContainer messageContainer)
        {
            if (messageContainer is Group group)
            {
                var me = this.GroupMeClient.WhoAmI();
                foreach (var member in group.Members)
                {
                    if (member.UserId == me.UserId)
                    {
                        return member.Muted;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// <see cref="DummyVisualSink"/> provides a placeholder notification sink that does absolutely nothing.
        /// </summary>
        private class DummyVisualSink : IPopupNotificationSink
        {
            public void RegisterClient(GroupMeClientApi.GroupMeClient client)
            {
            }

            public Task ShowLikableImageMessage(string title, string body, string avatarUrl, bool roundedAvatar, string imageUrl, string containerId, string messageId)
            {
                return Task.CompletedTask;
            }

            public Task ShowLikableMessage(string title, string body, string avatarUrl, bool roundedAvatar, string containerId, string messageId)
            {
                return Task.CompletedTask;
            }

            public Task ShowNotification(string title, string body, string avatarUrl, bool roundedAvatar, string containerId)
            {
                return Task.CompletedTask;
            }
        }
    }
}
