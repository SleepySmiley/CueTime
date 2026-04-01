using System.Windows;

namespace InTempo.Classes.Utilities.Monitors
{
    public class Monitor
    {
        public string Nome { get; set; } = string.Empty;
        public bool EPrimario { get; set; }

        public Rect AreaTotale { get; set; }

        public Rect AreaDiLavoro { get; set; }

        public Monitor() { }

        public Monitor(string nome, bool ePrimario, Rect areaTotale, Rect areaDiLavoro)
        {
            Nome = nome;
            EPrimario = ePrimario;
            AreaTotale = areaTotale;
            AreaDiLavoro = areaDiLavoro;
        }

        public Monitor Clone()
        {
            return new Monitor(Nome, EPrimario, AreaTotale, AreaDiLavoro);
        }
    }
}
