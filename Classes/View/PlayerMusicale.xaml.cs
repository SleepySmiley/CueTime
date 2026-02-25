using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace InTempo.Classes.View
{
    public partial class PlayerMusicale : Window
    {
        private MediaPlayer _player = new MediaPlayer();
        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);
        private List<string> _percorsiBrani = new List<string>();
        private int _indiceCorrente = -1;
        private bool _inRiproduzione = false;
        private bool _staTrascinandoSlider = false;

        public PlayerMusicale()
        {
            InitializeComponent();
            SliderVolume.ValueChanged += SliderVolume_ValueChanged;
            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            SliderVolume.Value = 1;
            _player.Volume = SliderVolume.Value;

            SliderProgresso.PreviewMouseLeftButtonDown += (s, e) => _staTrascinandoSlider = true;
            SliderProgresso.PreviewMouseLeftButtonUp += SliderProgresso_MouseUp;

            ListBrani.SelectionChanged += ListBrani_SelectionChanged;
            CaricaCartellaMusica(App.Settings.PercorsoCartellaMusica);
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = SliderVolume.Value;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnChiudi_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void BtnApriCartella_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Seleziona la cartella con le tracce audio"
            };

            if (dialog.ShowDialog() == true)
            {
                string cartellaSelezionata = dialog.FolderName;
                App.Settings.PercorsoCartellaMusica = cartellaSelezionata;

                _percorsiBrani.Clear();
                ListBrani.Items.Clear();

                string[] fileTrovati = Directory.GetFiles(cartellaSelezionata);

                foreach (string file in fileTrovati)
                {
                    string estensione = Path.GetExtension(file).ToLower();
                    if (estensione == ".mp3" || estensione == ".wav" || estensione == ".wma" || estensione == ".m4a")
                    {
                        _percorsiBrani.Add(file);
                        ListBrani.Items.Add(Path.GetFileNameWithoutExtension(file));
                    }
                }

                if (_percorsiBrani.Count > 0)
                {
                    RiproduciBrano(0);
                    _player.Pause();
                }
                else
                {
                    FinestraPopUP Avviso = new FinestraPopUP("Attenzione", "Non sono stati trovati file audio supportati in questa cartella.", 1);
                    Avviso.ShowDialog();
                }
            }
        }

        private void CaricaCartellaMusica(string cartellaSelezionata)
        {
            if (string.IsNullOrEmpty(cartellaSelezionata) || !Directory.Exists(cartellaSelezionata))
                return;

            _percorsiBrani.Clear();
            ListBrani.Items.Clear();

            string[] fileTrovati = Directory.GetFiles(cartellaSelezionata);

            foreach (string file in fileTrovati)
            {
                string estensione = Path.GetExtension(file).ToLower();
                if (estensione == ".mp3" || estensione == ".wav" || estensione == ".wma" || estensione == ".m4a")
                {
                    _percorsiBrani.Add(file);
                    ListBrani.Items.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            if (_percorsiBrani.Count > 0)
            {
                RiproduciBrano(0);
                _player.Pause();
            }
        }

        private void RiproduciBrano(int indice)
        {
            if (indice < 0 || indice >= _percorsiBrani.Count) return;

            if(TimerLogics.IsRunning)
            {
                FinestraPopUP Avviso = new FinestraPopUP("Attenzione", "Non è possibile riprodurre un brano mentre il timer è attivo. Per favore, ferma il timer prima di cambiare brano.", 1);
                Avviso.ShowDialog();
                return;
            }

            _timer.Stop();
            _staTrascinandoSlider = false;

            SliderProgresso.Value = 0;
            TxtTempoTrascorso.Text = "00:00";
            TxtTempoTotale.Text = "00:00";

            _indiceCorrente = indice;
            ListBrani.SelectedIndex = indice;

            _player.Open(new Uri(_percorsiBrani[indice]));
            _player.Play();

            _inRiproduzione = true;
            IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            TxtBranoCorrente.Text = ListBrani.Items[indice].ToString();

            _timer.Start();
        }

        private void BtnPlayPausa_Click(object sender, RoutedEventArgs e)
        {
            

            if (_percorsiBrani.Count == 0) return;

            if (_inRiproduzione)
            {
                _player.Pause();
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                _inRiproduzione = false;
            }
            else if(!TimerLogics.IsRunning) 
            {
                _player.Play();
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                _inRiproduzione = true;
            }
            else
            {
                FinestraPopUP Avviso = new FinestraPopUP("Attenzione", "Non è possibile riprodurre la musica mentre il timer è attivo. Per favore, ferma il timer prima di riprodurre la musica.", 1);
                Avviso.ShowDialog();
            }
        }

        private void BtnPrecedente_Click(object sender, RoutedEventArgs e)
        {
            if (_percorsiBrani.Count == 0) return;

            int nuovoIndice = _indiceCorrente - 1;
            if (nuovoIndice < 0) nuovoIndice = _percorsiBrani.Count - 1;

            RiproduciBrano(nuovoIndice);
        }

        private void BtnSuccessivo_Click(object sender, RoutedEventArgs e)
        {
            if (_percorsiBrani.Count == 0) return;

            int nuovoIndice = _indiceCorrente + 1;
            if (nuovoIndice >= _percorsiBrani.Count) nuovoIndice = 0;

            RiproduciBrano(nuovoIndice);
        }

        private void ListBrani_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_player.Source != null && _player.NaturalDuration.HasTimeSpan && !_staTrascinandoSlider)
            {
                SliderProgresso.Value = _player.Position.TotalSeconds;
                TxtTempoTrascorso.Text = _player.Position.ToString(@"mm\:ss");
            }

            if (TimerLogics.IsRunning)
            {
                _player.Stop();
                _inRiproduzione = false;
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                return;
            }

            if (TimerLogics.CheckTimerPreAdunanza)
            {
                GestisciStopGraduale();
                return;
            }

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
            double stepVolume = SliderVolume.Value / 30.0;

            _player.Volume -= stepVolume;

            if (_player.Volume <= 0)
            {
                _player.Stop();
                _inRiproduzione = false;
                IconaPlayPausa.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;

                SliderProgresso.Value = 0;
                TxtTempoTrascorso.Text = "00:00";
                _player.Volume = SliderVolume.Value;
                TimerLogics.CheckTimerPreAdunanza = false;
            }
        }
    }
}