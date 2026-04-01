using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using InTempo.Classes.Utilities.Theming;
using InTempo.Classes.View;

namespace InTempo
{
    public partial class MainWindow : Window
    {
        private readonly FinestraTimer _finestratimer;
        private PlayerMusicale player;
        private bool _isPaused = true;

        public Adunanza DatiAdunanza { get; set; }

        public TimerLogics LogicTimer { get; set; }

        public string IconaStatoTimer
        {
            get => (string)GetValue(IconaStatoTimerProperty);
            set => SetValue(IconaStatoTimerProperty, value);
        }

        public static readonly DependencyProperty IconaStatoTimerProperty =
            DependencyProperty.Register("IconaStatoTimer", typeof(string), typeof(MainWindow), new PropertyMetadata("Play"));

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += MainWindow_SourceInitialized;

            DatiAdunanza = new Adunanza(App.Settings);
            LogicTimer = new TimerLogics(DatiAdunanza, App.Settings);
            player = new PlayerMusicale(LogicTimer, App.Settings);

            AvviaOrologio();
            DataContext = this;
            AggiornaStatoNavigazioneParti();

            _finestratimer = new FinestraTimer(Orologio, LogicTimer, App.Settings);
            _finestratimer.Show();
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            HideCaptionIcon();
        }

        private void HideCaptionIcon()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

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
            bool needsBackgroundRefresh = false;

            try
            {
                await DatiAdunanza.SelectedAdunanza(preferCacheOnly: true);
                needsBackgroundRefresh = !DatiAdunanza.LastLoadIsCurrentWeek;
                LogicTimer.AggiornaGrafica();
                RichiediCanticoInizialeSeNecessario();
                AggiornaStatoNavigazioneParti();
            }
            catch (Exception ex)
            {
                DatiAdunanza.Parti.Clear();
                DatiAdunanza.Current = null;
                needsBackgroundRefresh = true;
                LogicTimer.AggiornaGrafica();
                AggiornaStatoNavigazioneParti();
                LogStartupError(ex);
            }
            finally
            {
                Caricamento();
            }

            if (needsBackgroundRefresh)
            {
                _ = AggiornaAdunanzaDaWebInBackgroundAsync();
                Caricamento();
            }
        }

        private void BtnAvanti_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused || DatiAdunanza.Current == null)
            {
                return;
            }

            DatiAdunanza.Avanti();
            CheckCantico();
            LogicTimer.AggiornaGrafica();
            AggiornaStatoNavigazioneParti();
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused || DatiAdunanza.Current == null)
            {
                return;
            }

            DatiAdunanza.Indietro();
            CheckCantico();
            LogicTimer.AggiornaGrafica();
            AggiornaStatoNavigazioneParti();
        }

        public bool CheckCantico()
        {
            Parte? current = DatiAdunanza.Current;

            if (current?.TipoParte == "Cantico")
            {
                _finestratimer.CambiaVista(VistaPresentazione.SoloScritta, current.NomeParte, System.Windows.Media.Brushes.Yellow);
                btnCommentoSchermo.IsEnabled = false;
                return true;
            }

            _finestratimer.CambiaVista(VistaPresentazione.SoloTimer, string.Empty, System.Windows.Media.Brushes.White);
            btnCommentoSchermo.IsEnabled = true;
            return false;
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

        private Parte? GetParteFromButton(object sender)
        {
            return sender is Button btn && btn.Tag is Parte parte ? parte : null;
        }

        private void MenuItemReset_Click(object sender, RoutedEventArgs e)
        {
            Parte? parte = GetParteFromButton(sender);
            if (parte != null)
            {
                LogicTimer.ResetTimerPreciso(parte);
            }
        }

        private void MenuItemAggiungi_Click(object sender, RoutedEventArgs e)
        {
            Parte? parteSelezionata = GetParteFromButton(sender);
            if (parteSelezionata == null)
            {
                return;
            }

            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();

            int indice = DatiAdunanza.Parti.IndexOf(parteSelezionata);
            ModificaParte finestra = new ModificaParte();

            if (finestra.ShowDialog() == true)
            {
                DatiAdunanza.Parti.Insert(indice + 1, finestra.ParteCopia);
                LogicTimer.AggiornaGrafica();
            }

            if (wasRunning)
            {
                LogicTimer.StartTimer();
            }
        }

        private void MenuItemElimina_Click(object sender, RoutedEventArgs e)
        {
            Parte? parteSelezionata = GetParteFromButton(sender);
            if (DatiAdunanza.Parti.Count == 1 || parteSelezionata == null)
            {
                return;
            }

            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();
            LogicTimer.RegistraRimozioneParte(parteSelezionata);

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
            }

            DatiAdunanza.Parti.Remove(parteSelezionata);
            LogicTimer.AggiornaGrafica();
            CheckCantico();

            if (wasRunning)
            {
                LogicTimer.StartTimer();
            }
        }

        private void MenuItemModifica_Click(object sender, RoutedEventArgs e)
        {
            Parte? parteSelezionata = GetParteFromButton(sender);
            if (parteSelezionata == null)
            {
                return;
            }

            bool wasRunning = LogicTimer.IsRunning;
            LogicTimer.StopTimer();

            ModificaParte finestra = new ModificaParte(parteSelezionata);

            if (finestra.ShowDialog() == true)
            {
                int index = DatiAdunanza.Parti.IndexOf(parteSelezionata);
                if (index != -1)
                {
                    TimeSpan differenzaTempo = finestra.ParteCopia.TempoParte - parteSelezionata.TempoParte;

                    Parte target = DatiAdunanza.Parti[index];
                    target.NumeroParte = finestra.ParteCopia.NumeroParte;
                    target.NomeParte = finestra.ParteCopia.NomeParte;
                    target.TempoParte = finestra.ParteCopia.TempoParte;
                    target.TipoParte = finestra.ParteCopia.TipoParte;
                    target.ColoreParte = finestra.ParteCopia.ColoreParte;
                    target.TempoScorrevole += differenzaTempo;
                }
            }

            LogicTimer.AggiornaGrafica();
            CheckCantico();

            if (wasRunning)
            {
                LogicTimer.StartTimer();
            }
        }

        public void Caricamento()
        {
            bool hasLoadedParts = DatiAdunanza.Parti.Count > 0;
            bool shouldShowLoading = WebPartsLoader.IsLoading && !hasLoadedParts;
            Visibility visibility = shouldShowLoading ? Visibility.Visible : Visibility.Collapsed;
            loadingOverlay.Visibility = visibility;
            prgbar.Visibility = shouldShowLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string themeStateBefore = CreateThemeStateSnapshot();
            Impostazioni finestra = new Impostazioni(LogicTimer, App.Settings)
            {
                Owner = this
            };

            if (finestra.ShowDialog() == true)
            {
                _finestratimer.ApplicaMonitorScelto();
                if (!string.Equals(themeStateBefore, CreateThemeStateSnapshot(), StringComparison.Ordinal))
                {
                    RicreaPlayerMusicale();
                }

                SincronizzaStatoVisuale();
            }
        }

        private DispatcherTimer Orologio { get; } = new DispatcherTimer(DispatcherPriority.Send);

        private void AvviaOrologio()
        {
            Orologio.Interval = TimeSpan.FromSeconds(1);
            Orologio.Tick += Timer_Tick;
            txtTimer.Visibility = Visibility.Collapsed;
            txtOrologio.Visibility = Visibility.Visible;
            Orologio.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            bool devePartire = LogicTimer.CalcolaStatoOrologio(now, _isPaused);

            if (devePartire)
            {
                SetStatoAdunanza(true);
            }
        }

        private void SetStatoAdunanza(bool avvia)
        {
            if (avvia)
            {
                if (!_isPaused)
                {
                    return;
                }

                _isPaused = false;
                IconaStatoTimer = "Pause";

                Orologio.Stop();
                _finestratimer.CambiaVista(VistaPresentazione.SoloTimer, string.Empty, System.Windows.Media.Brushes.White);
                txtOrologio.Visibility = Visibility.Collapsed;
                txtTimer.Visibility = Visibility.Visible;
                btnCommentoSchermo.IsEnabled = true;
                LogicTimer.StartTimer();
                CheckCantico();
                AggiornaStatoNavigazioneParti();
                return;
            }

            if (_isPaused)
            {
                return;
            }

            FinestraPopUP avvertimento = new FinestraPopUP(
                "Attenzione",
                "Sei sicuro di voler fermare il timer? \nL'adunanza verrà conclusa e resettata.",
                ConfigurazionePulsantiPopup.ConfermaAnnulla);
            avvertimento.ShowDialog();
            if (avvertimento.DialogResult != true)
            {
                return;
            }

            _isPaused = true;
            IconaStatoTimer = "Play";

            LogicTimer.StopTimer();
            LogicTimer.ResetCompleto();
            _finestratimer.CambiaVista(VistaPresentazione.Orologio, string.Empty, System.Windows.Media.Brushes.White);

            txtTimer.Visibility = Visibility.Collapsed;
            txtOrologio.Visibility = Visibility.Visible;
            btnCommentoSchermo.IsEnabled = false;

            Orologio.Start();
            AggiornaStatoNavigazioneParti();
        }

        private void AggiornaStatoNavigazioneParti()
        {
            bool adunanzaAvviata = !_isPaused && DatiAdunanza.Current != null && DatiAdunanza.Parti.Count > 0;
            BtnIndietro.IsEnabled = adunanzaAvviata;
            BtnAvanti.IsEnabled = adunanzaAvviata;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _finestratimer?.Close();
            player?.Close();
        }

        private void btnCommentoSchermo_Click(object sender, RoutedEventArgs e)
        {
            FinestraPopUP messaggioOratore = new FinestraPopUP("Messaggi", "Tutto Schermo", "Parziale", _finestratimer)
            {
                Owner = this
            };
            messaggioOratore.Show();
        }

        private void btnMusica_Click(object sender, RoutedEventArgs e)
        {
            player.Owner ??= this;
            player.Show();
        }

        private void RicreaPlayerMusicale()
        {
            bool wasVisible = player.IsVisible;
            Window? previousOwner = player.Owner;

            player.Close();
            player = new PlayerMusicale(LogicTimer, App.Settings)
            {
                Owner = previousOwner ?? this
            };

            if (wasVisible)
            {
                player.Show();
            }
        }

        private string CreateThemeStateSnapshot()
        {
            string themeKey = ThemeManager.GetThemeOrDefault(App.Settings.TemaSelezionato, App.Settings.TemaPersonalizzato).Key;
            string customPalette = JsonSerializer.Serialize(App.Settings.TemaPersonalizzato ?? ThemeManager.CreateDefaultCustomTheme());
            return $"{themeKey}|{customPalette}";
        }

        private static void LogStartupError(Exception ex)
        {
            AppLogger.LogError("Errore durante il caricamento iniziale dell'adunanza.", ex);
        }

        private async Task AggiornaAdunanzaDaWebInBackgroundAsync()
        {
            string? canticoInizialeManuale = GetCanticoInizialeManualeCorrente();

            try
            {
                await DatiAdunanza.SelectedAdunanza();
                RipristinaCanticoInizialeManuale(canticoInizialeManuale);
                LogicTimer.AggiornaGrafica();
                AggiornaStatoNavigazioneParti();
            }
            catch (Exception ex)
            {
                LogStartupError(ex);

                if (DatiAdunanza.Parti.Count == 0)
                {
                    FinestraPopUP errore = new FinestraPopUP(
                        "Errore caricamento",
                        "Impossibile caricare i dati dell'adunanza dal web e non era disponibile alcuna cache locale.",
                        ConfigurazionePulsantiPopup.Ok);
                    errore.ShowDialog();
                }
            }
            finally
            {
                Caricamento();
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
                true)
            {
                Owner = this
            };

            if (richiestaCantico.ShowDialog() == true && richiestaCantico.NumeroInserito.HasValue)
            {
                canticoIniziale.NomeParte = $"Cantico {richiestaCantico.NumeroInserito.Value}";
                LogicTimer.AggiornaGrafica();
            }
        }

        private string? GetCanticoInizialeManualeCorrente()
        {
            Parte? canticoIniziale = DatiAdunanza.Parti
                .FirstOrDefault(parte => parte.NumeroParte == 1 && string.Equals(parte.TipoParte, ParteFactory.TypeCantico, StringComparison.OrdinalIgnoreCase));

            if (canticoIniziale == null
                || string.Equals(canticoIniziale.NomeParte, "Cantico (iniziale)", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return canticoIniziale.NomeParte;
        }

        private void RipristinaCanticoInizialeManuale(string? nomeCantico)
        {
            if (string.IsNullOrWhiteSpace(nomeCantico))
            {
                return;
            }

            Parte? canticoIniziale = DatiAdunanza.Parti
                .FirstOrDefault(parte => parte.NumeroParte == 1 && string.Equals(parte.TipoParte, ParteFactory.TypeCantico, StringComparison.OrdinalIgnoreCase));

            if (canticoIniziale != null
                && string.Equals(canticoIniziale.NomeParte, "Cantico (iniziale)", StringComparison.OrdinalIgnoreCase))
            {
                canticoIniziale.NomeParte = nomeCantico;
            }
        }

        private void SincronizzaStatoVisuale()
        {
            LogicTimer.AggiornaGrafica();

            if (_isPaused)
            {
                _finestratimer.CambiaVista(VistaPresentazione.Orologio, string.Empty, System.Windows.Media.Brushes.White);
                btnCommentoSchermo.IsEnabled = false;
                txtTimer.Visibility = Visibility.Collapsed;
                txtOrologio.Visibility = Visibility.Visible;
                return;
            }

            CheckCantico();
            txtOrologio.Visibility = Visibility.Collapsed;
            txtTimer.Visibility = Visibility.Visible;
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
