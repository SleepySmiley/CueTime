using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using CueTime.Classes.NonAbstract;
using CueTime.Classes.Statistics;
using CueTime.Classes.Utilities.Impostazioni;

namespace CueTime.Classes.Utilities
{
    public class TimerLogics : INotifyPropertyChanged
    {
        private readonly DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
        private readonly ImpostazioniAdunanze _settings;
        private readonly GestoreStatisticheAdunanze? _gestoreStatistiche;
        private DateTime? _ultimoPreavvisoPerOrarioInizio;
        private DateTime _ultimoTickUtc;
        private bool _notificaAutoStopUltimaParteInviata;
        private string _testoSchermoPrincipale = "00:00:00";
        private Brush? _orologioBrush = Brushes.White;
        private Brush? _timerBrush;
        private Brush? _tempoResiduoBrush;

        public event EventHandler? UltimaParteFuoriTempoMassimoRaggiunto;

        public Adunanza AdunanzaCorrente { get; set; }

        public bool IsRunning { get; private set; }

        public bool CheckTimerPreAdunanza { get; set; }

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

        public Brush? TempoResiduoBrush
        {
            get => _tempoResiduoBrush;
            set
            {
                if (_tempoResiduoBrush != value)
                {
                    _tempoResiduoBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimerLogics(Adunanza adunanzaCorrente, ImpostazioniAdunanze settings, GestoreStatisticheAdunanze? gestoreStatistiche = null)
        {
            AdunanzaCorrente = adunanzaCorrente;
            _settings = settings;
            _gestoreStatistiche = gestoreStatistiche;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            RicalcolaTempoResiduo();
            CheckColorParte();
            CheckColorTempoResiduo();
        }

        public void StartTimer()
        {
            if (IsRunning)
            {
                return;
            }

            IsRunning = true;
            CheckTimerPreAdunanza = false;
            _ultimoTickUtc = DateTime.UtcNow;
            AggiornaGrafica();
            OnPropertyChanged(nameof(IsRunning));
            timer.Start();
        }

        public void PauseTimer()
        {
            StopTimer();
        }

        public void ResumeTimer()
        {
            StartTimer();
        }

        public void StopTimer()
        {
            if (!IsRunning)
            {
                return;
            }

            timer.Stop();
            _ultimoTickUtc = DateTime.MinValue;
            IsRunning = false;
            OnPropertyChanged(nameof(IsRunning));
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            DateTime nowUtc = DateTime.UtcNow;
            if (_ultimoTickUtc == DateTime.MinValue)
            {
                _ultimoTickUtc = nowUtc;
                return;
            }

            TimeSpan intervallo = nowUtc - _ultimoTickUtc;
            if (intervallo <= TimeSpan.Zero)
            {
                return;
            }

            _ultimoTickUtc = nowUtc;
            AvanzaTimerDi(intervallo, new DateTimeOffset(nowUtc, TimeSpan.Zero));
        }

        internal void AvanzaTimerDi(TimeSpan intervallo, DateTimeOffset? fineIntervalloUtc = null)
        {
            if (AdunanzaCorrente.Current == null || intervallo <= TimeSpan.Zero)
            {
                return;
            }

            Parte parteCorrente = AdunanzaCorrente.Current;
            TimeSpan tempoPrima = parteCorrente.TempoScorrevole;
            parteCorrente.TempoScorrevole = parteCorrente.TempoScorrevole.Subtract(intervallo);
            RicalcolaTempoResiduo();
            CheckColorParte();
            CheckColorTempoResiduo();
            _gestoreStatistiche?.RegistraScorrimentoTimer(
                parteCorrente,
                intervallo,
                tempoPrima,
                parteCorrente.TempoScorrevole,
                fineIntervalloUtc ?? new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero));
            ControllaAutoStopUltimaParte(parteCorrente);
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

        public void ResetTimerPreciso(Parte parte)
        {
            parte.TempoScorrevole = parte.TempoParte;
            AggiornaGrafica();
            _gestoreStatistiche?.RegistraResetParte(parte);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetCompleto()
        {
            foreach (Parte parte in AdunanzaCorrente.Parti)
            {
                parte.TempoScorrevole = parte.TempoParte;
            }

            AdunanzaCorrente.Current = AdunanzaCorrente.Parti.FirstOrDefault();
            AdunanzaCorrente.TempoConsumatoPartiRimosse = TimeSpan.Zero;
            _notificaAutoStopUltimaParteInviata = false;
            RicalcolaTempoResiduo();
            CheckColorParte();
            CheckColorTempoResiduo();
        }

        public void AggiornaGrafica()
        {
            RicalcolaTempoResiduo();
            CheckColorParte();
            CheckColorTempoResiduo();

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
                double secondiMancanti = tempoMancante.TotalSeconds;

                if (secondiMancanti > 0 && secondiMancanti <= 60 && _ultimoPreavvisoPerOrarioInizio != orarioInizio.Value)
                {
                    CheckTimerPreAdunanza = true;
                    _ultimoPreavvisoPerOrarioInizio = orarioInizio.Value;
                }

                return secondiMancanti <= 0 && secondiMancanti > -5;
            }

            return false;
        }

        private DateTime? OttieniOrarioInizioAdunanzaOggi(DateTime now)
        {
            if (now.DayOfWeek == _settings.Infrasettimanale.GiornoSettimana)
            {
                return new DateTime(
                    now.Year,
                    now.Month,
                    now.Day,
                    _settings.Infrasettimanale.OraInizio.Hour,
                    _settings.Infrasettimanale.OraInizio.Minute,
                    0);
            }

            if (now.DayOfWeek == _settings.FineSettimana.GiornoSettimana)
            {
                return new DateTime(
                    now.Year,
                    now.Month,
                    now.Day,
                    _settings.FineSettimana.OraInizio.Hour,
                    _settings.FineSettimana.OraInizio.Minute,
                    0);
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
                    OrologioBrush = tempoMancante.TotalSeconds > 60
                        ? Brushes.Green
                        : Brushes.Orange;
                    return;
                }
            }

            OrologioBrush = Brushes.White;
        }

        public void RicalcolaTempoResiduo()
        {
            AdunanzaCorrente.NormalizzaTracciamentoResiduo();

            if (AdunanzaCorrente.Parti.Count == 0)
            {
                AdunanzaCorrente.TempoResiduo = TimeSpan.Zero;
                return;
            }

            TimeSpan tempoProgrammatoCorrente = AdunanzaCorrente.CalcolaTempoTotaleParti();
            TimeSpan correzionePartiVisibili = CalcolaCorrezioneResiduoPartiVisibili();

            AdunanzaCorrente.TempoResiduo =
                AdunanzaCorrente.TempoTotaleRiferimento
                - tempoProgrammatoCorrente
                + correzionePartiVisibili
                - AdunanzaCorrente.TempoConsumatoPartiRimosse;
        }

        public void RegistraRimozioneParte(Parte parte)
        {
            TimeSpan tempoConsumato = CalcolaTempoConsumatoParte(parte);
            if (tempoConsumato > TimeSpan.Zero)
            {
                AdunanzaCorrente.TempoConsumatoPartiRimosse += tempoConsumato;
            }
        }

        private TimeSpan CalcolaCorrezioneResiduoPartiVisibili()
        {
            int indiceCorrente = OttieniIndiceCorrente();
            TimeSpan totale = TimeSpan.Zero;

            for (int i = 0; i < AdunanzaCorrente.Parti.Count; i++)
            {
                Parte parte = AdunanzaCorrente.Parti[i];

                if (i < indiceCorrente)
                {
                    totale += parte.TempoScorrevole;
                    continue;
                }

                if (i == indiceCorrente && parte.TempoScorrevole < TimeSpan.Zero)
                {
                    totale += parte.TempoScorrevole;
                }
            }

            return totale;
        }

        private int OttieniIndiceCorrente()
        {
            if (AdunanzaCorrente.Current != null)
            {
                int indiceCorrente = AdunanzaCorrente.Parti.IndexOf(AdunanzaCorrente.Current);
                if (indiceCorrente >= 0)
                {
                    return indiceCorrente;
                }
            }

            for (int i = 0; i < AdunanzaCorrente.Parti.Count; i++)
            {
                if (AdunanzaCorrente.Parti[i].IsCurrent)
                {
                    return i;
                }
            }

            return 0;
        }

        private static TimeSpan CalcolaTempoConsumatoParte(Parte parte)
        {
            TimeSpan consumato = parte.TempoParte - parte.TempoScorrevole;
            return consumato > TimeSpan.Zero ? consumato : TimeSpan.Zero;
        }

        private void ControllaAutoStopUltimaParte(Parte parteCorrente)
        {
            if (_notificaAutoStopUltimaParteInviata
                || AdunanzaCorrente.Parti.Count == 0
                || !ReferenceEquals(parteCorrente, AdunanzaCorrente.Parti.LastOrDefault()))
            {
                return;
            }

            if (parteCorrente.TempoScorrevole <= TimeSpan.FromMinutes(-2))
            {
                _notificaAutoStopUltimaParteInviata = true;
                UltimaParteFuoriTempoMassimoRaggiunto?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

