using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GroupMeClient.AvaloniaUI.Notifications.Display;
using GroupMeClient.AvaloniaUI.Notifications.Display.WpfToast;
using GroupMeClient.AvaloniaUI.Updates;
using GroupMeClient.Core.Messaging;
using GroupMeClient.Core.Notifications;
using GroupMeClient.Core.Services;
using GroupMeClient.Core.Tasks;
using GroupMeClient.Core.ViewModels;
using GroupMeClient.Core.ViewModels.Controls;
using GroupMeClientApi.Models;
using MicroCubeAvalonia.Controls;
using MicroCubeAvalonia.IconPack;
using MicroCubeAvalonia.IconPack.Icons;

namespace GroupMeClient.AvaloniaUI.ViewModels
{
    /// <summary>
    /// <see cref="MainViewModel"/> is the top-level ViewModel for the GroupMe Desktop Client Avalonia.
    /// </summary>
    public class MainViewModel : ViewModelBase
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
            Startup.StartupServices();
            Directory.CreateDirectory(this.DataRoot);

            GroupMeClient.Core.Utilities.TempFileUtils.InitializeTempStorage();

            this.TaskManager = new TaskManager();
            this.CacheManager = new GroupMeClient.Core.Caching.CacheManager(this.CachePath, this.TaskManager);

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
        public AvaloniaList<HamburgerMenuItem> MenuItems
        {
            get => this.menuItems;
            set => this.Set(() => this.MenuItems, ref this.menuItems, value);
        }

        /// <summary>
        /// Gets or sets the list of options items shown in the hamburger menu (at the bottom).
        /// </summary>
        public AvaloniaList<HamburgerMenuItem> MenuOptionItems
        {
            get => this.menuOptionItems;
            set => this.Set(() => this.MenuOptionItems, ref this.menuOptionItems, value);
        }

        /// <summary>
        /// Gets or sets the currently selected Hamburger Menu Tab.
        /// </summary>
        public HamburgerMenuItem SelectedItem
        {
            get => this.selectedItem;
            set => this.Set(() => this.SelectedItem, ref this.selectedItem, value);
        }

        /// <summary>
        /// Gets or sets the number of unread notifications that should be displayed in the
        /// taskbar badge.
        /// </summary>
        public int UnreadCount
        {
            get => this.unreadCount;
            set => this.Set(() => this.UnreadCount, ref this.unreadCount, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is currently reconnecting to GroupMe.
        /// </summary>
        public bool IsReconnecting
        {
            get => this.isReconnecting;
            set => this.Set(() => this.IsReconnecting, ref this.isReconnecting, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is currently refreshing the displayed content.
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.Set(() => this.IsRefreshing, ref this.isRefreshing, value);
        }

        /// <summary>
        /// Gets or sets a collection of reasons the application needs rebooted.
        /// </summary>
        public ObservableCollection<string> RebootReasons
        {
            get => this.rebootReasons;
            set => this.Set(() => this.RebootReasons, ref this.rebootReasons, value);
        }

        /// <summary>
        /// Gets or sets the manager for the dialog that should be displayed as a large popup.
        /// </summary>
        public PopupViewModel DialogManagerRegular { get; set; }

        /// <summary>
        /// Gets or sets the manager for the dialog that should be displayed as a large topmost popup.
        /// </summary>
        public PopupViewModel DialogManagerTopMost { get; set; }

        /// <summary>
        /// Gets or sets the command to be performed to refresh all displayed messages and groups.
        /// </summary>
        public ICommand RefreshEverythingCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to be performed to soft reboot the application.
        /// </summary>
        public ICommand RebootApplication { get; set; }

        /// <summary>
        /// Gets the Toast Holder Manager for this application.
        /// </summary>
        public ToastHolderViewModel ToastHolderManager { get; private set; }

        /// <summary>
        /// Gets the Task Manager for this application.
        /// </summary>
        public TaskManager TaskManager { get; }

        private string DataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MicroCube", "GroupMe Desktop Client");

        private string SettingsPath => Path.Combine(this.DataRoot, "settings.json");

        private string CachePath => Path.Combine(this.DataRoot, "cache.db");

        private string ImageCachePath => Path.Combine(this.DataRoot, "ImageCache");

        private string PluginsPath => Path.Combine(this.DataRoot, "Plugins");

        private GroupMeClientApi.GroupMeClient Client { get; set; }

        private GroupMeClient.Core.Caching.CacheManager CacheManager { get; }

        private GroupMeClient.Core.Settings.SettingsManager SettingsManager { get; set; }

        private NotificationRouter NotificationRouter { get; set; }

        private UpdateAssist UpdateAssist { get; set; }

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
            var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.Client.Chats(), this.Client.Groups());
            var group = groupsAndChats.FirstOrDefault(g => g.Id == containerId);
            var msg = Message.CreateMessage(
                body: messageText,
                guidPrefix: "gmdcatoast");

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
            var groupsAndChats = Enumerable.Concat<IMessageContainer>(this.Client.Chats(), this.Client.Groups());
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
            this.SettingsManager = new GroupMeClient.Core.Settings.SettingsManager(this.SettingsPath);
            this.SettingsManager.LoadSettings();

            //PluginInstaller.SetupPluginInstaller(this.PluginsPath);

            var pluginManager = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IPluginManagerService>();
            pluginManager.LoadPlugins(this.PluginsPath);

            Messenger.Default.Register<UnreadRequestMessage>(this, this.UpdateNotificationCount);
            Messenger.Default.Register<DisconnectedRequestMessage>(this, this.UpdateDisconnectedComponentsCount);
            Messenger.Default.Register<RunPluginRequestMessage>(this, this.IndexAndRunCommand);
            Messenger.Default.Register<SwitchToPageRequestMessage>(this, this.SwitchToPageCommand);
            Messenger.Default.Register<RebootRequestMessage>(this, (r) => this.RebootReasons.Add(r.Reason), true);

            this.RebootApplication = new RelayCommand(this.RestartCommand);

            //this.UpdateAssist = new UpdateAssist();
            //Application.Current.MainWindow.Closing += new CancelEventHandler(this.MainWindow_Closing);
            //this.UpdateAssist.BeginCheckForUpdates();
            //this.UpdateAssist.StartUpdateTimer(TimeSpan.FromMinutes(this.SettingsManager.CoreSettings.ApplicationUpdateFrequencyMinutes));

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
                this.Client = new GroupMeClientApi.GroupMeClient(this.SettingsManager.CoreSettings.AuthToken);
                this.Client.ImageDownloader = new GroupMeClientApi.CachedImageDownloader(this.ImageCachePath);

                this.NotificationRouter = new NotificationRouter(this.Client);

                this.ChatsViewModel = new ChatsViewModel(this.Client, this.SettingsManager, this.CacheManager);
                this.SearchViewModel = new SearchViewModel(this.Client, this.CacheManager);
                this.SettingsViewModel = new SettingsViewModel(this.SettingsManager, null /*TODO*/);

                this.RegisterNotifications();

                this.CreateMenuItemsRegular();
            }

            Messenger.Default.Register<DialogRequestMessage>(this, this.OpenBigPopup);

            this.DialogManagerRegular = new PopupViewModel()
            {
                EasyClosePopup = new RelayCommand(this.CloseBigPopup),
                ClosePopup = new RelayCommand(this.CloseBigPopup),
                PopupDialog = null,
            };

            this.DialogManagerTopMost = new PopupViewModel()
            {
                EasyClosePopup = new RelayCommand(this.CloseBigTopMostPopup),
                ClosePopup = new RelayCommand(this.CloseBigTopMostPopup),
                PopupDialog = null,
            };

            //Wpf.Native.RecoveryManager.RegisterForRecovery();
            //Wpf.Native.RecoveryManager.RegisterForRestart();
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

            var secondTab = new HamburgerMenuItem()
            {
                Icon = new IconControl() { BindableKind = PackIconMaterialKind.EmailSearch },
                Label = "Search",
                ToolTip = "Search all Groups and Chats.",
                Tag = this.SearchViewModel,
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
            this.MenuItems.Add(secondTab);

            // Add new Options
            this.MenuOptionItems.Add(settingsTab);

            // Set the section to the Chats tab
            this.SelectedItem = chatsTab;

            // Enable the refresh button
            this.RefreshEverythingCommand = new RelayCommand(async () => await this.RefreshEverything(), true);
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
            /*if (!this.UpdateAssist.CanShutdown)
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
            }*/
        }

        private void OpenBigPopup(DialogRequestMessage dialog)
        {
            if (dialog.TopMost)
            {
                this.DialogManagerTopMost.PopupDialog = dialog.Dialog;
            }
            else
            {
                this.DialogManagerRegular.PopupDialog = dialog.Dialog;
            }
        }

        private void CloseBigPopup()
        {
            if (this.DialogManagerRegular.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.DialogManagerRegular.PopupDialog = null;
        }

        private void CloseBigTopMostPopup()
        {
            if (this.DialogManagerTopMost.PopupDialog is IDisposable d)
            {
                d.Dispose();
            }

            this.DialogManagerTopMost.PopupDialog = null;
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

        private void IndexAndRunCommand(GroupMeClient.Core.Messaging.RunPluginRequestMessage cmd)
        {
            this.SearchViewModel.RunPlugin(cmd.MessageContainer, cmd.Plugin);
        }

        private void SwitchToPageCommand(GroupMeClient.Core.Messaging.SwitchToPageRequestMessage cmd)
        {
            ViewModelBase selectedPage = null;

            switch (cmd.SelectedPage)
            {
                case GroupMeClient.Core.Messaging.SwitchToPageRequestMessage.Page.Chats:
                    selectedPage = this.ChatsViewModel;
                    break;
                case GroupMeClient.Core.Messaging.SwitchToPageRequestMessage.Page.Search:
                    selectedPage = this.SearchViewModel;
                    break;
                case GroupMeClient.Core.Messaging.SwitchToPageRequestMessage.Page.Settings:
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
            var restoreService = GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<IRestoreService>();
            restoreService.SoftApplicationRestart();
        }
    }
}
