using Sorrentino_SocketAsyncLib.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sorrentino_SocketAsyncLib
{
    public class AsyncSocketServer
    {
        IPAddress mIP;
        int mPort;
        TcpListener mServer;
        bool keep;

        DateTime FirstDate;

        List<ClientChat> mClients;
        public AsyncSocketServer()
        {
            mClients = new List<ClientChat>();
        }

        // Metodo che mette in ascolto il server
        public async void StartListening(IPAddress ipaddr = null, int port = 23000)
        {
            // Controlli sull'ip address e sulla porta
            if (ipaddr == null)
            {
                ipaddr = IPAddress.Any;
            }

            if (port < 0 || port > 65535)
            {
                port = 23000;
            }

            mIP = ipaddr;
            mPort = port;

            Console.WriteLine($"Avvio il server, IP: {mIP.ToString()} - Porta: {mPort.ToString()}");

            // Creo l'oggetto server
            mServer = new TcpListener(mIP, mPort);

            // Avvio del server
            mServer.Start();
            keep = true;

            while (keep)
            {
                // Mi metto in ascolto di connessioni in entrata
                TcpClient client = await mServer.AcceptTcpClientAsync();

                // Metodo per registrare i dati del client
                RegisterClient(client);

                // Ricezione di messaggi
                ReceiveMessage(client);
            }
        }

        public async void RegisterClient(TcpClient client)
        {
            // Data del primo client connesso
            if (mClients.Count == 0)
            {
                FirstDate = DateTime.Now;
            }

            // Lettura nickname
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[512];

                // Ricezione effettiva
                int nBytes = await reader.ReadAsync(buff, 0, buff.Length);
                string Nickname = new string(buff, 0, nBytes);

                ClientChat NewClient = new ClientChat();
                NewClient.Nickname = Nickname;
                NewClient.Client = client;

                // Aggiunta client alla lista
                mClients.Add(NewClient);

                Console.WriteLine($"Returned bytes {nBytes}, Messaggio: {Nickname}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"Client connessi: {mClients.Count}. Client connesso: {client.Client.RemoteEndPoint}.");
        }

        public async void ReceiveMessage(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[512];

                // Ricezione effettiva
                while (keep)
                {
                    Console.WriteLine("Pronto ad ascoltare...");
                    int nBytes = await reader.ReadAsync(buff, 0, buff.Length);
                    if (nBytes == 0)
                    {
                        RemoveClient(client);
                        Console.WriteLine("Client disconnesso.");
                        break;
                    }
                    string recvMessage = new string(buff, 0, nBytes);

                    ClientChat cc = mClients.Where(riga => riga.Client == client).FirstOrDefault();
                    string replyToSend = $"({DateTime.Now.Hour}:{DateTime.Now.Minute}) {cc.Nickname}: {recvMessage}";

                    SendToAll(replyToSend);
                    Console.WriteLine($"Returned bytes {nBytes}, Messaggio: {recvMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SendToAll(string messaggio)
        {
            try
            {
                if (string.IsNullOrEmpty(messaggio))
                {
                    return;
                }

                byte[] buff = Encoding.ASCII.GetBytes(messaggio);

                foreach (ClientChat client in mClients)
                {
                    client.Client.GetStream().WriteAsync(buff, 0, buff.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }

        public void SendToOne(string messaggio, TcpClient client)
        {
            try
            {
                if (string.IsNullOrEmpty(messaggio))
                {
                    return;
                }

                byte[] buff = Encoding.ASCII.GetBytes(messaggio);

                client.GetStream().WriteAsync(buff, 0, buff.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }

        private void RemoveClient(TcpClient client)
        {
            // LINQ è una libreria in cui posso usare "cose" peculiari del SQL su Liste<> in c#

            // LINQ in forma SQL
            //ClientChat cc = (from riga in mClients where riga.Client == client select riga).FirstOrDefault();

            // LINQ in forma c#
            ClientChat cc = mClients.Where(riga => riga.Client == client).FirstOrDefault();
            
            if (cc != null)
            {
                mClients.Remove(cc);
            }
        }

        public void CloseConnection()
        {
            try
            {
                foreach (ClientChat client in mClients)
                {
                    client.Client.Close();
                    RemoveClient(client.Client);
                }

                mServer.Stop();
                mServer = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore:" + ex.Message);
            }
        }

    }
}
