using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using P2PMessenger.Core;
using P2PMessenger.Network;
using P2PMessenger.Utils;

namespace P2PMessenger
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // --- Читаем конфиг ---
                var configText = File.ReadAllText("config.json");
                var config = JsonSerializer.Deserialize<Config>(configText);

                // --- Общий список узлов ---
                HashSet<string> nodes = new HashSet<string>();

                // --- Запуск TCP-сервера ---
                TcpServer server = new TcpServer(config.TcpPort);
                server.Start();

                Console.WriteLine($"TCP сервер запущен на порту {server.Port}");

                // --- Запуск UDP discovery ---
                UdpDiscovery discovery = new UdpDiscovery(
                    config.UdpPort,
                    server.Port,
                    config.BroadcastAddress,
                    nodes
                );
                discovery.Start();

                Console.WriteLine("Введите адрес узла в формате IP:PORT (или Enter для пропуска):");
                string nodeInput = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(nodeInput))
                {
                    nodes.Add(nodeInput);
                    Console.WriteLine($"Узел {nodeInput} добавлен вручную");
                }

                while (true)
                {
                    Console.Write("Введите сообщение: ");
                    string text = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    Message message = new Message("Me", text);

                    lock (nodes)
                    {
                        foreach (var node in nodes)
                        {
                            var parts = node.Split(':');
                            string ip = parts[0];
                            int port = int.Parse(parts[1]);

                            TcpClientNode.SendMessage(ip, port, message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }

    // Класс для десериализации конфига
    class Config
    {
        public int TcpPort { get; set; }
        public int UdpPort { get; set; }
        public string BroadcastAddress { get; set; }
    }
}
