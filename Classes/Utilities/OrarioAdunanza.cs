using System;

namespace InTempo.Classes
{
    public class OrarioAdunanza
    {
        // Il giorno della settimana (Lunedì, Martedì, ecc.)
        public DayOfWeek GiornoSettimana { get; set; }

        // L'orario (usiamo DateTime perché si lega bene al TimePicker, ma ignoreremo la data)
        public DateTime OraInizio { get; set; }

        public OrarioAdunanza() { }

        // COSTRUTTORE COMODO
        public OrarioAdunanza(DayOfWeek giorno, int ore, int minuti)
        {
            GiornoSettimana = giorno;
            // Impostiamo una data fittizia (es. oggi) ma l'orario corretto
            OraInizio = new DateTime(1, 1, 1, ore, minuti, 0);
        }

        /// <summary>
        /// Calcola la data reale della PROSSIMA adunanza a partire da adesso.
        /// </summary>
        public DateTime GetProssimaData()
        {
            DateTime oggi = DateTime.Now;
            int giorniMancanti = ((int)GiornoSettimana - (int)oggi.DayOfWeek + 7) % 7;

            // Se è oggi, controlliamo se l'orario è già passato
            if (giorniMancanti == 0)
            {
                if (oggi.TimeOfDay > OraInizio.TimeOfDay)
                {
                    // Se l'orario è passato, andiamo alla prossima settimana
                    giorniMancanti = 7;
                }
            }

            // Calcolo la data finale sommando i giorni e impostando l'ora salvata
            DateTime dataProssima = oggi.AddDays(giorniMancanti);

            return new DateTime(
                dataProssima.Year,
                dataProssima.Month,
                dataProssima.Day,
                OraInizio.Hour,
                OraInizio.Minute,
                0);
        }
    }
}