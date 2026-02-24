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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Errore tecnico");
                return false;
            }
        }

        public static Adunanza CaricaAdunanza(string nomeFile)
        {

            return null;
        }

        public static List<string> OttieniListaSalvataggi()
        {

            return new List<string>();
        }

        public static void CreaCartella()
        {
            string percorsoCartella = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AdunanzeSalvate");

            if (!System.IO.Directory.Exists(percorsoCartella))
            {
                System.IO.Directory.CreateDirectory(percorsoCartella);
            }
        }
    }
}