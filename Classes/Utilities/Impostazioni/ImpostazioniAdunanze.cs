using InTempo.Classes.Utilities.Monitors;

namespace InTempo.Classes.Utilities.Impostazioni
{
    public class ImpostazioniAdunanze
    {
        public OrarioAdunanza Infrasettimanale { get; set; } = new OrarioAdunanza();
        public OrarioAdunanza FineSettimana { get; set; } = new OrarioAdunanza();

        public Monitors.Monitor MonitorScelto { get; set; } = new Monitors.Monitor();

        public ImpostazioniAdunanze()
        {
            Infrasettimanale.OraInizio = new DateTime(1, 1, 1, 20, 0, 0); // Default: 20:00
            Infrasettimanale.GiornoSettimana = DayOfWeek.Wednesday; // Default: Mercoledì
            FineSettimana.OraInizio = new DateTime(1, 1, 1, 10, 0, 0); // Default: 10:00
            FineSettimana.GiornoSettimana = DayOfWeek.Sunday; // Default: Domenica
            MonitorScelto = new Monitors.Monitor
            {
                Nome = "Default",
                EPrimario = true,
                AreaTotale = new System.Windows.Rect(0, 0, 1920, 1080),
                AreaDiLavoro = new System.Windows.Rect(0, 0, 1920, 1040)
            };
        }
    }
}
