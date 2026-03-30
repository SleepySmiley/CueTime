using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CueTime.Classes.Utilities.Theming
{
    public sealed class ThemeColorPickerItem : INotifyPropertyChanged
    {
        private Color? _selectedColor;
        private Brush _previewBrush = Brushes.Transparent;
        private string _hexValue = string.Empty;

        public ThemeColorPickerItem(string propertyName, string label, string description, string initialColor)
        {
            PropertyName = propertyName;
            Label = label;
            Description = description;

            if (ThemeManager.TryNormalizeColor(initialColor, out string normalized))
            {
                SelectedColor = (Color)ColorConverter.ConvertFromString(normalized)!;
            }
        }

        public string PropertyName { get; }
        public string Label { get; }
        public string Description { get; }

        public Color? SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor == value)
                {
                    return;
                }

                _selectedColor = value;
                RefreshDerivedState();
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

        public string HexValue
        {
            get => _hexValue;
            private set
            {
                _hexValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RefreshDerivedState()
        {
            if (SelectedColor is Color color)
            {
                PreviewBrush = new SolidColorBrush(color);
                HexValue = color.ToString();
                return;
            }

            PreviewBrush = Brushes.Transparent;
            HexValue = string.Empty;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

