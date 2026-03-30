using System;
using System.IO;
using System.Text.Json;
using CueTime.Classes.Utilities;

namespace CueTime.Classes.Utilities.Impostazioni
{
    public static class SettingsStore
    {
        private static readonly string AppName =
            "CueTime";

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
            return LoadFromPath(FilePath);
        }

        public static ImpostazioniAdunanze LoadFromPath(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new ImpostazioniAdunanze();
                }

                string json = File.ReadAllText(filePath);
                ImpostazioniAdunanze settings = JsonSerializer.Deserialize<ImpostazioniAdunanze>(json, JsonOptions)
                    ?? new ImpostazioniAdunanze();

                settings.Normalizza();
                return settings;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere le impostazioni da '{filePath}'. Verra creato un backup se possibile.", ex);
                BackupUnreadableSettingsFile(filePath);
                return new ImpostazioniAdunanze();
            }
        }

        public static void Save(ImpostazioniAdunanze settings)
        {
            SaveToPath(settings, FilePath);
        }

        public static void SaveToPath(ImpostazioniAdunanze settings, string filePath)
        {
            string? folderPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new InvalidOperationException("Il percorso file delle impostazioni non e valido.");
            }

            Directory.CreateDirectory(folderPath);

            settings ??= new ImpostazioniAdunanze();
            settings.Normalizza();

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            string tmp = filePath + ".tmp";
            File.WriteAllText(tmp, json);

            if (File.Exists(filePath))
            {
                File.Replace(tmp, filePath, null);
            }
            else
            {
                File.Move(tmp, filePath);
            }
        }

        public static string GetSettingsPath() => FilePath;

        public static string GetSettingsDirectoryPath() => FolderPath;

        private static void BackupUnreadableSettingsFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                string? folderPath = Path.GetDirectoryName(filePath);
                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    return;
                }

                Directory.CreateDirectory(folderPath);

                string backupName = $"settings.invalid-{DateTime.Now:yyyyMMdd-HHmmss}.json";
                string backupPath = Path.Combine(folderPath, backupName);

                if (!File.Exists(backupPath))
                {
                    File.Copy(filePath, backupPath, overwrite: false);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile creare il backup del file impostazioni '{filePath}'.", ex);
            }
        }
    }
}

