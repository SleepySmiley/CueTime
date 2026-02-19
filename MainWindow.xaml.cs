using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using InTempo.Classes.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace InTempo
{
    public partial class MainWindow : Window
    {
        public Adunanza DatiAdunanza { get; set; } = new Adunanza();
        public TimerLogics LogicTimer { get; set; }
        private bool _isPaused = true;

        private FinestraTimer _finestratimer;


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
            AvviaOrologio();
            LogicTimer = new TimerLogics(DatiAdunanza);
            DataContext = this;

            //Creiamo la finestra secondaria iniziando con l'orologio
            _finestratimer = new FinestraTimer(Orologio, LogicTimer);
            _finestratimer.Show();

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DatiAdunanza.SelectedAdunanza();

            Caricamento();
        }

        private void BtnAvanti_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Avanti();
            LogicTimer.AggiornaGrafica();
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Indietro();
            LogicTimer.AggiornaGrafica();
        }

        private void BtnPausaRiprendi_Click(object sender, RoutedEventArgs e)
        {
            SetStatoAdunanza(_isPaused);
        }

        public void StopOrologio()
        {
            Orologio.Stop();
            txtOrologio.Visibility = Visibility.Collapsed;
            txtTimer.Visibility = Visibility.Visible;
        }


        // --- GESTIONE MENU SEMPLIFICATA ---

        // Funzione helper per recuperare la riga cliccata
        private Parte? GetParteFromButton(object sender)
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
            if(DatiAdunanza.Parti.Count == 1)
            {
                return;
            }
            if (parteSelezionata == null)
            {
                return; 
            }
            if (parteSelezionata == DatiAdunanza.Current)
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
            else
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Classes.View.Impostazioni finestra = new Classes.View.Impostazioni();
            finestra.ShowDialog();
        }

        // Logica orologio
        DispatcherTimer Orologio {  get; set; } = new DispatcherTimer();
        private void AvviaOrologio()
        {
          
            Orologio.Interval = TimeSpan.FromSeconds(1);

            Orologio.Tick += Timer_Tick;
            txtTimer.Visibility = Visibility.Collapsed;
            txtOrologio.Visibility = Visibility.Visible;

            Orologio.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            txtOrologio.Text = now.ToString("HH:mm:ss");

            // INFRAs
            if (now.DayOfWeek == App.Settings.Infrasettimanale.GiornoSettimana &&
                now.Hour == App.Settings.Infrasettimanale.OraInizio.Hour &&
                now.Minute == App.Settings.Infrasettimanale.OraInizio.Minute)
            {
              SetStatoAdunanza(true);
            }

            // FINE SETTIMANA
            else if (now.DayOfWeek == App.Settings.FineSettimana.GiornoSettimana &&
                     now.Hour == App.Settings.FineSettimana.OraInizio.Hour &&
                     now.Minute == App.Settings.FineSettimana.OraInizio.Minute)
            {
                SetStatoAdunanza(true);
            }
        }

        // UNICO metodo che gestisce tutto: UI + orologio + timer adunanza + stato/icona
        private void SetStatoAdunanza(bool avvia)
        {
            if (avvia)
            {
                // Se è già avviata, non fare nulla
                if (!_isPaused) return;

                _isPaused = false;
                IconaStatoTimer = "Pause";

                // Passo da orologio -> timer adunanza
                Orologio.Stop();
                _finestratimer.CambiaVista(1, ""); // Passo alla vista timer
                txtOrologio.Visibility = Visibility.Collapsed;
                txtTimer.Visibility = Visibility.Visible;

                LogicTimer.StartTimer();
            }
            else
            {
                // Se è già ferma, non fare nulla
                if (_isPaused) return;

                _isPaused = true;
                IconaStatoTimer = "Play";

                // Stop = adunanza conclusa => stop + reset completo
                LogicTimer.StopTimer();
                LogicTimer.ResetCompleto();
                _finestratimer.CambiaVista(4, "");

                // Torno a orologio
                txtTimer.Visibility = Visibility.Collapsed;
                txtOrologio.Visibility = Visibility.Visible;

                Orologio.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _finestratimer.Close();
        }
    }
}