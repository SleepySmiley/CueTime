using InTempo.Classes.NonAbstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace InTempo.Classes.Utilities
{
    public static class GestoreSalvataggi
    {
        private static readonly string cartellaSalvataggi = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InTempo",
            "AdunanzeSalvate");

        public static bool SalvaAdunanza(Adunanza dati, string nomeFile)
        {
            try
            {
                CreaCartella();
                if (!TryBuildSafeSavePath(nomeFile, out string percorsoFile))
                {
                    return false;
                }

                string json = JsonSerializer.Serialize(dati);
                File.WriteAllText(percorsoFile, json);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Adunanza? CaricaAdunanza(string nomeFile)
        {
            try
            {
                if (!TryBuildSafeSavePath(nomeFile, out string percorsoFile))
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
            catch
            {
                return null;
            }
        }

        public static List<string> OttieniListaSalvataggi()
        {
            List<string> lista = new List<string>();

            if (!Directory.Exists(cartellaSalvataggi))
            {
                return lista;
            }

            string[] files = Directory.GetFiles(cartellaSalvataggi, "*.json");

            foreach (string file in files)
            {
                lista.Add(Path.GetFileNameWithoutExtension(file));
            }

            return lista;
        }

        public static void CreaCartella()
        {
            if (!Directory.Exists(cartellaSalvataggi))
            {
                Directory.CreateDirectory(cartellaSalvataggi);
            }
        }

        public static bool EliminaAdunanza(string nomeFile)
        {
            try
            {
                if (!TryBuildSafeSavePath(nomeFile, out string percorsoFile))
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
            catch
            {
                return false;
            }
        }

        private static bool TryBuildSafeSavePath(string nomeFile, out string percorsoFile)
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

            string root = Path.GetFullPath(cartellaSalvataggi);
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
