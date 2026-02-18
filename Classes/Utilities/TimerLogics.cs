using InTempo.Classes.NonAbstract;
using System.Windows.Media;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InTempo.Classes.Utilities
{
    public class TimerLogics : INotifyPropertyChanged
    {
        private DispatcherTimer timer = new DispatcherTimer();
        public Adunanza AdunanzaCorrente { get; }

        private bool isRunning = false;
        public bool IsRunning => isRunning; // Esposto per controllo esterno

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
            if (!isRunning)
            {
                isRunning = true;
                timer.Start();
            }
        }

        public void StopTimer()
        {
            if (isRunning)
            {
                timer.Stop();
                isRunning = false;
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
    }
}