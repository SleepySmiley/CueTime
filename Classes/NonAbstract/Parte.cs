using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.NonAbstract
{
    public class Parte : INotifyPropertyChanged
    {
        private int? _numeroParte;
        public int? NumeroParte
        {
            get => _numeroParte;
            set
            {
                if (_numeroParte == value) return;
                _numeroParte = value;
                OnPropertyChanged();
            }
        }

        private string _nomeParte = string.Empty;
        public string NomeParte
        {
            get => _nomeParte;
            set
            {
                if (_nomeParte == value) return;
                _nomeParte = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        private TimeSpan _tempoParte;
        public TimeSpan TempoParte
        {
            get => _tempoParte;
            set
            {
                if (_tempoParte == value) return;
                _tempoParte = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TempoParteLabel));
                OnPropertyChanged(nameof(TempoParteEdit));
            }
        }

        private string _tipoParte = string.Empty;
        public string TipoParte
        {
            get => _tipoParte;
            set
            {
                if (_tipoParte == value) return;
                _tipoParte = value ?? string.Empty;
                OnPropertyChanged();
            }
        }


        private string _coloreSalvato = "#FF000000"; 

        public string ColoreSalvato
        {
            get => _coloreSalvato;
            set
            {
                if (_coloreSalvato == value) return;
                _coloreSalvato = value;

                try
                {
                    if (new BrushConverter().ConvertFromString(_coloreSalvato) is Brush converted)
                    {
                        _coloreParte = converted;
                        OnPropertyChanged(nameof(ColoreParte));
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning($"Impossibile convertire il colore salvato '{_coloreSalvato}' per la parte '{_nomeParte}'.", ex);
                }
            }
        }

        [JsonIgnore]
        private Brush _coloreParte = Brushes.Black;

        [JsonIgnore]
        public Brush ColoreParte
        {
            get => _coloreParte;
            set
            {
                if (Equals(_coloreParte, value)) return;
                _coloreParte = value ?? Brushes.Black;

                if (_coloreParte is SolidColorBrush solidBrush)
                {
                    _coloreSalvato = solidBrush.Color.ToString();
                }

                OnPropertyChanged();
            }
        }

        private TimeSpan _tempoScorrevole;
        public TimeSpan TempoScorrevole
        {
            get => _tempoScorrevole;
            set
            {
                if (_tempoScorrevole == value) return;
                _tempoScorrevole = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TempoScorrevoleLabel));
            }
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                if (_isCurrent == value) return;
                _isCurrent = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string TempoParteLabel => ToMinSec(TempoParte);

        [JsonIgnore]
        public string TempoScorrevoleLabel => ToMinSec(TempoScorrevole);

        [JsonIgnore]
        public string TempoParteEdit
        {
            get => ToMinSec(TempoParte);
            set
            {
                if (TryParseMinSec(value, out var ts))
                    TempoParte = ts;
                else
                    OnPropertyChanged(nameof(TempoParteEdit));
            }
        }

        public Parte(string nome, TimeSpan tempoParte, string tipo, Brush colore, TimeSpan tempoScorrevole, int? numeroParte)
        {
            _nomeParte = nome ?? string.Empty;
            _tempoParte = tempoParte;
            _tipoParte = tipo ?? string.Empty;
            ColoreParte = colore ?? Brushes.Black;
            _tempoScorrevole = tempoScorrevole;
            _numeroParte = numeroParte;
        }

        public Parte() { }

        private static string ToMinSec(TimeSpan ts)
        {
            bool neg = ts < TimeSpan.Zero;
            if (neg) ts = ts.Negate();

            int minutes = (int)ts.TotalMinutes;
            int seconds = ts.Seconds;

            return (neg ? "-" : "") + $"{minutes:00}:{seconds:00}";
        }

        private static bool TryParseMinSec(string? input, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string s = input.Trim();
            if (s.StartsWith("-", StringComparison.Ordinal))
                return false;

            string[] parts = s.Split(':');

            int minutes;
            int seconds = 0;

            if (parts.Length == 1)
            {
                if (!int.TryParse(parts[0], out minutes) || minutes < 0) return false;
            }
            else if (parts.Length == 2)
            {
                if (!int.TryParse(parts[0], out minutes) || minutes < 0) return false;
                if (!int.TryParse(parts[1], out seconds) || seconds < 0 || seconds > 59) return false;
            }
            else
            {
                return false;
            }

            result = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
