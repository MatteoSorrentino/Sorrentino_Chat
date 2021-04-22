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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sorrentino_Chat_Client
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AsyncSocketClient client;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            client = new AsyncSocketClient();
            client.SetServerIpAddress(txt_UserIP.Text);
            client.SetServerPort(txt_UserPort.Text);
            client.ServerConnection();
            client.SendMessage(txt_Nickname.Text);

            Window wnd_chat = new Chat(client);
            wnd_chat.Show();
            this.Close();
        }
    }
}
