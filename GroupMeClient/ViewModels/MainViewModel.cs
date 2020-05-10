using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.Notifications.Display;
using GroupMeClient.Notifications.Display.WpfToast;
using GroupMeClient.Plugins;
using GroupMeClient.Updates;
using GroupMeClient.ViewModels.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;

namespace GroupMeClient.ViewModels
{
    /// <summary>
    /// <see cref="MainViewModel"/> is the top-level ViewModel for the GroupMe Desktop Client.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private HamburgerMenuItemCollection menuItems = new HamburgerMenuItemCollection();
        private HamburgerMenuItemCollection menuOptionItems = new HamburgerMenuItemCollection();
        private HamburgerMenuItem selectedItem;
        private int unreadCount;
        private bool isReconnecting;
        private bool isRefreshing;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            Directory.CreateDirectory(this.DataRoot);

            Utilities.TempFileUtils.InitializeTempStorage();

            this.TaskManager = new Tasks.TaskManager();
            this.CacheManager = new Caching.CacheManager(this.CachePath, this.TaskManager);

            this.TaskManager.TaskCountChanged += this.TaskManager_TaskCountChanged;

            // Create a throw-away DbContext to allow EF Core to begin allocating resouces
            // in the background, allowing for faster access later.
            Task.Run(() => this.CacheManager.OpenNewContext());

            // Perform additional startup procedures that are dependent on the values
            // configured in the settings file.
            this.InitializeClient();
        }

        /// <summary>
        /// Gets or sets the list of main items shown in the hamburger menu.
        /// </summary>
        public HamburgerMenuItemCollection MenuItems
        {
            get { return this.menuItems; }
            set { this.Set(() => this.MenuItems, ref this.menuItems, value); }
        }

        /// <summary>
        /// Gets or sets the list of options items shown in the hamburger menu (at the bottom).
        /// </summary>
        public HamburgerMenuItemCollection MenuOptionItems
        {
            get { return this.menuOptionItems; }
            set { this.Set(() => this.MenuOptionItems, ref this.menuOptionItems, value); }
        }

        /// <summary>
        /// Gets or sets the currently selected Hamburger Menu Tab.
        /// </summary>
        public HamburgerMenuItem SelectedItem
        {
            get { return this.selectedItem; }
            set { this.Set(() => this.SelectedItem, ref this.selectedItem, value); }
        }

        /// <summary>
        /// Gets or sets the number of unread notifications that should be displayed in the
        /// taskbar badge.
        /// </summary>
        public int UnreadCount
        {
            get { return this.unreadCount; }
            set { this.Set(() => this.UnreadCount, ref this.unreadCount, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is currently reconnecting to GroupMe.
        /// </summary>
        public bool IsReconnecting
        {
            get { return this.isReconnecting; }
            set { this.Set(() => this.IsReconnecting, ref this.isReconnecting, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is currently refreshing the displayed content.
        /// </summary>
        public bool IsRefreshing
        {
            get { return this.isRefreshing; }
            set { this.Set(() => this.IsRefreshing, ref this.isRefreshing, value); }
        }

        /// <summary>
        /// Gets the manager for the dialog that should be displayed as a large popup.
        /// </summary>
        public PopupViewModel DialogManager { get; set; }

        /// <summary>
        /// Gets or sets the command to be performed to refresh all displayed messages and groups.
        /// </summary>
        public ICommand RefreshEverythingCommand { get; set; }

        /// <summary>
        /// Gets the Toast Holder Manager for this application.
        /// </summary>
        public ToastHolderViewModel ToastHolderManager { get; private set; }

        /// <summary>
        /// Gets the Task Manager for this application.
        /// </summary>
        public Tasks.TaskManager TaskManager { get; }

        private string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MicroCube", "GroupMe Desktop Client");

        private string SettingsPath => Path.Combine(this.DataRoot, "settings.json");

        private string CachePath => Path.Combine(this.DataRoot, "cache.db");

        private string ImageCachePath => Path.Combine(this.DataRoot, "ImageCache");

        private string PluginsPath => Path.Combine(this.DataRoot, "Plugins");

        private GroupMeClientApi.GroupMeClient GroupMeClient { get; set; }

        private Caching.CacheManager CacheManager { get; }

        private Settings.SettingsManager SettingsManager { get; set; }

        private NotificationRouter NotificationRouter { get; set; }

        private UpdateAssist UpdateAssist { get; set; }

        private ChatsViewModel ChatsViewModel { get; set; }

        private SearchViewModel SearchViewModel { get; set; }

        private SettingsViewModel SettingsViewModel { get; set; }

        private LoginViewModel LoginViewModel { get; set; }

        private ProgressRing UpdatingSpinner { get; } = new ProgressRing() { IsActive = true, Width = 20, Foreground = System.Windows.Media.Brushes.White };

        private int DisconnectedComponentCount { get; set; }

        private void InitializeClient()
        {
            this.SettingsManager = new Settings.SettingsManager(this.SettingsPath);
            this.SettingsManager.LoadSettings();

            PluginManager.Instance.LoadPlugins(this.PluginsPath);

            Messenger.Default.Register<Messaging.UnreadRequestMessage>(this, this.UpdateNotificationCount);
            Messenger.Default.Register<Messaging.DisconnectedRequestMessage>(this, this.UpdateDisconnectedComponentsCount);
            Messenger.Default.Register<Messaging.IndexAndRunPluginRequestMessage>(this, this.IndexAndRunCommand);

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
                this.GroupMeClient = new GroupMeClientApi.GroupMeClient(this.SettingsManager.CoreSettings.AuthToken);
                this.GroupMeClient.ImageDownloader = new GroupMeClientApi.CachedImageDownloader(this.ImageCachePath);

                this.NotificationRouter = new NotificationRouter(this.GroupMeClient);

                this.ChatsViewModel = new ChatsViewModel(this.GroupMeClient, this.SettingsManager, this.CacheManager);
                this.SearchViewModel = new SearchViewModel(this.GroupMeClient, this.CacheManager);
                this.SettingsViewModel = new SettingsViewModel(this.SettingsManager);

                this.RegisterNotifications();

                this.CreateMenuItemsRegular();
            }

            Messenger.Default.Register<Messaging.DialogRequestMessage>(this, this.OpenBigPopup);

            this.DialogManager = new PopupViewModel()
            {
                EasyClosePopup = new RelayCommand(this.CloseBigPopup),
                ClosePopup = new RelayCommand(this.CloseBigPopup),
                PopupDialog = null,
            };

            this.UpdateAssist = new UpdateAssist();
            Application.Current.MainWindow.Closing += new CancelEventHandler(this.MainWindow_Closing);

            Native.RecoveryManager.RegisterForRecovery();
            Native.RecoveryManager.RegisterForRestart();
        }

        private void RegisterNotifications()
        {
            this.ToastHolderManager = new ToastHolderViewModel();

            this.NotificationRouter.RegisterNewSubscriber(this.ChatsViewModel);
            this.NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreatePlatformNotificationProvider());
            this.NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreateInternalNotificationProvider(this.ToastHolderManager));
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

            var secondTab = new HamburgerMenuIconItem()
            {
                Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.EmailSearch },
                Label = "Search",
                ToolTip = "Search all Groups and Chats.",
                Tag = this.SearchViewModel,
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
            this.MenuItems.Add(secondTab);

            // Add Options
            this.MenuOptionItems.Add(settingsTab);

            // Enable the refresh button
            this.RefreshEverythingCommand = new RelayCommand(async () => await this.RefreshEverything(), true);

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
            if (!this.UpdateAssist.CanShutdown)
            {
                this.UpdateAssist.UpdateMonitor.Task.ContinueWith(this.UpdateCompleted);
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
            }
        }

        private void UpdateCompleted(Task<bool> result)
        {
            // safe to shutdown now.
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow.Close();
            });
        }

        private void OpenBigPopup(Messaging.DialogRequestMessage dialog)
        {
            this.DialogManager.PopupDialog = dialog.Dialog;
        }

        private void CloseBigPopup()
        {
            if (this.DialogManager.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.DialogManager.PopupDialog = null;
        }

        private void UpdateNotificationCount(Messaging.UnreadRequestMessage update)
        {
            this.UnreadCount = update.Count;
        }

        private void UpdateDisconnectedComponentsCount(Messaging.DisconnectedRequestMessage update)
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

        private void IndexAndRunCommand(Messaging.IndexAndRunPluginRequestMessage cmd)
        {
            this.SearchViewModel.RunPlugin(cmd.MessageContainer, cmd.Plugin);
        }

        private async Task RefreshEverything()
        {
            this.IsRefreshing = true;
            await this.ChatsViewModel.RefreshEverything();
            this.IsRefreshing = false;
        }
    }
}