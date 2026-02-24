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
        private static readonly string cartellaSalvataggi = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdunanzeSalvate");

        public static bool SalvaAdunanza(Adunanza dati, string nomeFile)
        {
            try
            {
                CreaCartella();
                string percorsoFile = Path.Combine(cartellaSalvataggi, $"{nomeFile}.json");
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
                string percorsoFile = Path.Combine(cartellaSalvataggi, $"{nomeFile}.json");

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
            string percorsoCartella = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdunanzeSalvate");

            if (!Directory.Exists(percorsoCartella))
            {
                Directory.CreateDirectory(percorsoCartella);
            }
        }

        public static bool EliminaAdunanza(string nomeFile)
        {
            try
            {
                string percorsoFile = Path.Combine(cartellaSalvataggi, $"{nomeFile}.json");
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
    }
}