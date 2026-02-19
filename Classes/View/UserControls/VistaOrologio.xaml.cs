using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InTempo.Classes.View.UserControls
{
    public partial class VistaOrologio : UserControl
    {
        public VistaOrologio(DispatcherTimer OrologioCopia)
        {
            InitializeComponent();

            // 1. Scriviamo subito l'ora attuale appena si apre la vista (per non vedere vuoto nel primo secondo)
            TxtOrologio.Text = DateTime.Now.ToString("HH:mm:ss");

            // 2. Ci "agganciamo" al battito del timer che gira già nella MainWindow
            if (OrologioCopia != null)
            {
                OrologioCopia.Tick += Orologio_Tick;
            }
        }

        private void Orologio_Tick(object? sender, EventArgs e)
        {
            // 3. Ogni volta che il timer fa "Tick" (ogni secondo), aggiorniamo la scritta
            TxtOrologio.Text = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}