using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InTempo
{
    public partial class MainWindow : Window
    {
        public Adunanza DatiAdunanza { get; set; } = new Adunanza();
        public TimerLogics LogicTimer { get; set; }
        private bool _isPaused = true;

        // Proprietà per cambiare l'icona Play/Pausa
        public string IconaStatoTimer
        {
            get { return (string)GetValue(IconaStatoTimerProperty); }
            set { SetValue(IconaStatoTimerProperty, value); }
        }
        public static readonly DependencyProperty IconaStatoTimerProperty =
            DependencyProperty.Register("IconaStatoTimer", typeof(string), typeof(MainWindow), new PropertyMetadata("Play"));

        public MainWindow()
        {
            InitializeComponent();
            LogicTimer = new TimerLogics(DatiAdunanza);
            DataContext = this;
         
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DatiAdunanza.SelectedAdunanza();

            Caricamento();
        }

        private void BtnAvanti_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Avanti();
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Indietro();
        }

        private void BtnPausaRiprendi_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused)
            {
                LogicTimer.StartTimer();
                _isPaused = false;
                IconaStatoTimer = "Pause"; // Cambio icona in Pausa
            }
            else
            {
                LogicTimer.StopTimer();
                _isPaused = true;
                IconaStatoTimer = "Play"; // Cambio icona in Play
            }
        }

        // --- GESTIONE MENU SEMPLIFICATA ---

        // Funzione helper per recuperare la riga cliccata
        private Parte GetParteFromButton(object sender)
        {
            if (sender is Button btn && btn.Tag is Parte parte)
            {
                return parte;
            }
            return null;
        }

        private void MenuItemReset_Click(object sender, RoutedEventArgs e)
        {
            var parte = GetParteFromButton(sender);
            if (parte != null) LogicTimer.ResetTimerPreciso(parte);
        }

        private void MenuItemAggiungi_Click(object sender, RoutedEventArgs e)
        {
            var parteSelezionata = GetParteFromButton(sender);
            if (parteSelezionata == null) return;

            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();

            int indice = DatiAdunanza.Parti.IndexOf(parteSelezionata);
            Classes.View.ModificaParte finestra = new Classes.View.ModificaParte();

            if (finestra.ShowDialog() == true)
            {
                DatiAdunanza.TempoResiduo -= finestra.ParteCopia.TempoParte;
                DatiAdunanza.Parti.Insert(indice + 1, finestra.ParteCopia);
            }

            if (wasRunning) LogicTimer.StartTimer();
        }

        private void MenuItemEllimina_Click(object sender, RoutedEventArgs e)
        {
            var parteSelezionata = GetParteFromButton(sender);
            if(parteSelezionata == DatiAdunanza.Current)
            {
                
                if (DatiAdunanza.Parti[0] == DatiAdunanza.Current)
                {
                   DatiAdunanza.Avanti();
                }
                else
                {
                    DatiAdunanza.Indietro();
                }
                DatiAdunanza.Parti.Remove(parteSelezionata);
                return;
            }
            if (parteSelezionata != null)
            {
                DatiAdunanza.TempoResiduo += parteSelezionata.TempoParte;
                DatiAdunanza.Parti.Remove(parteSelezionata);
            }
        }

        private void MenuItemModifica_Click(object sender, RoutedEventArgs e)
        {
            var parteSelezionata = GetParteFromButton(sender);
            if (parteSelezionata == null) return;

            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();

            Classes.View.ModificaParte finestra = new Classes.View.ModificaParte(parteSelezionata);

            if (finestra.ShowDialog() == true)
            {
                int index = DatiAdunanza.Parti.IndexOf(parteSelezionata);
                if (index != -1)
                {
                    TimeSpan differenzaTempo = finestra.ParteCopia.TempoParte - parteSelezionata.TempoParte;
                    DatiAdunanza.TempoResiduo -= differenzaTempo;

                    var target = DatiAdunanza.Parti[index];
                    target.NumeroParte = finestra.ParteCopia.NumeroParte;
                    target.NomeParte = finestra.ParteCopia.NomeParte;
                    target.TempoParte = finestra.ParteCopia.TempoParte;
                    target.TipoParte = finestra.ParteCopia.TipoParte;
                    target.ColoreParte = finestra.ParteCopia.ColoreParte;

                    if (target == DatiAdunanza.Current)
                    {
                        target.TempoScorrevole += differenzaTempo;
                    }
                    else
                    {
                        target.TempoScorrevole = finestra.ParteCopia.TempoParte;
                    }
                }
            }

            if (wasRunning) LogicTimer.StartTimer();
        }

        public void Caricamento()
        {
            if(WebPartsLoader.IsLoading)
            {
                prgbar.Visibility = Visibility.Visible;
            }
            else
            {
                prgbar.Visibility = Visibility.Collapsed;
            }
        }
    }
}