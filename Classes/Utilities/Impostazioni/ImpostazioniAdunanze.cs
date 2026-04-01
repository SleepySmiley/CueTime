using System;
using System.Linq;
using InTempo.Classes.Utilities.Monitors;
using InTempo.Classes.Utilities.Theming;

namespace InTempo.Classes.Utilities.Impostazioni
{
    public class ImpostazioniAdunanze
    {
        public OrarioAdunanza Infrasettimanale { get; set; } = new OrarioAdunanza();
        public OrarioAdunanza FineSettimana { get; set; } = new OrarioAdunanza();

        public Monitors.Monitor MonitorScelto { get; set; } = CreateDefaultMonitor();

        public string PercorsoCartellaMusica { get; set; } = string.Empty;

        public DateTime[] DateVisitaSorvegliante { get; set; } = CreateDefaultDateVisitaSorvegliante();

        public string TemaSelezionato { get; set; } = ThemeManager.DefaultThemeKey;

        public CustomThemePalette TemaPersonalizzato { get; set; } = ThemeManager.CreateDefaultCustomTheme();

        public ImpostazioniAdunanze()
        {
            Infrasettimanale.OraInizio = new DateTime(1, 1, 1, 20, 0, 0);
            Infrasettimanale.GiornoSettimana = DayOfWeek.Wednesday;
            FineSettimana.OraInizio = new DateTime(1, 1, 1, 10, 0, 0);
            FineSettimana.GiornoSettimana = DayOfWeek.Sunday;
            MonitorScelto = CreateDefaultMonitor();
            DateVisitaSorvegliante = CreateDefaultDateVisitaSorvegliante();
            TemaSelezionato = ThemeManager.DefaultThemeKey;
            TemaPersonalizzato = ThemeManager.CreateDefaultCustomTheme();
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

            TemaPersonalizzato ??= ThemeManager.CreateDefaultCustomTheme();
            TemaPersonalizzato.Normalizza();
            TemaSelezionato = ThemeManager.GetThemeOrDefault(TemaSelezionato, TemaPersonalizzato).Key;
        }

        public ImpostazioniAdunanze Clone()
        {
            ImpostazioniAdunanze clone = new ImpostazioniAdunanze();
            clone.CopyFrom(this);
            return clone;
        }

        public void CopyFrom(ImpostazioniAdunanze source)
        {
            source ??= new ImpostazioniAdunanze();
            source.Normalizza();

            Infrasettimanale = source.Infrasettimanale.Clone();
            FineSettimana = source.FineSettimana.Clone();
            MonitorScelto = source.MonitorScelto?.Clone() ?? CreateDefaultMonitor();
            PercorsoCartellaMusica = source.PercorsoCartellaMusica ?? string.Empty;
            DateVisitaSorvegliante = (source.DateVisitaSorvegliante ?? CreateDefaultDateVisitaSorvegliante())
                .Select(data => data)
                .ToArray();
            TemaPersonalizzato = source.TemaPersonalizzato?.Clone() ?? ThemeManager.CreateDefaultCustomTheme();
            TemaPersonalizzato.Normalizza();
            TemaSelezionato = ThemeManager.GetThemeOrDefault(source.TemaSelezionato, TemaPersonalizzato).Key;
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
