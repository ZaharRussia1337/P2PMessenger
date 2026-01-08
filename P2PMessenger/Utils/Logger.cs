using System;
using System.IO;

namespace P2PMessenger.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath = "errors.log";

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(
                    LogFilePath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR {message}{Environment.NewLine}"
                );
            }
            catch
            {
                // если даже логгер сдох — ну всё, пиздец, молимся
            }
        }

        public static void Log(Exception ex)
        {
            Log(ex.ToString());
        }
    }
}
