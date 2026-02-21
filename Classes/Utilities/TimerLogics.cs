using InTempo.Classes.NonAbstract;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InTempo.Classes.Utilities
{
    public class TimerLogics : INotifyPropertyChanged
    {
        private DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
        public Adunanza AdunanzaCorrente { get; }

        public static bool IsRunning { get; private set; } = false;

        private string _testoSchermoPrincipale = "00:00:00";

        public string TestoSchermoPrincipale
        {
            get => _testoSchermoPrincipale;
            set
            {
                if (_testoSchermoPrincipale != value)
                {
                    _testoSchermoPrincipale = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush? _orologioBrush = Brushes.White;
        public Brush? OrologioBrush
        {
            get => _orologioBrush;
            set
            {
                if (_orologioBrush != value)
                {
                    _orologioBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush? _timerBrush;
        public Brush? TimerBrush
        {
            get => _timerBrush;
            set
            {
                if (_timerBrush != value)
                {
                    _timerBrush = value;
                    OnPropertyChanged();
                }
            }
        }
        private Brush? _temporesiduobrush;
        public Brush? TempoResiduoBrush
        {
            get => _temporesiduobrush;
            set
            {
                if (_temporesiduobrush != value)
                {
                    _temporesiduobrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimerLogics(Adunanza adunanzaCorrente)
        {
            AdunanzaCorrente = adunanzaCorrente;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            CheckColorParte();
            CheckColorTempoResiduo();
        }

        public void StartTimer()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                AggiornaGrafica(); // Aggiorna subito la grafica per evitare ritardi visivi
                timer.Start();
            }
        }

        public void StopTimer()
        {
            if (IsRunning)
            {
                timer.Stop();
                IsRunning = false;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AdunanzaCorrente.Current != null)
            {
                AdunanzaCorrente.Current.TempoScorrevole = AdunanzaCorrente.Current.TempoScorrevole.Add(TimeSpan.FromSeconds(-1));
                CheckColorParte();
                CheckColorTempoResiduo();
            }
        }

        private void CheckColorParte()
        {
            if (AdunanzaCorrente.Current == null)
            {
                return;
            }

            if (AdunanzaCorrente.Current.TempoScorrevole > TimeSpan.FromSeconds(60))
            {
                TimerBrush = Brushes.Green;
            }
            else if (AdunanzaCorrente.Current.TempoScorrevole > TimeSpan.Zero)
            {
                TimerBrush = Brushes.Orange;
            }
            else
            {
                TimerBrush = Brushes.Red;
                // siamo in ritardo sul tempo perciò continuo a scalare il tempo residuo
                AdunanzaCorrente.TempoResiduo = AdunanzaCorrente.TempoResiduo.Add(TimeSpan.FromSeconds(-1));
            }
        }

        private void CheckColorTempoResiduo()
        {
            if (AdunanzaCorrente.TempoResiduo > TimeSpan.Zero)
            {
                TempoResiduoBrush = Brushes.Green;
            }
            else if (AdunanzaCorrente.TempoResiduo == TimeSpan.Zero)
            {
                TempoResiduoBrush = Brushes.Black;
            }
            else
            {
                TempoResiduoBrush = Brushes.Red;
            }
        }

        public void ResetTimerPreciso(Parte Parte)
        {
            if(Parte == AdunanzaCorrente.Current)
            {
                TimeSpan temp = Parte.TempoParte - Parte.TempoScorrevole;
                if (temp > TimeSpan.Zero)
                {
                    AdunanzaCorrente.TempoResiduo = AdunanzaCorrente.TempoResiduo.Add(temp);
                }
            }

            Parte.TempoScorrevole = Parte.TempoParte;

            if (Parte == AdunanzaCorrente.Current)
            {
                CheckColorParte();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //metodo per resettare tutti i timer e far ripartire un aduannza da zero 
        public void ResetCompleto()
        {
            foreach (var parte in AdunanzaCorrente.Parti)
            {
                parte.TempoScorrevole = parte.TempoParte;
            }
            AdunanzaCorrente.Current = AdunanzaCorrente.Parti.FirstOrDefault();
            AdunanzaCorrente.TempoResiduo = TimeSpan.Zero;
            CheckColorParte();
            CheckColorTempoResiduo();
        }

        public void AggiornaGrafica()
        {
            // 1. Ricalcola i colori per sicurezza
            CheckColorParte();
            CheckColorTempoResiduo();

            // 2. Forza lo XAML a rileggere i dati in questo esatto millisecondo
            OnPropertyChanged(nameof(AdunanzaCorrente));
            OnPropertyChanged(nameof(TimerBrush));
            OnPropertyChanged(nameof(TempoResiduoBrush));
        }

        public bool CalcolaStatoOrologio(DateTime now, bool isPaused)
        {
            DateTime? orarioInizio = OttieniOrarioInizioAdunanzaOggi(now);

            if (isPaused)
            {
                TestoSchermoPrincipale = GeneraTestoSchermo(now, orarioInizio);
                AggiornaColoreOrologio(now, orarioInizio);
            }

            if (orarioInizio.HasValue && isPaused)
            {
                TimeSpan tempoMancante = orarioInizio.Value - now;

                if (tempoMancante.TotalSeconds <= 0 && tempoMancante.TotalSeconds > -5)
                {
                    return true;
                }
                else
                {
                    return false; 
                }
            }
            return false;
        }

        private DateTime? OttieniOrarioInizioAdunanzaOggi(DateTime now)
        {
            if (now.DayOfWeek == App.Settings.Infrasettimanale.GiornoSettimana)
            {
                return new DateTime(now.Year, now.Month, now.Day,
                                    App.Settings.Infrasettimanale.OraInizio.Hour,
                                    App.Settings.Infrasettimanale.OraInizio.Minute, 0);
            }

            if (now.DayOfWeek == App.Settings.FineSettimana.GiornoSettimana)
            {
                return new DateTime(now.Year, now.Month, now.Day,
                                    App.Settings.FineSettimana.OraInizio.Hour,
                                    App.Settings.FineSettimana.OraInizio.Minute, 0);
            }

            return null;
        }

        private string GeneraTestoSchermo(DateTime now, DateTime? orarioInizio)
        {
            if (orarioInizio.HasValue)
            {
                TimeSpan tempoMancante = orarioInizio.Value - now;

                if (tempoMancante.TotalMinutes <= 30 && tempoMancante.TotalSeconds > 0)
                {
                    return tempoMancante.ToString(@"mm\:ss");
                }
            }

            return now.ToString("HH:mm:ss");
        }
        private void AggiornaColoreOrologio(DateTime now, DateTime? orarioInizio)
        {
            if (orarioInizio.HasValue)
            {
                TimeSpan tempoMancante = orarioInizio.Value - now;

                if (tempoMancante.TotalMinutes <= 30 && tempoMancante.TotalSeconds > 0)
                {
                    if (tempoMancante.TotalSeconds > 60)
                    {
                        OrologioBrush = Brushes.Green;
                    }
                    else if (tempoMancante.TotalSeconds > 0)
                    {
                        OrologioBrush = Brushes.Orange;
                    }
                    else
                    {
                        OrologioBrush = Brushes.Red;
                    }
                    return;
                }
            }
            OrologioBrush = Brushes.White;
        }

    }
}