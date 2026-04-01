using System.Windows.Media;

namespace InTempo.Classes.Utilities.Theming
{
    public sealed class AppThemeDefinition
    {
        public AppThemeDefinition(
            string key,
            string displayName,
            string description,
            string palettePath,
            string previewBackdropColor,
            string previewSurfaceColor,
            string previewAccentColor)
        {
            Key = key;
            DisplayName = displayName;
            Description = description;
            PalettePath = palettePath;
            PreviewBackdropBrush = CreateBrush(previewBackdropColor);
            PreviewSurfaceBrush = CreateBrush(previewSurfaceColor);
            PreviewAccentBrush = CreateBrush(previewAccentColor);
        }

        public string Key { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string PalettePath { get; }

        public Brush PreviewBackdropBrush { get; }
        public Brush PreviewSurfaceBrush { get; }
        public Brush PreviewAccentBrush { get; }

        private static Brush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
        }
    }
}
