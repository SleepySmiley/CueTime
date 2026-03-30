#nullable enable

using CueTime.Classes.Utilities;
using CueTime.Classes.Utilities.Impostazioni;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CueTime.Classes.View
{
    public partial class PlayerMusicale : Window
    {
        private readonly MediaPlayer _player = new MediaPlayer();
        private readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Send);
        private readonly TimerLogics _timerLogics;
        private readonly ImpostazioniAdunanze _settings;
        private readonly List<string> _percorsiBrani = new List<string>();

        private int _indiceCorrente = -1;
        private bool _inRiproduzione;
        private bool _staTrascinandoSlider;
        private bool _suppressSelectionChanged;
        private bool _initialFolderLoaded;
        private bool _consentiChiusuraDefinitiva;

        public PlayerMusicale(TimerLogics timerLogics, ImpostazioniAdunanze settings)
        {
            InitializeComponent();

            _timerLogics = timerLogics;
            _settings = settings;

            SliderVolume.ValueChanged += SliderVolume_ValueChanged;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            SliderVolume.Value = 1;
            _player.Volume = SliderVolume.Value;

            SliderProgresso.PreviewMouseLeftButtonDown += SliderProgresso_MouseDown;
            SliderProgresso.PreviewMouseLeftButtonUp += SliderProgresso_MouseUp;
            ListBrani.SelectionChanged += ListBrani_SelectionChanged;

            Loaded += PlayerMusicale_Loaded;
        }

        private async void PlayerMusicale_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInitialFolderAsync();
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = SliderVolume.Value;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnChiudi_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private async void BtnApriCartella_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog
            {
                Title = "Seleziona la cartella con le tracce audio"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string cartellaSelezionata = dialog.FolderName;
            bool caricata = await TryCaricaCartellaMusicaAsync(cartellaSelezionata, selezionaPrimoBrano: true);

            if (caricata)
            {
                _settings.PercorsoCartellaMusica = cartellaSelezionata;
                return;
            }

            SvuotaPlayer();

            FinestraPopUP avviso = new FinestraPopUP(
                "Attenzione",
                "Non sono stati trovati file audio supportati in questa cartella locale.",
                ConfigurazionePulsantiPopup.Ok);

            avviso.ShowDialog();
        }

        private async Task LoadInitialFolderAsync()
        {
            if (_initialFolderLoaded)
            {
                return;
            }

            _initialFolderLoaded = true;

            if (!await TryCaricaCartellaMusicaAsync(_settings.PercorsoCartellaMusica, selezionaPrimoBrano: true))
            {
                SvuotaPlayer();
            }
        }

        private void RiproduciBrano(int indice)
        {
            if (indice < 0 || indice >= _percorsiBrani.Count)
            {
                return;
            }

            if (_timerLogics.IsRunning)
            {
                FinestraPopUP avviso = new FinestraPopUP(
                    "Attenzione",
                    "Non e possibile riprodurre un brano mentre il timer e attivo. Ferma il timer prima di cambiare brano.",
                    ConfigurazionePulsantiPopup.Ok);

                avviso.ShowDialog();
                return;
            }

            _timer.Stop();
            _staTrascinandoSlider = false;

            SliderProgresso.Value = 0;
            TxtTempoTrascorso.Text = "00:00";
            TxtTempoTotale.Text = "00:00";

            _indiceCorrente = indice;

            _suppressSelectionChanged = true;
            ListBrani.SelectedIndex = indice;
            _suppressSelectionChanged = false;

            _player.Open(new Uri(_percorsiBrani[indice]));
            _player.Play();

            _inRiproduzione = true;
            IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            TxtBranoCorrente.Text = ListBrani.Items[indice]?.ToString() ?? "Nessun brano in riproduzione";

            _timer.Start();
        }

        private void BtnPlayPausa_Click(object sender, RoutedEventArgs e)
        {
            if (_percorsiBrani.Count == 0)
            {
                return;
            }

            if (_inRiproduzione)
            {
                _player.Pause();
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                _inRiproduzione = false;
                _timer.Stop();
                return;
            }

            if (_timerLogics.IsRunning)
            {
                FinestraPopUP avviso = new FinestraPopUP(
                    "Attenzione",
                    "Non e possibile riprodurre la musica mentre il timer e attivo. Ferma il timer prima di riprodurre la musica.",
                    ConfigurazionePulsantiPopup.Ok);

                avviso.ShowDialog();
                return;
            }

            int indiceDaRiprodurre = _indiceCorrente >= 0 ? _indiceCorrente : 0;

            if (_player.Source == null)
            {
                RiproduciBrano(indiceDaRiprodurre);
                return;
            }

            _player.Play();
            IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            _inRiproduzione = true;
            _timer.Start();
        }

        private void BtnPrecedente_Click(object sender, RoutedEventArgs e)
        {
            if (_percorsiBrani.Count == 0)
            {
                return;
            }

            int nuovoIndice = _indiceCorrente - 1;
            if (nuovoIndice < 0)
            {
                nuovoIndice = _percorsiBrani.Count - 1;
            }

            RiproduciBrano(nuovoIndice);
        }

        private void BtnSuccessivo_Click(object sender, RoutedEventArgs e)
        {
            if (_percorsiBrani.Count == 0)
            {
                return;
            }

            int nuovoIndice = _indiceCorrente + 1;
            if (nuovoIndice >= _percorsiBrani.Count)
            {
                nuovoIndice = 0;
            }

            RiproduciBrano(nuovoIndice);
        }

        private void ListBrani_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelectionChanged)
            {
                return;
            }

            if (ListBrani.SelectedIndex != -1 && ListBrani.SelectedIndex != _indiceCorrente)
            {
                RiproduciBrano(ListBrani.SelectedIndex);
            }
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            if (_player.NaturalDuration.HasTimeSpan)
            {
                SliderProgresso.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                TxtTempoTotale.Text = _player.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            BtnSuccessivo_Click(null!, null!);
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            _timer.Stop();
            _player.Stop();

            _inRiproduzione = false;
            _staTrascinandoSlider = false;

            SliderProgresso.Value = 0;
            TxtTempoTrascorso.Text = "00:00";
            TxtTempoTotale.Text = "00:00";
            IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;

            AppLogger.LogError("Errore durante la riproduzione del file audio.", e.ErrorException);

            FinestraPopUP avviso = new FinestraPopUP(
                "Errore riproduzione",
                "Il file audio selezionato non puo essere riprodotto.",
                ConfigurazionePulsantiPopup.Ok);

            avviso.ShowDialog();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_player.Source != null && _player.NaturalDuration.HasTimeSpan && !_staTrascinandoSlider)
            {
                SliderProgresso.Value = _player.Position.TotalSeconds;
                TxtTempoTrascorso.Text = _player.Position.ToString(@"mm\:ss");
            }

            if (_timerLogics.IsRunning)
            {
                _player.Stop();
                _inRiproduzione = false;
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                SliderProgresso.Value = 0;
                TxtTempoTrascorso.Text = "00:00";
                _timer.Stop();
                return;
            }

            if (_timerLogics.CheckTimerPreAdunanza)
            {
                GestisciStopGraduale();
            }
        }

        private void SliderProgresso_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _staTrascinandoSlider = true;
        }

        private void SliderProgresso_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_player.Source != null && _player.NaturalDuration.HasTimeSpan)
            {
                _player.Position = TimeSpan.FromSeconds(SliderProgresso.Value);
            }

            _staTrascinandoSlider = false;
        }

        private void GestisciStopGraduale()
        {
            if (!_inRiproduzione || _player.Source == null || SliderVolume.Value <= 0)
            {
                _timerLogics.CheckTimerPreAdunanza = false;
                _player.Volume = SliderVolume.Value;
                return;
            }

            double stepVolume = SliderVolume.Value / 30.0;
            _player.Volume = Math.Max(0, _player.Volume - stepVolume);

            if (_player.Volume <= 0)
            {
                _player.Stop();
                _inRiproduzione = false;
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                SliderProgresso.Value = 0;
                TxtTempoTrascorso.Text = "00:00";
                _player.Volume = SliderVolume.Value;
                _timerLogics.CheckTimerPreAdunanza = false;
                _timer.Stop();
            }
        }

        private async Task<bool> TryCaricaCartellaMusicaAsync(string cartellaSelezionata, bool selezionaPrimoBrano)
        {
            if (!IsPercorsoCartellaValido(cartellaSelezionata))
            {
                return false;
            }

            string[] fileTrovati;

            try
            {
                fileTrovati = await Task.Run(() =>
                    Directory.EnumerateFiles(cartellaSelezionata)
                        .Where(IsAudioSupportato)
                        .OrderBy(file => Path.GetFileName(file), StringComparer.CurrentCultureIgnoreCase)
                        .ToArray());
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Errore durante la scansione della cartella musicale '{cartellaSelezionata}'.", ex);
                return false;
            }

            _timer.Stop();
            _player.Stop();
            _player.Close();

            _percorsiBrani.Clear();
            ListBrani.Items.Clear();

            foreach (string file in fileTrovati)
            {
                _percorsiBrani.Add(file);
                ListBrani.Items.Add(Path.GetFileNameWithoutExtension(file));
            }

            if (_percorsiBrani.Count == 0)
            {
                return false;
            }

            _indiceCorrente = 0;
            _inRiproduzione = false;
            _staTrascinandoSlider = false;

            if (selezionaPrimoBrano)
            {
                _suppressSelectionChanged = true;
                ListBrani.SelectedIndex = 0;
                _suppressSelectionChanged = false;
            }

            SliderProgresso.Value = 0;
            TxtTempoTrascorso.Text = "00:00";
            TxtTempoTotale.Text = "00:00";
            TxtBranoCorrente.Text = "Nessun brano in riproduzione";
            IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
            _player.Volume = SliderVolume.Value;

            return true;
        }

        private static bool IsPercorsoCartellaValido(string cartellaSelezionata)
        {
            return !string.IsNullOrWhiteSpace(cartellaSelezionata)
                   && Directory.Exists(cartellaSelezionata);
        }

        private void SvuotaPlayer()
        {
            _timer.Stop();
            _player.Stop();
            _player.Close();

            _percorsiBrani.Clear();
            ListBrani.Items.Clear();
            ListBrani.SelectedIndex = -1;

            _indiceCorrente = -1;
            _inRiproduzione = false;
            _staTrascinandoSlider = false;

            SliderProgresso.Maximum = 100;
            SliderProgresso.Value = 0;
            TxtTempoTrascorso.Text = "00:00";
            TxtTempoTotale.Text = "00:00";
            TxtBranoCorrente.Text = "Nessun brano in riproduzione";
            IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
            _player.Volume = SliderVolume.Value;
        }

        private void PopupVolumeContent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PopupVolumeContent.IsVisible)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Keyboard.Focus(PopupVolumeContent);
                }), DispatcherPriority.Input);
            }
        }

        private void PopupVolumeContent_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!PopupVolumeContent.IsKeyboardFocusWithin)
                {
                    PopupVolume.IsPopupOpen = false;
                }
            }), DispatcherPriority.Background);
        }

        private static bool IsAudioSupportato(string filePath)
        {
            string estensione = Path.GetExtension(filePath).ToLowerInvariant();
            return estensione == ".mp3"
                || estensione == ".wav"
                || estensione == ".wma"
                || estensione == ".m4a";
        }

        public void PrepareForShutdown()
        {
            _consentiChiusuraDefinitiva = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_consentiChiusuraDefinitiva)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            _timer.Stop();
            _timer.Tick -= Timer_Tick;

            SliderVolume.ValueChanged -= SliderVolume_ValueChanged;
            SliderProgresso.PreviewMouseLeftButtonDown -= SliderProgresso_MouseDown;
            SliderProgresso.PreviewMouseLeftButtonUp -= SliderProgresso_MouseUp;
            ListBrani.SelectionChanged -= ListBrani_SelectionChanged;
            Loaded -= PlayerMusicale_Loaded;

            _player.MediaOpened -= Player_MediaOpened;
            _player.MediaEnded -= Player_MediaEnded;
            _player.MediaFailed -= Player_MediaFailed;

            _player.Stop();
            _player.Close();

            base.OnClosing(e);
        }
    }
}