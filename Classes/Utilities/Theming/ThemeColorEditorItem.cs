using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace InTempo.Classes.Utilities.Theming
{
    public sealed class ThemeColorEditorItem : INotifyPropertyChanged
    {
        private string _value;
        private Brush _previewBrush = Brushes.Transparent;
        private bool _isValid;
        private string _normalizedValue = string.Empty;

        public ThemeColorEditorItem(string propertyName, string label, string description, string value)
        {
            PropertyName = propertyName;
            Label = label;
            Description = description;
            _value = value;
            Refresh();
        }

        public string PropertyName { get; }
        public string Label { get; }
        public string Description { get; }

        public string Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                _value = value;
                Refresh();
                OnPropertyChanged();
            }
        }

        public Brush PreviewBrush
        {
            get => _previewBrush;
            private set
            {
                _previewBrush = value;
                OnPropertyChanged();
            }
        }

        public bool IsValid
        {
            get => _isValid;
            private set
            {
                _isValid = value;
                OnPropertyChanged();
            }
        }

        public string NormalizedValue
        {
            get => _normalizedValue;
            private set
            {
                _normalizedValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Refresh()
        {
            if (ThemeManager.TryNormalizeColor(Value, out string normalized))
            {
                Color color = (Color)ColorConverter.ConvertFromString(normalized)!;
                PreviewBrush = new SolidColorBrush(color);
                NormalizedValue = normalized;
                IsValid = true;
                return;
            }

            PreviewBrush = Brushes.Transparent;
            NormalizedValue = string.Empty;
            IsValid = false;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
