using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using P2PMessenger.Core;
using P2PMessenger.Utils;

namespace P2PMessenger.Network
{
    public class TcpServer
    {
        private TcpListener _listener;
        private bool _isRunning;

        public int Port { get; private set; }

        public TcpServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            try
            {
                _listener.Start();
                Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
                _isRunning = true;

                Thread serverThread = new Thread(ListenForClients);
                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void ListenForClients()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    Thread clientThread = new Thread(HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;

            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Message message = Message.Deserialize(data);

                Console.WriteLine(
                    $"[{message.Timestamp:HH:mm:ss}] {message.Sender}: {message.Text}"
                );
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                client.Close();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }
    }
}
