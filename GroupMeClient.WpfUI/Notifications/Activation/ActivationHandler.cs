﻿using System;
using System.Collections.Generic;
using System.Windows;
using GroupMeClient.WpfUI.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GroupMeClient.WpfUI.Notifications.Activation
{
    public class ActivationHandler
    {
        public static void HandleActivation(string arguments, IDictionary<string, object> userInput)
        {
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                if (arguments.Length == 0)
                {
                    // Perform a normal launch
                    OpenWindowIfNeeded();
                }

                // Parse user arguments
                var args = ToastArguments.Parse(arguments);

                var conversationId = args[NotificationArguments.ConversationId];

                args.TryGetValue(NotificationArguments.MessageId, out var messageId);
                args.TryGetValue(NotificationArguments.ContainerName, out var containerName);
                args.TryGetValue(NotificationArguments.ContainerAvatar, out var containerAvatar);

                var action = LaunchActions.ShowGroup;
                if (args.Contains(NotificationArguments.Action))
                {
                    action = (LaunchActions)Enum.Parse(typeof(LaunchActions), args[NotificationArguments.Action]);
                }

                // Actions are currently routed through the MainViewModel which is kinda hacky but works ¯\_(ツ)_/¯.
                var mainViewModel = (App.Current.Windows[0] as MainWindow).DataContext as MainViewModel;

                switch (action)
                {
                    case LaunchActions.ShowGroup:
                        OpenWindowIfNeeded();

                        var openChatCommand = new Core.Messaging.ShowChatRequestMessage(conversationId);
                        WeakReferenceMessenger.Default.Send(openChatCommand);

                        var showChatsPageCommand = new Core.Messaging.SwitchToPageRequestMessage(Core.Messaging.SwitchToPageRequestMessage.Page.Chats);
                        WeakReferenceMessenger.Default.Send(showChatsPageCommand);

                        break;

                    case LaunchActions.LikeMessage:
                        await mainViewModel.NotificationLikeMessage(conversationId, messageId);
                        break;

                    case LaunchActions.InitiateReplyMessage:
                        ShowReplyToast(conversationId, messageId, containerName, containerAvatar);
                        break;

                    case LaunchActions.SendReplyMessage:
                        var success = await mainViewModel.NotificationQuickReplyMessage(conversationId, (string)userInput["tbReply"]);
                        if (success)
                        {
                            ShowReplyConfirmation(conversationId, messageId);
                        }

                        break;
                }
            });
        }

        private static void OpenWindowIfNeeded()
        {
            // Make sure we have a window open (in case user clicked toast while app closed)
            if (Application.Current.Windows.Count == 0)
            {
                new MainWindow().Show();
            }

            var mainWindow = Application.Current.Windows[0];

            // Activate the window, bringing it to focus
            mainWindow.Activate();

            // And make sure to maximize the window too, in case it was currently minimized
            // Setting 'Normal' will restore the window to maximized if it was full-screen before being minimized
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Normal;
            }
        }

        private static void ShowReplyToast(string containerId, string messageId, string containerName, string containerAvatar)
        {
            new ToastContentBuilder()
                .AddArgument(NotificationArguments.ConversationId, containerId)
                .AddText($"Reply to {containerName}")
                .AddAppLogoOverride(new Uri(containerAvatar))
                .AddInputTextBox("tbReply", $"Message to {containerName}")
                .AddButton(new ToastButton()
                    .SetBackgroundActivation()
                    .SetTextBoxId("tbReply")
                    .SetContent("Send")
                    .AddArgument(NotificationArguments.Action, LaunchActions.SendReplyMessage))
                .AddAudio(new ToastAudio() { Silent = true })
                .Show(toast =>
                {
                    toast.Tag = $"{containerId}{messageId}";
                    toast.Group = containerId;
                });
        }

        private static void ShowReplyConfirmation(string containerId, string messageId)
        {
            new ToastContentBuilder()
                .AddArgument(NotificationArguments.ConversationId, containerId)
                .AddText("Message Sent Successfully", AdaptiveTextStyle.Title)
                .AddAudio(new ToastAudio() { Silent = true })
                .Show(toast =>
                {
                    toast.Tag = $"{containerId}{messageId}";
                    toast.Group = containerId;
                });
        }
    }
}
