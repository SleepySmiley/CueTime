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

                // “Fix” anti-null se nel file manca qualcosa
                s.Infrasettimanale ??= new OrarioAdunanza();
                s.FineSettimana ??= new OrarioAdunanza();

                return s;
            }
            catch
            {
                return new ImpostazioniAdunanze();
            }
        }

        public static void Save(ImpostazioniAdunanze settings)
        {
            Directory.CreateDirectory(FolderPath);

            string json = JsonSerializer.Serialize(settings ?? new ImpostazioniAdunanze(), JsonOptions);

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
    }
}
