using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GroupMeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string token = System.IO.File.ReadAllText("../../../DevToken.txt");
            var groupMeClient = new LibGroupMe.GroupMeClient(token);

            var groups = await groupMeClient.GetGroupsAsync();
            var messagesInFirstGroup = await groups[0].GetMessagesAsync();

            var chats = await groupMeClient.GetChatsAsync();
            var messagesInFirstChat = await chats[0].GetMessagesAsync();
        }
    }
}
