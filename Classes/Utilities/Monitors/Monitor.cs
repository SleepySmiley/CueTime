using System.Windows;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace InTempo.Classes.Utilities.Monitors
{

    // Questo è l'oggetto "pulito" che userai nel tuo codice WPF
    public class Monitor
    {
        public string Nome { get; set; } = string.Empty;
        public bool EPrimario { get; set; }

        // L'area totale dello schermo (Risoluzione)
        public Rect AreaTotale { get; set; }

        // L'area utilizzabile (Esclude la barra delle applicazioni di Windows)
        public Rect AreaDiLavoro { get; set; }

        public Monitor() { }

        public Monitor(string nome, bool ePrimario, Rect areaTotale, Rect areaDiLavoro)
        {
            Nome = nome;
            EPrimario = ePrimario;
            AreaTotale = areaTotale;
            AreaDiLavoro = areaDiLavoro;
        }

    }
}
