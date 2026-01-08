using System;

namespace P2PMessenger.Core
{
    public class Message
    {
        public string Sender { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public Message(string sender, string text)
        {
            Sender = sender;
            Text = text;
            Timestamp = DateTime.Now;
        }

        // Преобразуем сообщение в строку для отправки по TCP
        public string Serialize()
        {
            return $"{Timestamp:yyyy-MM-dd HH:mm:ss}|{Sender}|{Text}";
        }

        // Восстанавливаем сообщение из строки
        public static Message Deserialize(string data)
        {
            var parts = data.Split('|', 3);

            return new Message(parts[1], parts[2])
            {
                Timestamp = DateTime.Parse(parts[0])
            };
        }
    }
}
