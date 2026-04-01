using System;

namespace InTempo.Classes.Utilities
{
    public class OrarioAdunanza
    {
        public DayOfWeek GiornoSettimana { get; set; }

        public DateTime OraInizio { get; set; }

        public OrarioAdunanza() { }

        public OrarioAdunanza(DayOfWeek giorno, int ore, int minuti)
        {
            GiornoSettimana = giorno;
            OraInizio = new DateTime(1, 1, 1, ore, minuti, 0);
        }

        public OrarioAdunanza Clone()
        {
            return new OrarioAdunanza
            {
                GiornoSettimana = GiornoSettimana,
                OraInizio = OraInizio
            };
        }

        public DateTime GetProssimaData()
        {
            DateTime oggi = DateTime.Now;
            int giorniMancanti = ((int)GiornoSettimana - (int)oggi.DayOfWeek + 7) % 7;

            if (giorniMancanti == 0 && oggi.TimeOfDay > OraInizio.TimeOfDay)
            {
                giorniMancanti = 7;
            }

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
