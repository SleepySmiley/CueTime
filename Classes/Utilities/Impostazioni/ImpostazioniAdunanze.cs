using System;
using InTempo.Classes.Utilities.Monitors;

namespace InTempo.Classes.Utilities.Impostazioni
{
    public class ImpostazioniAdunanze
    {
        public OrarioAdunanza Infrasettimanale { get; set; } = new OrarioAdunanza();
        public OrarioAdunanza FineSettimana { get; set; } = new OrarioAdunanza();

        public Monitors.Monitor MonitorScelto { get; set; } = CreateDefaultMonitor();

        public string PercorsoCartellaMusica { get; set; } = string.Empty;

        public DateTime[] DateVisitaSorvegliante { get; set; } = CreateDefaultDateVisitaSorvegliante();

        public ImpostazioniAdunanze()
        {
            Infrasettimanale.OraInizio = new DateTime(1, 1, 1, 20, 0, 0);
            Infrasettimanale.GiornoSettimana = DayOfWeek.Wednesday;
            FineSettimana.OraInizio = new DateTime(1, 1, 1, 10, 0, 0);
            FineSettimana.GiornoSettimana = DayOfWeek.Sunday;
            MonitorScelto = CreateDefaultMonitor();
            DateVisitaSorvegliante = CreateDefaultDateVisitaSorvegliante();
        }

        public void Normalizza()
        {
            Infrasettimanale ??= new OrarioAdunanza
            {
                OraInizio = new DateTime(1, 1, 1, 20, 0, 0),
                GiornoSettimana = DayOfWeek.Wednesday
            };

            FineSettimana ??= new OrarioAdunanza
            {
                OraInizio = new DateTime(1, 1, 1, 10, 0, 0),
                GiornoSettimana = DayOfWeek.Sunday
            };

            if (MonitorScelto == null || string.IsNullOrWhiteSpace(MonitorScelto.Nome))
            {
                MonitorScelto = CreateDefaultMonitor();
            }

            PercorsoCartellaMusica ??= string.Empty;

            if (DateVisitaSorvegliante == null || DateVisitaSorvegliante.Length < 2)
            {
                DateVisitaSorvegliante = CreateDefaultDateVisitaSorvegliante();
            }
        }

        public static bool IsDataVisitaValida(DateTime data)
        {
            return data > DateTime.MinValue;
        }

        public static DateTime[] CreateDefaultDateVisitaSorvegliante()
        {
            return new[] { DateTime.MinValue, DateTime.MinValue };
        }

        public static Monitors.Monitor CreateDefaultMonitor()
        {
            return new Monitors.Monitor
            {
                Nome = "Default",
                EPrimario = true,
                AreaTotale = new System.Windows.Rect(0, 0, 1920, 1080),
                AreaDiLavoro = new System.Windows.Rect(0, 0, 1920, 1040)
            };
        }
    }
}
