using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using InTempo.Classes.View;
using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
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
            SourceInitialized += MainWindow_SourceInitialized;
            AvviaOrologio();
            LogicTimer = new TimerLogics(DatiAdunanza);
            DataContext = this;

            //Creiamo la finestra secondaria iniziando con l'orologio
            _finestratimer = new FinestraTimer(Orologio, LogicTimer);
            _finestratimer.Show();

        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            HideCaptionIcon();
        }

        private void HideCaptionIcon()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero) return;

            // Rimuove l'icona dalla title bar senza mostrare la placeholder stock.
            IntPtr exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
            IntPtr newStyle = (IntPtr)(exStyle.ToInt64() | WS_EX_DLGMODALFRAME);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, newStyle);

            SetWindowPos(
                hwnd,
                IntPtr.Zero,
                0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await DatiAdunanza.SelectedAdunanza();
                RichiediCanticoInizialeSeNecessario();
            }
            catch (Exception ex)
            {
                DatiAdunanza.Parti.Clear();
                DatiAdunanza.Current = null;
                LogicTimer.AggiornaGrafica();
                LogStartupError(ex);
                FinestraPopUP errore = new FinestraPopUP(
                    "Errore caricamento",
                    "Impossibile caricare i dati dell'adunanza dal web. Controlla la connessione e riprova.",
                    1);
                errore.ShowDialog();
            }
            finally
            {
                Caricamento();
            }
        }

        private void BtnAvanti_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Avanti();
            CheckCantico();
            LogicTimer.AggiornaGrafica();
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            DatiAdunanza.Indietro();
            CheckCantico();
            LogicTimer.AggiornaGrafica();
        }

        public bool CheckCantico()
        {
            Parte? current = DatiAdunanza.Current;

            if (current?.TipoParte == "Cantico")
            {
                _finestratimer.CambiaVista(3, current.NomeParte, System.Windows.Media.Brushes.Yellow);
                btnCommentoSchermo.IsEnabled = false;
                return true;
            }
            else
            {
                _finestratimer.CambiaVista(1, "", System.Windows.Media.Brushes.White);
                btnCommentoSchermo.IsEnabled = true;
                return false;
            }
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

            bool wasRunning = TimerLogics.IsRunning;
            LogicTimer.StopTimer();

            int indice = DatiAdunanza.Parti.IndexOf(parteSelezionata);
            ModificaParte finestra = new ModificaParte();

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

            ModificaParte finestra = new ModificaParte(parteSelezionata);

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

                    target.TempoScorrevole += differenzaTempo;
                }
            }

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
            Impostazioni finestra = new Impostazioni(LogicTimer);
            finestra.ShowDialog();
        }

        // Logica orologio
        DispatcherTimer Orologio {  get; set; } = new DispatcherTimer(DispatcherPriority.Render);
        private void AvviaOrologio()
        {
          
            Orologio.Interval = TimeSpan.FromMilliseconds(100);

            Orologio.Tick += Timer_Tick;
            txtTimer.Visibility = Visibility.Collapsed;
            txtOrologio.Visibility = Visibility.Visible;
            Orologio.Start();
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            bool devePartire = LogicTimer.CalcolaStatoOrologio(now, _isPaused);

            if (devePartire)
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
                _finestratimer.CambiaVista(1, "", System.Windows.Media.Brushes.White); // Passo alla vista timer
                txtOrologio.Visibility = Visibility.Collapsed;
                txtTimer.Visibility = Visibility.Visible;
                btnCommentoSchermo.IsEnabled = true;
                LogicTimer.StartTimer();
                CheckCantico();
            }
            else
            {
                // Se è già ferma, non fare nulla
                if (_isPaused) return;
                
                FinestraPopUP Avvertimento = new FinestraPopUP("Attenzione", "Sei sicuro di voler fermare il timer? \nL'adunanza verrà conclusa e resettata.",2);
                Avvertimento.ShowDialog();
                if(Avvertimento.DialogResult != true)
                {
                    return;
                }

                _isPaused = true;
                IconaStatoTimer = "Play";

                // Stop = adunanza conclusa => stop + reset completo
                LogicTimer.StopTimer();
                LogicTimer.ResetCompleto();
                _finestratimer.CambiaVista(4, "", System.Windows.Media.Brushes.White);

                // Torno a orologio
                txtTimer.Visibility = Visibility.Collapsed;
                txtOrologio.Visibility = Visibility.Visible;
                btnCommentoSchermo.IsEnabled = false;

                Orologio.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_finestratimer != null)
            {
                _finestratimer.Close();
            }
           
            if(player != null)
            {
                player.Close();
            }
            
        }

        private void btnCommentoSchermo_Click(object sender, RoutedEventArgs e)
        {
            FinestraPopUP MessaggioOratore = new FinestraPopUP("Messaggi","Tutto Schermo","Parziale", _finestratimer);
            MessaggioOratore.ShowDialog();
        }
        PlayerMusicale player = new PlayerMusicale();
        private void btnMusica_Click(object sender, RoutedEventArgs e)
        {
            player.Show();
        }

        private static void LogStartupError(Exception ex)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "InTempo",
                    "logs");
                Directory.CreateDirectory(logDir);

                string logPath = Path.Combine(logDir, "startup-errors.log");
                string entry =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex}{Environment.NewLine}";
                File.AppendAllText(logPath, entry + Environment.NewLine);
            }
            catch
            {
                // non bloccare mai la UI per errori di logging
            }
        }

        private void RichiediCanticoInizialeSeNecessario()
        {
            Parte? canticoIniziale = DatiAdunanza.Parti
                .FirstOrDefault(parte => string.Equals(parte.NomeParte, "Cantico (iniziale)", StringComparison.OrdinalIgnoreCase));

            if (canticoIniziale == null)
            {
                return;
            }

            FinestraPopUP richiestaCantico = new FinestraPopUP(
                "Cantico iniziale",
                "Inserisci il numero del cantico iniziale per questa adunanza.",
                "Annulla",
                "Conferma",
                true);

            richiestaCantico.Owner = this;

            if (richiestaCantico.ShowDialog() == true && richiestaCantico.NumeroInserito.HasValue)
            {
                canticoIniziale.NomeParte = $"Cantico {richiestaCantico.NumeroInserito.Value}";
                LogicTimer.AggiornaGrafica();
            }
        }

        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_DLGMODALFRAME = 0x00000001L;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);
    }
}
