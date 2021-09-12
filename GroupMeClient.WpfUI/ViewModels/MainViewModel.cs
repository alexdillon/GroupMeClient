﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Notifications;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Tasks;
using GroupMeClient.Core.ViewModels;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClient.WpfUI.Notifications.Display;
using GroupMeClient.WpfUI.Notifications.Display.WpfToast;
using GroupMeClientApi.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace GroupMeClient.WpfUI.ViewModels
{
    /// <summary>
    /// <see cref="MainViewModel"/> is the top-level ViewModel for the GroupMe Desktop Client, WPF implementation.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private HamburgerMenuItemCollection menuItems = new HamburgerMenuItemCollection();
        private HamburgerMenuItemCollection menuOptionItems = new HamburgerMenuItemCollection();
        private HamburgerMenuItemBase selectedItem;
        private int unreadCount;
        private bool isReconnecting;
        private bool isRefreshing;
        private ObservableCollection<string> rebootReasons = new ObservableCollection<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            Desktop.MigrationAssistant.MigrationManager.EnsureMigration(App.StartupParams);
            this.SettingsManager = Ioc.Default.GetService<SettingsManager>();
            this.ClientIdentity = Ioc.Default.GetService<IClientIdentityService>();

            // Create a throw-away DbContext to allow EF Core to begin allocating resouces
            // in the background, allowing for faster access later.
            Task.Run(() => Ioc.Default.GetService<CacheManager>().OpenNewContext());
            Task.Run(() => Ioc.Default.GetService<PersistManager>().OpenNewContext());

            Core.Utilities.TempFileUtils.InitializeTempStorage();

            // Perform additional startup procedures that are dependent on the values
            // configured in the settings file.
            this.InitializeClient();
        }

        /// <summary>
        /// Gets or sets the list of main items shown in the hamburger menu.
        /// </summary>
        public HamburgerMenuItemCollection MenuItems
        {
            get => this.menuItems;
            set => this.SetProperty(ref this.menuItems, value);
        }

        /// <summary>
        /// Gets or sets the list of options items shown in the hamburger menu (at the bottom).
        /// </summary>
        public HamburgerMenuItemCollection MenuOptionItems
        {
            get => this.menuOptionItems;
            set => this.SetProperty(ref this.menuOptionItems, value);
        }

        /// <summary>
        /// Gets or sets the currently selected Hamburger Menu Tab.
        /// </summary>
        public HamburgerMenuItemBase SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value);
        }

        /// <summary>
        /// Gets or sets the number of unread notifications that should be displayed in the
        /// taskbar badge.
        /// </summary>
        public int UnreadCount
        {
            get => this.unreadCount;
            set => this.SetProperty(ref this.unreadCount, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is currently reconnecting to GroupMe.
        /// </summary>
        public bool IsReconnecting
        {
            get => this.isReconnecting;
            set => this.SetProperty(ref this.isReconnecting, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is currently refreshing the displayed content.
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.SetProperty(ref this.isRefreshing, value);
        }

        /// <summary>
        /// Gets or sets a collection of reasons the application needs rebooted.
        /// </summary>
        public ObservableCollection<string> RebootReasons
        {
            get => this.rebootReasons;
            set => this.SetProperty(ref this.rebootReasons, value);
        }

        /// <summary>
        /// Gets a value indicating whether this ViewModel is represents a complete copy of GMDC.
        /// </summary>
        public bool IsFullGMDC => true;

        /// <summary>
        /// Gets the manager for the dialog that should be displayed as a large popup.
        /// </summary>
        public PopupViewModel DialogManagerRegular { get; private set; }

        /// <summary>
        /// Gets the manager for the dialog that should be displayed as a large topmost popup.
        /// </summary>
        public PopupViewModel DialogManagerTopMost { get; private set; }

        /// <summary>
        /// Gets the command to be performed to refresh all displayed messages and groups.
        /// </summary>
        public ICommand RefreshEverythingCommand { get; private set; }

        /// <summary>
        /// Gets the command to be performed to soft reboot the application.
        /// </summary>
        public ICommand RebootApplication { get; private set; }

        /// <summary>
        /// Gets the command to be performed to snap to the chats page.
        /// </summary>
        public ICommand GotoChatsPage { get; private set; }

        /// <summary>
        /// Gets the command to be performed to snap to the search page.
        /// </summary>
        public ICommand GotoSearchPage { get; private set; }

        /// <summary>
        /// Gets the Toast Holder Manager for this application.
        /// </summary>
        public ToastHolderViewModel ToastHolderManager { get; private set; }

        /// <summary>
        /// Gets the Task Manager in use for this application.
        /// </summary>
        public TaskManager TaskManager { get; private set; }

        private IClientIdentityService ClientIdentity { get; }

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private SettingsManager SettingsManager { get; set; }

        private NotificationRouter NotificationRouter { get; set; }

        private ChatsViewModel ChatsViewModel { get; set; }

        private SearchViewModel SearchViewModel { get; set; }

        private StarsViewModel StarsViewModel { get; set; }

        private SettingsViewModel SettingsViewModel { get; set; }

        private LoginViewModel LoginViewModel { get; set; }

        private ProgressRing UpdatingSpinner { get; } = new ProgressRing() { IsActive = true, Width = 20, Foreground = System.Windows.Media.Brushes.White };

        private int DisconnectedComponentCount { get; set; }

        /// <summary>
        /// Provides <see cref="Message"/> sending functionality than can be invoked from a notification.
        /// </summary>
        /// <param name="containerId">The ID of the <see cref="Group"/> or <see cref="Chat"/> to send to.</param>
        /// <param name="messageText">The message text to send.</param>
        /// /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> NotificationQuickReplyMessage(string containerId, string messageText)
        {
            var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Chats(), this.GroupMeClient.Groups());
            var group = groupsAndChats.FirstOrDefault(g => g.Id == containerId);
            var msg = Message.CreateMessage(
                body: messageText,
                guidPrefix: this.ClientIdentity.ClientGuidQuickResponsePrefix);

            if (msg == null)
            {
                return false;
            }

            return await group?.SendMessage(msg);
        }

        /// <summary>
        /// Provides "Liking" functionality for a <see cref="Message"/> that can be invoked from a notification.
        /// </summary>
        /// <param name="containerId">The <see cref="Group"/> or <see cref="Chat"/> ID.</param>
        /// <param name="messageId">The ID of the <see cref="Message"/> to like.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> NotificationLikeMessage(string containerId, string messageId)
        {
            var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.GroupMeClient.Chats(), this.GroupMeClient.Groups());
            var group = groupsAndChats.FirstOrDefault(g => g.Id == containerId);
            var message = group?.Messages.FirstOrDefault(m => m.Id == messageId);

            if (message == null)
            {
                return false;
            }

            return await message.LikeMessage();
        }

        private void InitializeClient()
        {
            // Setup plugins
            Task.Run(() =>
                Ioc.Default.GetService<IPluginManagerService>().LoadPlugins(WpfUI.App.PluginsPath));

            // Setup messaging
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.UnreadRequestMessage>(this, (r, m) => r.UpdateNotificationCount(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.DisconnectedRequestMessage>(this, (r, m) => r.UpdateDisconnectedComponentsCount(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.SwitchToPageRequestMessage>(this, (r, m) => r.SwitchToPageCommand(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.RebootRequestMessage>(this, (r, m) => r.RebootReasons.Add(m.Reason));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.DialogRequestMessage>(this, (r, m) => r.OpenBigPopup(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.DialogDismissMessage>(this, (r, m) => r.DismissCallback(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.RefreshAllMessage>(this, (r, m) => Task.Run(r.RefreshEverything));

            // Setup updating
            Application.Current.MainWindow.Closing += new CancelEventHandler(this.MainWindow_Closing);
            var updateService = Ioc.Default.GetService<IUpdateService>();
            updateService.StartUpdateTimer(TimeSpan.FromMinutes(this.SettingsManager.CoreSettings.ApplicationUpdateFrequencyMinutes));

            this.RebootApplication = new RelayCommand(this.RestartCommand);
            this.GotoChatsPage = new RelayCommand(() => this.SwitchToPageCommand(new Core.Messaging.SwitchToPageRequestMessage(Core.Messaging.SwitchToPageRequestMessage.Page.Chats)));
            this.GotoSearchPage = new RelayCommand(() => this.SwitchToPageCommand(new Core.Messaging.SwitchToPageRequestMessage(Core.Messaging.SwitchToPageRequestMessage.Page.Search)));

            this.TaskManager = Ioc.Default.GetService<TaskManager>();
            this.TaskManager.TaskCountChanged += this.TaskManager_TaskCountChanged;

            if (string.IsNullOrEmpty(this.SettingsManager.CoreSettings.AuthToken))
            {
                // Startup in Login Mode
                this.LoginViewModel = new LoginViewModel(this.SettingsManager)
                {
                    LoginCompleted = new RelayCommand(this.InitializeClient),
                };

                this.CreateMenuItemsLoginOnly();
            }
            else
            {
                // Startup Regularly
                this.GroupMeClient = Ioc.Default.GetRequiredService<GroupMeClientApi.GroupMeClient>();
                this.GroupMeClient.ImageDownloader = new GroupMeClientApi.CachedImageDownloader(App.ImageCachePath);

                this.NotificationRouter = new NotificationRouter(this.GroupMeClient);

                this.ChatsViewModel = Ioc.Default.GetService<ChatsViewModel>();
                this.SearchViewModel = Ioc.Default.GetService<SearchViewModel>();
                this.StarsViewModel = Ioc.Default.GetService<StarsViewModel>();
                this.SettingsViewModel = Ioc.Default.GetService<SettingsViewModel>();

                this.RegisterNotifications();

                this.CreateMenuItemsRegular();
            }

            this.DialogManagerRegular = new PopupViewModel()
            {
                ClosePopupCallback = new RelayCommand(this.CloseBigPopup),
                EasyClosePopupCallback = new RelayCommand(this.CloseBigPopup),
            };

            this.DialogManagerTopMost = new PopupViewModel()
            {
                ClosePopupCallback = new RelayCommand(this.CloseBigTopMostPopup),
                EasyClosePopupCallback = new RelayCommand(this.CloseBigTopMostPopup),
            };

            Desktop.Native.Windows.RecoveryManager.RegisterForRecovery();
            Desktop.Native.Windows.RecoveryManager.RegisterForRestart();
        }

        private void RegisterNotifications()
        {
            this.ToastHolderManager = new ToastHolderViewModel();

            this.NotificationRouter.RegisterNewSubscriber(this.ChatsViewModel);
            this.NotificationRouter.RegisterNewSubscriber(NativeDesktopNotificationProvider.CreatePlatformNotificationProvider(this.SettingsManager));
            this.NotificationRouter.RegisterNewSubscriber(NativeDesktopNotificationProvider.CreateInternalNotificationProvider(this.ToastHolderManager));
        }

        private void CreateMenuItemsRegular()
        {
            // "Tabs" (Top Menu Items)
            var chatsTab = new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.MessageText },
                Label = "Chats",
                ToolTip = "View Groups and Chats.",
                Tag = this.ChatsViewModel,
            };

            var searchTab = new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.EmailSearch },
                Label = "Search",
                ToolTip = "Search all Groups and Chats.",
                Tag = this.SearchViewModel,
            };

            var starsTab = new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Star },
                Label = "Starred Messages",
                ToolTip = "View Stars in Groups and Chats.",
                Tag = this.StarsViewModel,
            };

            // Options (Bottom Menu Items)
            var settingsTab = new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.CogOutline },
                Label = "Settings",
                ToolTip = "GroupMe Settings",
                Tag = this.SettingsViewModel,
            };

            // Add Tabs
            this.MenuItems.Add(chatsTab);
            this.MenuItems.Add(searchTab);
            this.MenuItems.Add(starsTab);

            // Add Options
            this.MenuOptionItems.Add(settingsTab);

            // Enable the refresh button
            this.RefreshEverythingCommand = new AsyncRelayCommand(this.RefreshEverything);

            // Set the section to the Chats tab
            this.SelectedItem = chatsTab;

            // Remove the old Tabs and Options AFTER the new one has been bound
            // There should be a better way to do this...
            var newTopOptionIndex = this.MenuOptionItems.IndexOf(settingsTab);
            for (int i = 0; i < newTopOptionIndex; i++)
            {
                this.MenuOptionItems.RemoveAt(0);
            }

            var newTopIndex = this.MenuItems.IndexOf(chatsTab);
            for (int i = 0; i < newTopIndex; i++)
            {
                this.MenuItems.RemoveAt(0);
            }
        }

        private void CreateMenuItemsLoginOnly()
        {
            this.MenuItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Login },
                    Label = "Login",
                    ToolTip = "Login To GroupMe",
                    Tag = this.LoginViewModel,
                },
            };

            this.MenuOptionItems = new HamburgerMenuItemCollection();

            // Disable Refresh button by marking CanExecute as false
            this.RefreshEverythingCommand = new RelayCommand(() => { }, canExecute: () => false);

            this.SelectedItem = this.MenuItems[0];
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var updateService = Ioc.Default.GetService<IUpdateService>();

            updateService.CanShutdown.Subscribe(canShutDown =>
            {
                if (!canShutDown)
                {
                    // Cancel the shutdown and show the updating indicator
                    e.Cancel = true;

                    var updatingTab = new HamburgerMenuIconItem()
                    {
                        Icon = this.UpdatingSpinner,
                        Label = "Updating",
                        ToolTip = "Updating",
                        Tag = new UpdatingViewModel(),
                    };

                    this.MenuItems.Add(updatingTab);
                    this.SelectedItem = updatingTab;

                    updateService.CanShutdown.Subscribe(this.CanShutdownChanged);
                }
            }).Dispose();
        }

        private void CanShutdownChanged(bool canShutdown)
        {
            if (canShutdown)
            {
                // safe to shutdown now.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.MainWindow.Close();
                });
            }
        }

        private void OpenBigPopup(Core.Messaging.DialogRequestMessage dialog)
        {
            if (dialog.TopMost)
            {
                this.DialogManagerTopMost.OpenPopup(dialog.Dialog, dialog.DialogId);
            }
            else
            {
                this.DialogManagerRegular.OpenPopup(dialog.Dialog, dialog.DialogId);
            }
        }

        private void CloseBigPopup()
        {
            if (this.DialogManagerRegular.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            var closeId = this.DialogManagerRegular.PopupId;
            this.DialogManagerRegular.ClosePopup();
            WeakReferenceMessenger.Default.Send(new Core.Messaging.DialogDismissMessage(closeId));
        }

        private void CloseBigTopMostPopup()
        {
            if (this.DialogManagerTopMost.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            var closeId = this.DialogManagerTopMost.PopupId;
            this.DialogManagerTopMost.ClosePopup();
            WeakReferenceMessenger.Default.Send(new Core.Messaging.DialogDismissMessage(closeId));
        }

        private void DismissCallback(Core.Messaging.DialogDismissMessage dismissMessage)
        {
            if (this.DialogManagerTopMost.PopupId == dismissMessage.DialogId && dismissMessage.DialogId != Guid.Empty)
            {
                this.CloseBigTopMostPopup();
            }
            else if (this.DialogManagerRegular.PopupId == dismissMessage.DialogId && dismissMessage.DialogId != Guid.Empty)
            {
                this.CloseBigPopup();
            }
        }

        private void UpdateNotificationCount(Core.Messaging.UnreadRequestMessage update)
        {
            this.UnreadCount = update.Count;
        }

        private void UpdateDisconnectedComponentsCount(Core.Messaging.DisconnectedRequestMessage update)
        {
            this.DisconnectedComponentCount += update.Disconnected ? 1 : -1;
            this.DisconnectedComponentCount = Math.Max(this.DisconnectedComponentCount, 0); // make sure it never goes negative
            this.TaskManager.UpdateNumberOfBackgroundLoads(this.DisconnectedComponentCount);
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsReconnecting = this.DisconnectedComponentCount > 0 || this.TaskManager.RunningTasks.Count > 0;
            });
        }

        private void TaskManager_TaskCountChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsReconnecting = this.DisconnectedComponentCount > 0 || this.TaskManager.RunningTasks.Count > 0;
            });
        }

        private void SwitchToPageCommand(Core.Messaging.SwitchToPageRequestMessage cmd)
        {
            ObservableObject selectedPage = null;

            switch (cmd.SelectedPage)
            {
                case Core.Messaging.SwitchToPageRequestMessage.Page.Chats:
                    selectedPage = this.ChatsViewModel;
                    break;
                case Core.Messaging.SwitchToPageRequestMessage.Page.Search:
                    selectedPage = this.SearchViewModel;
                    break;
                case Core.Messaging.SwitchToPageRequestMessage.Page.Settings:
                    selectedPage = this.SettingsViewModel;
                    break;
            }

            foreach (var menuItem in this.MenuItems)
            {
                if (menuItem.Tag == selectedPage)
                {
                    this.SelectedItem = menuItem;
                    break;
                }
            }
        }

        private async Task RefreshEverything()
        {
            this.IsRefreshing = true;
            await this.ChatsViewModel.RefreshEverything();
            this.IsRefreshing = false;
        }

        private void RestartCommand()
        {
            var restoreService = Ioc.Default.GetService<IRestoreService>();
            restoreService.SoftApplicationRestart();
        }
    }
}