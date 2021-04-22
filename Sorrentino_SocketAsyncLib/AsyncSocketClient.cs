using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sorrentino_SocketAsyncLib
{
    public class AsyncSocketClient
    {
        IPAddress mServerIpAddress;
        int mServerPort;
        TcpClient mClient;

        public List<string> Messages = new List<string>();

        public event EventHandler OnNewMessage;
        protected virtual void OnNewMessageHandler(EventArgs e)
        {
            EventHandler handler = OnNewMessage;
            handler?.Invoke(this, e);
        }

        public IPAddress ServerIpAddress
        {
            get
            {
                return mServerIpAddress;
            }
        }

        public int ServerPort
        {
            get
            {
                return mServerPort;
            }
        }

        public bool SetServerIpAddress(string str_ipaddr)
        {
            IPAddress ipaddr = null;
            if (!IPAddress.TryParse(str_ipaddr, out ipaddr))
            {
                Console.WriteLine("IP non valido");
                return false;
            }
            mServerIpAddress = ipaddr;
            return true;
        }

        public bool SetServerPort(string str_port)
        {
            int port = -1;
            if (!int.TryParse(str_port, out port))
            {
                Console.WriteLine("Porta non valida");
                return false;
            }

            if (port < 0 || port > 65535)
            {
                Console.WriteLine("La porta deve essere compresa tra 0 e 65535");
                return false;
            }

            mServerPort = port;
            return true;
        }

        public async Task ServerConnection()
        {
            if (mClient == null) 
            {
                mClient = new TcpClient();
            }

            try
            {
                await mClient.ConnectAsync(mServerIpAddress, mServerPort);
                Console.WriteLine($"Connesso al server IP: {mServerIpAddress.ToString()} PORT: {mServerPort}");
                ReceiveMessage();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ReceiveMessage()
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = mClient.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[512];

                while (true)
                {
                    int nBytes = await reader.ReadAsync(buff, 0, buff.Length);
                    if (nBytes == 0)
                    {
                        Console.WriteLine("Client disconnesso.");
                        break;
                    }

                    string recvMessage = new string(buff, 0, nBytes);
                    //Aggiungo il messaggio alla lista
                    Messages.Add(recvMessage);
                    EventArgs e = new EventArgs();
                    OnNewMessageHandler(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendMessage(string messaggio)
        {
            if (mClient == null)
            {
                return;
            }

            if (!mClient.Connected)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(messaggio))
                {
                    return;
                }
                byte[] buff = Encoding.ASCII.GetBytes(messaggio);
                mClient.GetStream().WriteAsync(buff, 0, buff.Length);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        public AsyncSocketClient()
        {
            mServerIpAddress = null;
            mServerPort = -1;
            mClient = null;
        }

    }
}
