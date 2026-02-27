using System;
using InTempo.Classes.Utilities.Monitors;

namespace InTempo.Classes.Utilities.Impostazioni
{
    public class ImpostazioniAdunanze
    {
        public OrarioAdunanza Infrasettimanale { get; set; } = new OrarioAdunanza();
        public OrarioAdunanza FineSettimana { get; set; } = new OrarioAdunanza();

        public Monitors.Monitor MonitorScelto { get; set; } = new Monitors.Monitor();

        public string PercorsoCartellaMusica { get; set; } = string.Empty;

        public DateTime[] DateVisitaSorvegliante { get; set; } = new DateTime[2];

        public ImpostazioniAdunanze()
        {
            Infrasettimanale.OraInizio = new DateTime(1, 1, 1, 20, 0, 0);
            Infrasettimanale.GiornoSettimana = DayOfWeek.Wednesday;
            FineSettimana.OraInizio = new DateTime(1, 1, 1, 10, 0, 0);
            FineSettimana.GiornoSettimana = DayOfWeek.Sunday;
            MonitorScelto = new Monitors.Monitor
            {
                Nome = "Default",
                EPrimario = true,
                AreaTotale = new System.Windows.Rect(0, 0, 1920, 1080),
                AreaDiLavoro = new System.Windows.Rect(0, 0, 1920, 1040)
            };
            DateVisitaSorvegliante[0] = DateTime.Now;
            DateVisitaSorvegliante[1] = DateTime.Now;
        }
    }
}