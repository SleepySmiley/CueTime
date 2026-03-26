using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace InTempo.Classes.Utilities.Impostazioni
{
    public static class SettingsStore
    {
        private static readonly string AppName =
            Assembly.GetEntryAssembly()?.GetName().Name ?? "InTempo";

        private static readonly string FolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

        private static readonly string FilePath =
            Path.Combine(FolderPath, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static ImpostazioniAdunanze Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return new ImpostazioniAdunanze();

                string json = File.ReadAllText(FilePath);

                var s = JsonSerializer.Deserialize<ImpostazioniAdunanze>(json, JsonOptions)
                        ?? new ImpostazioniAdunanze();

                s.Normalizza();
                return s;
            }
            catch
            {
                BackupUnreadableSettingsFile();
                return new ImpostazioniAdunanze();
            }
        }

        public static void Save(ImpostazioniAdunanze settings)
        {
            Directory.CreateDirectory(FolderPath);

            settings ??= new ImpostazioniAdunanze();
            settings.Normalizza();

            string json = JsonSerializer.Serialize(settings, JsonOptions);

            // Scrittura “atomica” (riduce il rischio di file corrotto)
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(FilePath))
                File.Replace(tmp, FilePath, null);
            else
                File.Move(tmp, FilePath);
        }

        // comodo per debug
        public static string GetSettingsPath() => FilePath;

        private static void BackupUnreadableSettingsFile()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return;

                Directory.CreateDirectory(FolderPath);

                string backupName = $"settings.invalid-{DateTime.Now:yyyyMMdd-HHmmss}.json";
                string backupPath = Path.Combine(FolderPath, backupName);

                if (!File.Exists(backupPath))
                    File.Copy(FilePath, backupPath, overwrite: false);
            }
            catch
            {
                // non bloccare l'avvio se anche il backup fallisce
            }
        }
    }
}
