using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Media;
using GroupMeClient.AvaloniaUI.Notifications.Display;
using GroupMeClient.AvaloniaUI.Notifications.Display.WpfToast;
using GroupMeClient.Core.Caching;
using GroupMeClient.Core.Messaging;
using GroupMeClient.Core.Notifications;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Settings;
using GroupMeClient.Core.Tasks;
using GroupMeClient.Core.ViewModels;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;
using MicroCubeAvalonia.Controls;
using MicroCubeAvalonia.IconPack;
using MicroCubeAvalonia.IconPack.Icons;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace GroupMeClient.AvaloniaUI.ViewModels
{
    /// <summary>
    /// <see cref="MainViewModel"/> is the top-level ViewModel for the GroupMe Desktop Client Avalonia.
    /// </summary>
    public class MainViewModel : ObservableObject
    {
        private AvaloniaList<HamburgerMenuItem> menuItems = new AvaloniaList<HamburgerMenuItem>();
        private AvaloniaList<HamburgerMenuItem> menuOptionItems = new AvaloniaList<HamburgerMenuItem>();
        private HamburgerMenuItem selectedItem;
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
        /// Gets the list of main items shown in the hamburger menu.
        /// </summary>
        public AvaloniaList<HamburgerMenuItem> MenuItems
        {
            get => this.menuItems;
            private set => this.SetProperty(ref this.menuItems, value);
        }

        /// <summary>
        /// Gets the list of options items shown in the hamburger menu (at the bottom).
        /// </summary>
        public AvaloniaList<HamburgerMenuItem> MenuOptionItems
        {
            get => this.menuOptionItems;
            private set => this.SetProperty(ref this.menuOptionItems, value);
        }

        /// <summary>
        /// Gets or sets the currently selected Hamburger Menu Tab.
        /// </summary>
        public HamburgerMenuItem SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value);
        }

        /// <summary>
        /// Gets the number of unread notifications that should be displayed in the
        /// taskbar badge.
        /// </summary>
        public int UnreadCount
        {
            get => this.unreadCount;
            private set => this.SetProperty(ref this.unreadCount, value);
        }

        /// <summary>
        /// Gets a value indicating whether the application is currently reconnecting to GroupMe.
        /// </summary>
        public bool IsReconnecting
        {
            get => this.isReconnecting;
            private set => this.SetProperty(ref this.isReconnecting, value);
        }

        /// <summary>
        /// Gets a value indicating whether the application is currently refreshing the displayed content.
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            private set => this.SetProperty(ref this.isRefreshing, value);
        }

        /// <summary>
        /// Gets a collection of reasons the application needs rebooted.
        /// </summary>
        public ObservableCollection<string> RebootReasons
        {
            get => this.rebootReasons;
            private set => this.SetProperty(ref this.rebootReasons, value);
        }

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
        /// Gets the Toast Holder Manager for this application.
        /// </summary>
        public ToastHolderViewModel ToastHolderManager { get; private set; }

        /// <summary>
        /// Gets the Task Manager for this application.
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

        private ProgressRing UpdatingSpinner { get; } = new ProgressRing() { IsActive = true, Width = 20, Foreground = Brushes.White };

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
               Ioc.Default.GetService<IPluginManagerService>().LoadPlugins(App.PluginsPath));

            // Setup messaging
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.UnreadRequestMessage>(this, (r, m) => r.UpdateNotificationCount(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.DisconnectedRequestMessage>(this, (r, m) => r.UpdateDisconnectedComponentsCount(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.SwitchToPageRequestMessage>(this, (r, m) => r.SwitchToPageCommand(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.RebootRequestMessage>(this, (r, m) => r.RebootReasons.Add(m.Reason));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.DialogRequestMessage>(this, (r, m) => r.OpenBigPopup(m));
            //WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.DialogDismissMessage>(this, (r, m) => r.DismissCallback(m));
            WeakReferenceMessenger.Default.Register<MainViewModel, Core.Messaging.RefreshAllMessage>(this, (r, m) => Task.Run(r.RefreshEverything));

            // Setup updating
            //Application.Current.MainWindow.Closing += new CancelEventHandler(this.MainWindow_Closing);
            //var updateService = SimpleIoc.Default.GetInstance<IUpdateService>();
            //updateService.StartUpdateTimer(TimeSpan.FromMinutes(this.SettingsManager.CoreSettings.ApplicationUpdateFrequencyMinutes));

            this.RebootApplication = new RelayCommand(this.RestartCommand);

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

            //Desktop.Native.Windows.RecoveryManager.RegisterForRecovery();
            //Desktop.Native.Windows.RecoveryManager.RegisterForRestart();
        }

        private void RegisterNotifications()
        {
            this.ToastHolderManager = new ToastHolderViewModel();

            this.NotificationRouter.RegisterNewSubscriber(this.ChatsViewModel);
            this.NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreatePlatformNotificationProvider(this.SettingsManager));
            this.NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreateInternalNotificationProvider(this.ToastHolderManager));
        }

        private void CreateMenuItemsRegular()
        {
            this.MenuItems.Clear();
            this.MenuOptionItems.Clear();

            var chatsTab = new HamburgerMenuItem()
            {
                Icon = new IconControl() { BindableKind = PackIconMaterialKind.MessageText },
                Label = "Chats",
                ToolTip = "View Groups and Chats.",
                Tag = this.ChatsViewModel,
            };

            var searchTab = new HamburgerMenuItem()
            {
                Icon = new IconControl() { BindableKind = PackIconMaterialKind.EmailSearch },
                Label = "Search",
                ToolTip = "Search all Groups and Chats.",
                Tag = this.SearchViewModel,
            };

            var starsTab = new HamburgerMenuItem()
            {
                Icon = new IconControl() { BindableKind = PackIconMaterialKind.Star },
                Label = "Starred Messages",
                ToolTip = "View Stars in Groups and Chats.",
                Tag = this.StarsViewModel,
            };

            var settingsTab = new HamburgerMenuItem()
            {
                Icon = new IconControl() { BindableKind = PackIconMaterialKind.SettingsOutline },
                Label = "Settings",
                ToolTip = "GroupMe Settings",
                Tag = this.SettingsViewModel,
            };

            // Add new Tabs
            this.MenuItems.Add(chatsTab);
            this.MenuItems.Add(searchTab);
            this.MenuItems.Add(starsTab);

            // Add new Options
            this.MenuOptionItems.Add(settingsTab);

            // Set the section to the Chats tab
            this.SelectedItem = chatsTab;

            // Enable the refresh button
            this.RefreshEverythingCommand = new RelayCommand(async () => await this.RefreshEverything());
        }

        private void CreateMenuItemsLoginOnly()
        {
            this.MenuItems.Clear();
            this.MenuOptionItems.Clear();

            var loginTab = new HamburgerMenuItem()
            {
                Icon = new IconControl() { BindableKind = PackIconMaterialKind.Login },
                Label = "Login",
                ToolTip = "Login To GroupMe",
                Tag = this.LoginViewModel,
            };

            this.MenuItems.Add(loginTab);
            this.SelectedItem = loginTab;

            // Disable Refresh button by marking CanExecute as false
            this.RefreshEverythingCommand = new RelayCommand(() => { }, canExecute: () => false);
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

                    var updatingTab = new HamburgerMenuItem()
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
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    Program.GMDCMainWindow.Close();
                });
            }
        }

        private void OpenBigPopup(DialogRequestMessage dialog)
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

        private void UpdateNotificationCount(UnreadRequestMessage update)
        {
            this.UnreadCount = update.Count;
        }

        private void UpdateDisconnectedComponentsCount(DisconnectedRequestMessage update)
        {
            this.DisconnectedComponentCount += update.Disconnected ? 1 : -1;
            this.DisconnectedComponentCount = Math.Max(this.DisconnectedComponentCount, 0); // make sure it never goes negative
            this.TaskManager.UpdateNumberOfBackgroundLoads(this.DisconnectedComponentCount);
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                this.IsReconnecting = this.DisconnectedComponentCount > 0 || this.TaskManager.RunningTasks.Count > 0;
            });
        }

        private void TaskManager_TaskCountChanged(object sender, EventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                this.IsReconnecting = this.DisconnectedComponentCount > 0 || this.TaskManager.RunningTasks.Count > 0;
            });
        }

        private void SwitchToPageCommand(SwitchToPageRequestMessage cmd)
        {
            ObservableObject selectedPage = null;

            switch (cmd.SelectedPage)
            {
                case SwitchToPageRequestMessage.Page.Chats:
                    selectedPage = this.ChatsViewModel;
                    break;
                case SwitchToPageRequestMessage.Page.Search:
                    selectedPage = this.SearchViewModel;
                    break;
                case SwitchToPageRequestMessage.Page.Settings:
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
