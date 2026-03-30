using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CueTime.Classes.NonAbstract;

namespace CueTime.Classes.Utilities
{
    public static class GestoreSalvataggi
    {
        private static readonly string cartellaSalvataggi = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CueTime",
            "AdunanzeSalvate");

        public static bool SalvaAdunanza(Adunanza dati, string nomeFile)
        {
            return SalvaAdunanza(dati, nomeFile, cartellaSalvataggi);
        }

        internal static bool SalvaAdunanza(Adunanza dati, string nomeFile, string cartellaRadice)
        {
            try
            {
                CreaCartella(cartellaRadice);
                if (!TryBuildSafeSavePath(nomeFile, cartellaRadice, out string percorsoFile))
                {
                    return false;
                }

                string json = JsonSerializer.Serialize(dati);
                File.WriteAllText(percorsoFile, json);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Errore durante il salvataggio dell'adunanza '{nomeFile}'.", ex);
                return false;
            }
        }

        public static Adunanza? CaricaAdunanza(string nomeFile)
        {
            return CaricaAdunanza(nomeFile, cartellaSalvataggi);
        }

        internal static Adunanza? CaricaAdunanza(string nomeFile, string cartellaRadice)
        {
            try
            {
                if (!TryBuildSafeSavePath(nomeFile, cartellaRadice, out string percorsoFile))
                {
                    return null;
                }

                if (!File.Exists(percorsoFile))
                {
                    return null;
                }

                string json = File.ReadAllText(percorsoFile);
                return JsonSerializer.Deserialize<Adunanza>(json);
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Errore durante il caricamento dell'adunanza '{nomeFile}'.", ex);
                return null;
            }
        }

        public static List<string> OttieniListaSalvataggi()
        {
            return OttieniListaSalvataggi(cartellaSalvataggi);
        }

        internal static List<string> OttieniListaSalvataggi(string cartellaRadice)
        {
            List<string> lista = new List<string>();

            if (!Directory.Exists(cartellaRadice))
            {
                return lista;
            }

            string[] files = Directory.GetFiles(cartellaRadice, "*.json");

            foreach (string file in files)
            {
                lista.Add(Path.GetFileNameWithoutExtension(file));
            }

            return lista;
        }

        public static void CreaCartella()
        {
            CreaCartella(cartellaSalvataggi);
        }

        internal static void CreaCartella(string cartellaRadice)
        {
            if (!Directory.Exists(cartellaRadice))
            {
                Directory.CreateDirectory(cartellaRadice);
            }
        }

        public static bool EliminaAdunanza(string nomeFile)
        {
            return EliminaAdunanza(nomeFile, cartellaSalvataggi);
        }

        internal static bool EliminaAdunanza(string nomeFile, string cartellaRadice)
        {
            try
            {
                if (!TryBuildSafeSavePath(nomeFile, cartellaRadice, out string percorsoFile))
                {
                    return false;
                }

                if (File.Exists(percorsoFile))
                {
                    File.Delete(percorsoFile);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Errore durante l'eliminazione dell'adunanza '{nomeFile}'.", ex);
                return false;
            }
        }

        public static string GetSaveDirectoryPath() => cartellaSalvataggi;

        private static bool TryBuildSafeSavePath(string nomeFile, string cartellaRadice, out string percorsoFile)
        {
            percorsoFile = string.Empty;

            if (string.IsNullOrWhiteSpace(nomeFile))
            {
                return false;
            }

            string trimmed = nomeFile.Trim();
            string baseName = Path.GetFileNameWithoutExtension(trimmed);

            if (!string.Equals(trimmed, baseName, StringComparison.Ordinal))
            {
                return false;
            }

            if (baseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }

            string root = Path.GetFullPath(cartellaRadice);
            string candidate = Path.GetFullPath(Path.Combine(root, $"{baseName}.json"));
            string rootWithSep = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;

            if (!candidate.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            percorsoFile = candidate;
            return true;
        }
    }
}

