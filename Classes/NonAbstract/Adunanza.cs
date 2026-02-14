using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace InTempo.Classes.NonAbstract
{
    public class Adunanza : INotifyPropertyChanged
    {
        private Finesettimanale _finesettimana = new Finesettimanale();
        private Infrasettimanale _infrasettimanale = new Infrasettimanale();

        private ObservableCollection<Parte> _parti = new ObservableCollection<Parte>();
        public ObservableCollection<Parte> Parti
        {
            get => _parti;
            set
            {
                if (!ReferenceEquals(_parti, value))
                {
                    _parti = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _currentParte = 0;
        private Parte? _current;

        public Parte? Current
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged();
                }
            }
        }

        private TimeSpan _tempoResiduo;
        public TimeSpan TempoResiduo
        {
            get => _tempoResiduo;
            set
            {
                if (_tempoResiduo != value)
                {
                    _tempoResiduo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TempoResiduoString));
                }
            }
        }



        // Proprietà aggiunta per gestire la visualizzazione con +/- 
        public string TempoResiduoString
        {
            get
            {
                string segno = "";

                if (TempoResiduo > TimeSpan.Zero)
                {
                    segno = "-";
                }
                else if (TempoResiduo < TimeSpan.Zero)
                {
                    segno = "+";
                }

                return $"{segno}{_tempoResiduo:mm\\:ss}";
            }
        }

        public async Task SelectedAdunanza()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            Parti.Clear();

            if (today == DayOfWeek.Sunday || today == DayOfWeek.Saturday)
            {
                await _finesettimana.LoadAsync();
                Parti = _finesettimana.Parti;
            }
            else
            {
                await _infrasettimanale.LoadAsync();
                Parti = _infrasettimanale.Parti;
            }

            if (Parti.Count > 0)
            {
                _currentParte = 0;
                Current = Parti[_currentParte];
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Avanti()
        {
            if (_currentParte >= Parti.Count - 1)
            {
                return;
            }
            else if (Current != null)
            {
                // Sottraggo il tempo della parte che ho skippato al tempo residuo 
                // CORREZIONE: Se ho risparmiato tempo (scorre > 0), lo AGGIUNGO al residuo.
                // Se ero in ritardo (scorre < 0), il timer ha già scalato il residuo, quindi non faccio nulla.
                if (Current.TempoScorrevole > TimeSpan.Zero)
                {
                    TempoResiduo += Current.TempoScorrevole;
                }

                _currentParte++;
                Current = Parti[_currentParte];
            }
        }

        public void Indietro()
        {
            if (_currentParte <= 0)
            {
                return;
            }
            else
            {
                _currentParte--;

                // Riaggiungo il tempo della parte skippata al tempo residuo
                // CORREZIONE: Inverto la logica di Avanti. Se avevo aggiunto tempo, ora lo tolgo.
                if (Parti[_currentParte].TempoScorrevole > TimeSpan.Zero)
                {
                    TempoResiduo -= Parti[_currentParte].TempoScorrevole;
                }

                Current = Parti[_currentParte];
            }
        }
    }
}