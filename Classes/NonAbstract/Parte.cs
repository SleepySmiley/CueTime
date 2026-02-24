using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;

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

        public string TempoParteLabel => ToMinSec(TempoParte);
        public string TempoScorrevoleLabel => ToMinSec(TempoScorrevole);

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
            _coloreParte = colore ?? Brushes.Black;
            _tempoScorrevole = tempoScorrevole;
            _numeroParte = numeroParte;
        }

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

            bool neg = false;
            if (s.StartsWith("-", StringComparison.Ordinal))
            {
                neg = true;
                s = s.Substring(1).Trim();
                if (s.Length == 0) return false;
            }

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
            if (neg) result = -result;
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}