using System;
using System.Runtime.InteropServices;
using GroupMeClient.AvaloniaUI.ViewModels;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using static GroupMeClient.AvaloniaUI.Notifications.Display.Win10.Win10ToastNotificationsProvider;

namespace GroupMeClient.AvaloniaUI.Notifications.Display.Win10
{
    /// <summary>
    /// Provides a COM Interface to support activation when a user clicks on a Windows 10 Toast Notfication.
    /// </summary>
    /// <remarks>
    /// Squirrel automatically generates a CLSID based on the NuGet package name.
    /// For 'GroupMeDesktopClient', this generated GUID is '3d1bf80b-078b-5aee-b9a0-fc40af7fc030'.
    /// </remarks>
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(INotificationActivationCallback))]
    [ComVisible(true)]
    [Guid("3333380b-078b-5aee-b9a0-fc40af7fffff")]
    public class GroupMeNotificationActivator : NotificationActivator
    {
        /// <inheritdoc/>
        public override void OnActivated(string invokedArgs, NotificationUserInput userInput, string appUserModelId)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (invokedArgs.Length == 0)
                {
                    // Perform a normal launch
                    this.OpenWindowIfNeeded();
                }

                var args = QueryString.Parse(invokedArgs);
                var action = (LaunchActions)Enum.Parse(typeof(LaunchActions), args["action"]);

                var mainViewModel = Program.GroupMeMainWindow.DataContext as MainViewModel;

                switch (action)
                {
                    case LaunchActions.ShowGroup:
                        this.OpenWindowIfNeeded();

                        var openChatCommand = new GroupMeClient.Core.Messaging.ShowChatRequestMessage(args["conversationId"]);
                        WeakReferenceMessenger.Default.Send(openChatCommand);

                        var showChatsPageCommand = new GroupMeClient.Core.Messaging.SwitchToPageRequestMessage(GroupMeClient.Core.Messaging.SwitchToPageRequestMessage.Page.Chats);
                        WeakReferenceMessenger.Default.Send(showChatsPageCommand);

                        break;

                    case LaunchActions.LikeMessage:
                        await mainViewModel.NotificationLikeMessage(args["conversationId"], args["messageId"]);
                        break;

                    case LaunchActions.InitiateReplyMessage:
                        this.ShowReplyToast(args["conversationId"], args["messageId"], args["containerName"], args["containerAvatar"]);
                        break;

                    case LaunchActions.SendReplyMessage:
                        var success = await mainViewModel.NotificationQuickReplyMessage(args["conversationId"], userInput["tbReply"]);
                        if (success)
                        {
                            this.ShowReplyConfirmation(args["conversationId"], args["messageId"]);
                        }

                        break;
                }
            });
        }

        private void OpenWindowIfNeeded()
        {
            // TODO

            //// Make sure we have a window open (in case user clicked toast while app closed)
            //if (Application.Current.Windows.Count == 0)
            //{
            //    new MainWindow().Show();
            //}

            //var mainWindow = Application.Current.Windows[0];

            //// Activate the window, bringing it to focus
            //mainWindow.Activate();

            //// And make sure to maximize the window too, in case it was currently minimized
            //// Setting 'Normal' will restore the window to maximized if it was full-screen before being minimized
            //if (mainWindow.WindowState == WindowState.Minimized)
            //{
            //    mainWindow.WindowState = WindowState.Normal;
            //}
        }

        private void ShowReplyToast(string containerId, string messageId, string containerName, string containerAvatar)
        {
            ToastContent toastContent = new ToastContent()
            {
                Launch = $"action={LaunchActions.ShowGroup}&conversationId={containerId}",

                Audio = new ToastAudio()
                {
                    Silent = true,
                },

                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastTextBox("tbReply")
                        {
                            PlaceholderContent = $"Message to {containerName}",
                        },
                    },

                    Buttons =
                    {
                        new ToastButton("Send", $"action={LaunchActions.SendReplyMessage}&conversationId={containerId}&messageId={messageId}")
                        {
                            TextBoxId = "tbReply",
                        },
                    },
                },

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"Reply to {containerName}",
                            },
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = containerAvatar,
                        },
                    },
                },
            };

            var doc = new XmlDocument();
            doc.LoadXml(toastContent.GetContent());

            // And create the toast notification
            var toast = new ToastNotification(doc)
            {
                Tag = $"{containerId}{messageId}",
            };

            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }

        private void ShowReplyConfirmation(string containerId, string messageId)
        {
            ToastContent toastContent = new ToastContent()
            {
                Launch = $"action={LaunchActions.ShowGroup}&conversationId={containerId}",

                Audio = new ToastAudio()
                {
                    Silent = true,
                },

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"Message Sent Successfully",
                            },
                        },
                    },
                },
            };

            var doc = new XmlDocument();
            doc.LoadXml(toastContent.GetContent());

            // And create the toast notification
            var toast = new ToastNotification(doc)
            {
                Tag = $"{containerId}{messageId}",
            };

            // And then show it
            DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
        }
    }
}
