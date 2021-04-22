using Sorrentino_SocketAsyncLib;
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
using System.Windows.Shapes;

namespace Sorrentino_Chat_Client
{
    /// <summary>
    /// Logica di interazione per Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        AsyncSocketClient Client;
        public Chat(AsyncSocketClient client)
        {
            InitializeComponent();
            Client = client;
            Client.OnNewMessage += Client_OnNewMessage;
        }

        private void Client_OnNewMessage(object sender, EventArgs e)
        {
            lst_Chat.ItemsSource = Client.Messages;
            lst_Chat.Items.Refresh();
        }

        private void btn_Invia_Click(object sender, RoutedEventArgs e)
        {
            Client.SendMessage(txt_Messaggio.Text);
        }
    }
}
