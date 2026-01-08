using System;
using System.Net.Sockets;
using System.Text;
using P2PMessenger.Core;
using P2PMessenger.Utils;

namespace P2PMessenger.Network
{
    public class TcpClientNode
    {
        public static void SendMessage(string ip, int port, Message message)
        {
            try
            {
                using TcpClient client = new TcpClient();
                client.Connect(ip, port);

                using NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message.Serialize());

                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
