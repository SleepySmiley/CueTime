using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InTempo.Classes.NonAbstract
{
    public class Adunanza : INotifyPropertyChanged
    {
        private Finesettimanale _finesettimana = new Finesettimanale();
        private Infrasettimanale _infrasettimanale = new Infrasettimanale();
        private Sorvegliante_Infrasettimanale _sorveglianteInfrasettimanale = new Sorvegliante_Infrasettimanale();
        private Sorvegliante_Finesettimanale _sorveglianteFinesettimanale = new Sorvegliante_Finesettimanale();


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

        private int _currentParteIndex = 0;
        private Parte? _current;

        [JsonIgnore]
        public Parte? Current
        {
            get => _current;
            set
            {
                if (ReferenceEquals(_current, value))
                    return;

                if (_current != null)
                    _current.IsCurrent = false;

                _current = value;

                if (_current != null)
                    _current.IsCurrent = true;

                OnPropertyChanged();
            }
        }


        private TimeSpan _tempoResiduo;

        [JsonIgnore]
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
        [JsonIgnore]
        public string TempoResiduoString
        {
            get
            {
                var ts = TempoResiduo;
                var sign = ts > TimeSpan.Zero ? "-" : ts < TimeSpan.Zero ? "+" : "";
                ts = ts.Duration();

                int totalMinutes = (int)ts.TotalMinutes;
                int seconds = ts.Seconds;

                return $"{sign}{totalMinutes}:{seconds:00}";
            }
        }


        public async Task SelectedAdunanza()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            Parti.Clear();

            if (today == DayOfWeek.Sunday || today == DayOfWeek.Saturday)
            {
                if(DateTime.Today == App.Settings.DateVisitaSorvegliante[1])
                {
                    await _sorveglianteInfrasettimanale.CaricaSchema();
                    Parti = _sorveglianteInfrasettimanale.Parti;
                    return;
                }
                await _finesettimana.LoadAsync();
                Parti = _finesettimana.Parti;
            }
            else
            {
                if(DateTime.Today == App.Settings.DateVisitaSorvegliante[0])
                {
                    await _sorveglianteFinesettimanale.CaricaSchema();
                    Parti = _sorveglianteFinesettimanale.Parti;
                    return;
                }
                await _infrasettimanale.LoadAsync();
                Parti = _infrasettimanale.Parti;
            }

            if (Parti.Count > 0)
            {
                _currentParteIndex = 0;
                Current = Parti[_currentParteIndex];
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Avanti()
        {
            

            if (Parti.Count == 0)
            {
                Current = null;
                _currentParteIndex = 0;
                return;
            }

            int idx = Current != null ? Parti.IndexOf(Current) : -1;
            if (idx < 0)
            {
                _currentParteIndex = 0;
                Current = Parti[0];
                return;
            }

            if (idx >= Parti.Count - 1)
                return;

            if (Current!.TempoScorrevole > TimeSpan.Zero)
                TempoResiduo += Current.TempoScorrevole;

            _currentParteIndex = idx + 1;
            Current = Parti[_currentParteIndex];
        }

        public void Indietro()
        {
            if (Parti.Count == 0)
            {
                Current = null;
                _currentParteIndex = 0;
                return;
            }

            int idx = Current != null ? Parti.IndexOf(Current) : -1;
            if (idx < 0)
            {
                _currentParteIndex = 0;
                Current = Parti[0];
                return;
            }

            if (idx <= 0)
                return;

            _currentParteIndex = idx - 1;

            if (Parti[_currentParteIndex].TempoScorrevole > TimeSpan.Zero)
                TempoResiduo -= Parti[_currentParteIndex].TempoScorrevole;

            Current = Parti[_currentParteIndex];
        }

       

    }
}