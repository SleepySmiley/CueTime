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
            logicaTimer = TimerLogic;
            OrologioCopia = TempoCopia;
            this.DataContext = logicaTimer;
            ContenitorePrincipale.Content = new VistaOrologio();
            ApplicaMonitorScelto();
                    
        }

        public void ApplicaMonitorScelto()
        {
            var monitor = App.Settings.MonitorScelto;

            if (monitor != null)
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = monitor.AreaTotale.Left;
                this.Top = monitor.AreaTotale.Top;
                this.Width = monitor.AreaTotale.Width;
                this.Height = monitor.AreaTotale.Height;
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
