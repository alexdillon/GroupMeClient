using GalaSoft.MvvmLight;
using GroupMeClient.Notifications.Display;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;

namespace GroupMeClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private HamburgerMenuItemCollection menuItems;

        private HamburgerMenuItemCollection menuOptionItems;

        public MainViewModel()
        {
            InitializeGroupMeClient();

            ChatsViewModel = new ChatsViewModel(GroupMeClient);
            SecondViewModel = new SecondViewModel();
            SettingsViewModel = new SettingsViewModel();

            RegisterNotifications();

            this.CreateMenuItems();
        }

        private static GroupMeClientCached.GroupMeCachedClient GroupMeClient { get; set; }

        private static NotificationRouter NotificationRouter { get; set; }

        /// <summary>
        /// Gets or sets a static instance of one of the Chat's ViewModel.
        /// </summary>
        private static ChatsViewModel ChatsViewModel { get; set; }

        /// <summary>
        /// Gets or sets a static instance of one of the **** TODO***** ViewModel.
        /// </summary>
        private static SecondViewModel SecondViewModel { get; set; }

        /// <summary>
        /// Gets or sets a static instance of one of the Settings ViewModel.
        /// </summary>
        private static SettingsViewModel SettingsViewModel { get; set; }

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

        private void CreateMenuItems()
        {
            this.MenuItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.MessageText },
                    Label = "Chats",
                    ToolTip = "View Groups and Chats.",
                    Tag = ChatsViewModel,
                },
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.Settings },
                    Label = "Second Menu",
                    ToolTip = "The Application settings.",
                    Tag = SecondViewModel,
                },
            };

            this.MenuOptionItems = new HamburgerMenuItemCollection
            {
                new HamburgerMenuIconItem()
                {
                    Icon = new PackIconMaterial() { Kind = PackIconMaterialKind.SettingsOutline },
                    Label = "Settings",
                    ToolTip = "GroupMe Settings",
                    Tag = SettingsViewModel,
                },
            };
        }

        private static void InitializeGroupMeClient()
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            GroupMeClient = new GroupMeClientCached.GroupMeCachedClient(token, "cache.db");
            NotificationRouter = new NotificationRouter(GroupMeClient);
        }

        private static void RegisterNotifications()
        {
            NotificationRouter.RegisterNewSubscriber(ChatsViewModel);
            NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreatePlatformNotificationProvider());
            NotificationRouter.RegisterNewSubscriber(PopupNotificationProvider.CreateInternalNotificationProvider());
            // TODO register windows notifications
        }
    }
}