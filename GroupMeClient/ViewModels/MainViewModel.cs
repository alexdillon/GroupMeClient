namespace GroupMeClient.ViewModels
{
    using GalaSoft.MvvmLight;
    using GroupMeClient.Notifications.Display;
    using MahApps.Metro.Controls;
    using MahApps.Metro.IconPacks;

    public class MainViewModel : ViewModelBase
    {
        private HamburgerMenuItemCollection menuItems;

        private HamburgerMenuItemCollection menuOptionItems;

        public MainViewModel()
        {
            InitializeGroupMeClient();

            MainViewModel.ChatsViewModel = new ChatsViewModel(MainViewModel.GroupMeClient);
            MainViewModel.SecondViewModel = new SecondViewModel();
            MainViewModel.SettingsViewModel = new SettingsViewModel();

            RegisterNotifications();

            CreateMenuItems();
        }

        static GroupMeClientCached.GroupMeCachedClient GroupMeClient { get; set; }
        static NotificationRouter NotificationRouter { get; set; }

        /// <summary>
        /// Gets a static instance of one of the Chat's ViewModel
        /// </summary>
        static ChatsViewModel ChatsViewModel { get; set; }

        /// <summary>
        /// Gets a static instance of one of the **** TODO***** ViewModel
        /// </summary>
        static SecondViewModel SecondViewModel { get; set; }

        /// <summary>
        /// Gets a static instance of one of the Settings ViewModel
        /// </summary>
        static SettingsViewModel SettingsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the list of main items shown in the hamburger menu.
        /// </summary>
        public HamburgerMenuItemCollection MenuItems
        {
            get { return menuItems; }
            set { Set(() => this.MenuItems, ref menuItems, value); }
        }

        /// <summary>
        /// Gets or sets the list of options items shown in the hamburger menu (at the bottom).
        /// </summary>
        public HamburgerMenuItemCollection MenuOptionItems
        {
            get { return menuOptionItems; }
            set { Set(() => this.MenuOptionItems, ref menuOptionItems, value); }
        }

        private void CreateMenuItems()
        {
            MenuItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.MessageText},
                    Label = "Chats",
                    ToolTip = "View Groups and Chats.",
                    Tag = MainViewModel.ChatsViewModel
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.Settings},
                    Label = "Second Menu",
                    ToolTip = "The Application settings.",
                    Tag = MainViewModel.SecondViewModel
                }
            };

            MenuOptionItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() {Kind = PackIconMaterialKind.SettingsOutline},
                    Label = "Settings",
                    ToolTip = "GroupMe Settings",
                    Tag = MainViewModel.SettingsViewModel
                }
            };
        }
        
        private static void InitializeGroupMeClient()
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            MainViewModel.GroupMeClient = new GroupMeClientCached.GroupMeCachedClient(token, "cache.db");
            MainViewModel.NotificationRouter = new NotificationRouter(MainViewModel.GroupMeClient);
        }

        private static void RegisterNotifications()
        {
            MainViewModel.NotificationRouter.RegisterNewSubscriber(MainViewModel.ChatsViewModel);
            MainViewModel.NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreatePlatformNotificationProvider());
            MainViewModel.NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreateInternalNotificationProvider());
            // TODO register windows notifications
        }
    }
}