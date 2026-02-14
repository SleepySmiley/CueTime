using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
                if (_numeroParte != value)
                {
                    _numeroParte = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _nomeParte;
        public string NomeParte
        {
            get => _nomeParte;
            set
            {
                if (_nomeParte != value)
                {
                    _nomeParte = value;
                    OnPropertyChanged();
                }
            }
        }

        private TimeSpan _tempoParte;
        public TimeSpan TempoParte
        {
            get => _tempoParte;
            set
            {
                if (_tempoParte != value)
                {
                    _tempoParte = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _tipoParte;
        public string TipoParte
        {
            get => _tipoParte;
            set
            {
                if (_tipoParte != value)
                {
                    _tipoParte = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush _coloreParte;
        public Brush ColoreParte
        {
            get => _coloreParte;
            set
            {
                if (_coloreParte != value)
                {
                    _coloreParte = value;
                    OnPropertyChanged();
                }
            }
        }

        private TimeSpan _tempoScorrevole;
        public TimeSpan TempoScorrevole
        {
            get => _tempoScorrevole;
            set
            {
                if (_tempoScorrevole != value)
                {
                    _tempoScorrevole = value;
                    OnPropertyChanged();
                }
            }
        }

        public Parte(string nome, TimeSpan t, string tipo, Brush colore, TimeSpan tempoScorrevole, int? numeroParte)
        {
            NomeParte = nome;
            TempoParte = t;
            TipoParte = tipo;
            ColoreParte = colore;
            TempoScorrevole = tempoScorrevole;
            NumeroParte = numeroParte;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}