using System;
using System.Windows;
using System.Windows.Threading;

namespace InTempo.Classes.View
{
    public partial class FinestraIdentifica : Window
    {
        private DispatcherTimer _timer;

        public FinestraIdentifica(string numeroTesto)
        {
            InitializeComponent();
            TxtNumero.Text = numeroTesto;

            // Imposta un timer per chiudere la finestra dopo 3 secondi
            _timer = new DispatcherTimer(DispatcherPriority.Send);
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                this.Close();
            };
            _timer.Start();
        }
    }
}
