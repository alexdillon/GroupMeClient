namespace GroupMeClient.ViewModels
{
    using GalaSoft.MvvmLight;
    using MahApps.Metro.Controls;
    using MahApps.Metro.IconPacks;

    public class MainViewModel : ViewModelBase
    {
        private HamburgerMenuItemCollection menuItems;

        private HamburgerMenuItemCollection menuOptionItems;

        public MainViewModel()
        {
            CreateMenuItems();
        }

        /// <summary>
        /// Gets a static instance of one of the Chat's ViewModel
        /// </summary>
        static ChatsViewModel ChatsViewModel { get; } = new ChatsViewModel();

        /// <summary>
        /// Gets a static instance of one of the **** TODO***** ViewModel
        /// </summary>
        static SecondViewModel SecondViewModel { get; } = new SecondViewModel();

        /// <summary>
        /// Gets a static instance of one of the Settings ViewModel
        /// </summary>
        static SettingsViewModel SettingsViewModel { get; } = new SettingsViewModel();

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
    }
}