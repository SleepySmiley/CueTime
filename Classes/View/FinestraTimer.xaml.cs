using System.Windows;
using System.Windows.Threading;
using InTempo.Classes.Utilities;
using InTempo.Classes.Utilities.Impostazioni;
using InTempo.Classes.View.UserControls;

namespace InTempo.Classes.View
{
    public partial class FinestraTimer : Window
    {
        private readonly ImpostazioniAdunanze _settings;

        public DispatcherTimer OrologioCopia { get; set; }

        public TimerLogics logicaTimer { get; set; }

        public FinestraTimer(DispatcherTimer tempoCopia, TimerLogics timerLogic, ImpostazioniAdunanze settings)
        {
            InitializeComponent();
            logicaTimer = timerLogic;
            OrologioCopia = tempoCopia;
            _settings = settings;
            DataContext = logicaTimer;
            ContenitorePrincipale.Content = new VistaOrologio();
            ApplicaMonitorScelto();
        }

        public void ApplicaMonitorScelto()
        {
            Utilities.Monitors.Monitor? monitor = _settings.MonitorScelto;

            if (monitor != null)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = monitor.AreaTotale.Left;
                Top = monitor.AreaTotale.Top;
                Width = monitor.AreaTotale.Width;
                Height = monitor.AreaTotale.Height;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
            }
        }

        public void CambiaVista(VistaPresentazione tipoVista, string testoPersonalizzato, System.Windows.Media.Brush colorescritta)
        {
            switch (tipoVista)
            {
                case VistaPresentazione.SoloTimer:
                    ContenitorePrincipale.Content = new VistaSoloTimer();
                    break;
                case VistaPresentazione.Mista:
                    ContenitorePrincipale.Content = new VistaMista(testoPersonalizzato, logicaTimer);
                    break;
                case VistaPresentazione.SoloScritta:
                    ContenitorePrincipale.Content = new VistaSoloScritta(testoPersonalizzato, colorescritta);
                    break;
                case VistaPresentazione.Orologio:
                    ContenitorePrincipale.Content = new VistaOrologio();
                    break;
            }

            logicaTimer.AggiornaGrafica();
        }
    }
}
