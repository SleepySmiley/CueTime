using InTempo.Classes.Utilities;
using InTempo.Classes.View.UserControls;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;

namespace InTempo.Classes.View
{
    public partial class FinestraTimer : Window
    {

        public DispatcherTimer OrologioCopia { get; set; }

        public TimerLogics logicaTimer { get; set; }


        public FinestraTimer(DispatcherTimer TempoCopia, TimerLogics TimerLogic)
        {
            InitializeComponent();
            PosizionaSulMonitorScelto();
            logicaTimer = TimerLogic;
            OrologioCopia = TempoCopia;
            this.DataContext = logicaTimer;
            ContenitorePrincipale.Content = new VistaOrologio();
                   
        }

        private void PosizionaSulMonitorScelto()
        {
            // Recupera il monitor che hai salvato in precedenza
            // (Assicurati che il percorso di App.Settings sia quello corretto nel tuo progetto)
            var monitor = App.Settings.MonitorScelto;

            if (monitor != null)
            {
                // 1. OBBLIGATORIO: Diciamo a WPF che posizioneremo la finestra a mano
                this.WindowStartupLocation = WindowStartupLocation.Manual;

                // 2. Impostiamo coordinate e dimensioni
                // Nota: Usa "AreaTotale" se vuoi che copra tutto lo schermo (barra di Windows inclusa)
                // Usa "AreaDiLavoro" se vuoi che la barra di Windows rimanga visibile
                this.Left = monitor.AreaTotale.Left;
                this.Top = monitor.AreaTotale.Top;
                this.Width = monitor.AreaTotale.Width;
                this.Height = monitor.AreaTotale.Height;

                // 3. OPZIONALE: Se vuoi che sia un vero "Schermo Intero" senza bordi e senza X in alto a destra
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
            }
        }

        // Aggiungi questo metodo dentro FinestraTimer
        public void CambiaVista(int tipoVista, string testoPersonalizzato, System.Windows.Media.Brush colorescritta)
        {
            switch (tipoVista)
            {
                case 1:
                    ContenitorePrincipale.Content = new VistaSoloTimer(); 
                    break;
                case 2:
                    ContenitorePrincipale.Content = new VistaMista(testoPersonalizzato, logicaTimer);
                    break;
                case 3:
                    ContenitorePrincipale.Content = new VistaSoloScritta(testoPersonalizzato, colorescritta);
                    break;
                case 4:
                    ContenitorePrincipale.Content = new VistaOrologio();
                    break;
            }
            logicaTimer.AggiornaGrafica();
        }


    }
}