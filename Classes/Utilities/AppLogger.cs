using System;
using System.Diagnostics;
using System.IO;

namespace InTempo.Classes.Utilities
{
    internal static class AppLogger
    {
        private static readonly object SyncRoot = new object();
        private static readonly string LogDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InTempo",
            "logs");

        private static readonly string LogFilePath = Path.Combine(LogDirectoryPath, "app.log");

        public static string GetLogDirectory() => LogDirectoryPath;

        public static string GetLogFilePath() => LogFilePath;

        public static void LogInfo(string message)
        {
            Write("INFO", message);
        }

        public static void LogWarning(string message, Exception? exception = null)
        {
            Write("WARN", message, exception);
        }

        public static void LogError(string message, Exception? exception = null)
        {
            Write("ERROR", message, exception);
        }

        private static void Write(string level, string message, Exception? exception = null)
        {
            string safeMessage = string.IsNullOrWhiteSpace(message) ? "Messaggio di log assente." : message.Trim();
            string entry =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {safeMessage}{Environment.NewLine}{exception}";

            Debug.WriteLine(entry);

            try
            {
                lock (SyncRoot)
                {
                    Directory.CreateDirectory(LogDirectoryPath);
                    File.AppendAllText(LogFilePath, entry + Environment.NewLine + Environment.NewLine);
                }
            }
            catch (Exception logException)
            {
                Debug.WriteLine($"[LOGGER] Impossibile scrivere il log su file: {logException}");
            }
        }
    }
}
