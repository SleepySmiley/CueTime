using System.Runtime.InteropServices;
using System.Windows;

namespace CueTime.Classes.Utilities.Monitors
{



    public static class GestoreMonitor
    {
        // =========================================================
        // SEZIONE A: Le "Mappe" per tradurre i dati da Windows a C#
        // =========================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Sinistra, Alto, Destra, Basso;
            public int Larghezza => Destra - Sinistra;
            public int Altezza => Basso - Alto;
        }

        // Questa struttura deve corrispondere ESATTAMENTE a quella di Windows
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MONITORINFOEX
        {
            public int dimensioneStruttura; // Serve a Windows per capire la versione
            public RECT areaMonitor;
            public RECT areaLavoro;
            public int flag;                // Contiene info se è il monitor principale

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string nomeDispositivo;
        }

        // =========================================================
        // SEZIONE B: Le chiavi per aprire le porte di Windows (DLL)
        // =========================================================

        // Funzione che "cerca" tutti i monitor
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, DelegatoMonitor lpfnEnum, IntPtr dwData);

        // Funzione che estrae i dettagli di un singolo monitor trovato
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        // Il "citofono" che Windows usa per avvisarci ogni volta che trova un monitor
        private delegate bool DelegatoMonitor(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        // =========================================================
        // SEZIONE C: Il Metodo Pubblico (Quello che chiamerai tu)
        // =========================================================

        public static List<Monitor> OttieniTuttiIMonitor()
        {
            var listaMonitor = new List<Monitor>();

            // Creiamo il metodo che Windows chiamerà per ogni monitor
            DelegatoMonitor callback = (IntPtr hMonitor, IntPtr hdc, ref RECT rect, IntPtr data) =>
            {
                // Prepariamo il "contenitore" vuoto per i dettagli
                MONITORINFOEX info = new MONITORINFOEX();
                info.dimensioneStruttura = Marshal.SizeOf(typeof(MONITORINFOEX)); // Obbligatorio per Windows

                // Chiediamo a Windows di riempire il contenitore
                if (GetMonitorInfo(hMonitor, ref info))
                {
                    // Traduciamo i dati di Windows nel nostro oggetto C# pulito
                    listaMonitor.Add(new Monitor
                    {
                        Nome = info.nomeDispositivo,
                        EPrimario = (info.flag & 1) != 0, // 1 significa che è il principale

                        AreaTotale = new Rect(info.areaMonitor.Sinistra, info.areaMonitor.Alto, info.areaMonitor.Larghezza, info.areaMonitor.Altezza),
                        AreaDiLavoro = new Rect(info.areaLavoro.Sinistra, info.areaLavoro.Alto, info.areaLavoro.Larghezza, info.areaLavoro.Altezza)
                    });
                }

                return true; // Diciamo a Windows: "Ok, passa al prossimo monitor!"
            };

            // Facciamo partire la ricerca
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

            Monitors = listaMonitor;
            return listaMonitor;
        }

        public static List<Monitor> AggiornaDatiMonitor()
        {
            return OttieniTuttiIMonitor();
        }

        public static List<Monitor> Monitors { get; private set; } = OttieniTuttiIMonitor();
    }
}
