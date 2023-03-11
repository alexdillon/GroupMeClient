﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GroupMeClient.AvaloniaUI.Notifications.Display.WpfToast
{
    /// <summary>
    /// <see cref="ToastHolderViewModel"/> provides a ViewModel for the <see cref="ToastHolder"/> control.
    /// Controls for sending messages are also included.
    /// </summary>
    public class ToastHolderViewModel : ObservableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToastHolderViewModel"/> class.
        /// </summary>
        public ToastHolderViewModel()
        {
            this.Notifications = new ObservableCollection<ToastNotificationViewModel>();
            this.DismissalTasks = new List<Task>();
        }

        /// <summary>
        /// Gets a collection of <see cref="ToastNotification"/> to display.
        /// </summary>
        public ObservableCollection<ToastNotificationViewModel> Notifications { get; }

        /// <summary>
        /// Gets or sets the maximum number of notifications that can be displayed at once.
        /// </summary>
        public int MaximumNotifications { get; set; } = 5;

        /// <summary>
        /// Gets or sets the amount of time for which toast notifications are displayed.
        /// After this time has elapsed, notifications will be automatically dismissed.
        /// </summary>
        public TimeSpan AutomaticDismissalTime { get; set; } = TimeSpan.FromSeconds(7);

        private List<Task> DismissalTasks { get; }

        /// <summary>
        /// Displays a new Toast Notification.
        /// </summary>
        /// <param name="notification">The toast to display.</param>
        public void DisplayNewToast(ToastNotificationViewModel notification)
        {
            notification.CloseAction = new RelayCommand<ToastNotificationViewModel>(this.CloseToast);
            this.DismissalTasks.Add(this.DelayedDismissal(notification));

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Notifications.Insert(0, notification);

                while (this.Notifications.Count > this.MaximumNotifications)
                {
                    this.Notifications.RemoveAt(this.Notifications.Count - 1);
                }
            });
        }

        private void CloseToast(ToastNotificationViewModel notification)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.Notifications.Remove(notification);
            });
        }

        private async Task DelayedDismissal(ToastNotificationViewModel toast)
        {
            await Task.Delay(this.AutomaticDismissalTime);
            this.CloseToast(toast);
        }
    }
}