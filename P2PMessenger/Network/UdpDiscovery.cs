using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using P2PMessenger.Utils;

namespace P2PMessenger.Network
{
    public class UdpDiscovery
    {
        private readonly int _udpPort;
        private readonly int _tcpPort;
        private readonly string _broadcastAddress;
        private readonly HashSet<string> _nodes;
        private bool _isRunning;

        public UdpDiscovery(int udpPort, int tcpPort, string broadcastAddress, HashSet<string> nodes)
        {
            _udpPort = udpPort;
            _tcpPort = tcpPort;
            _broadcastAddress = broadcastAddress;
            _nodes = nodes;
        }

        public void Start()
        {
            _isRunning = true;

            Thread discoveryThread = new Thread(DiscoveryLoop);
            discoveryThread.IsBackground = true;
            discoveryThread.Start();
        }

        private void DiscoveryLoop()
        {
            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                socket.Bind(new IPEndPoint(IPAddress.Any, _udpPort));

                EndPoint broadcastEP = new IPEndPoint(
                    IPAddress.Parse(_broadcastAddress),
                    _udpPort
                );

                byte[] buffer = new byte[1024];

                while (_isRunning)
                {
                    // --- SEND ---
                    string msg = $"DISCOVER:{_tcpPort}";
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    socket.SendTo(data, broadcastEP);

                    // --- RECEIVE ---
                    socket.ReceiveTimeout = 2000;
                    try
                    {
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        int size = socket.ReceiveFrom(buffer, ref remoteEP);

                        string text = Encoding.UTF8.GetString(buffer, 0, size);

                        if (text.StartsWith("DISCOVER"))
                        {
                            int port = int.Parse(text.Split(':')[1]);
                            var ip = ((IPEndPoint)remoteEP).Address.ToString();

                            // ❗ НЕ добавляем самого себя
                            if (port != _tcpPort)
                            {
                                lock (_nodes)
                                {
                                    _nodes.Add($"{ip}:{port}");
                                }
                            }
                        }
                    }
                    catch (SocketException)
                    {
                        // таймаут — это нормально, блять
                    }

                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
