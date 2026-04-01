using InTempo.Classes.Utilities.Theming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace InTempo.Classes.View
{
    public partial class ThemeCustomizerWindow : Window, INotifyPropertyChanged
    {
        private sealed record ThemeFieldDefinition(string PropertyName, string Label, string Description);

        private static readonly ThemeFieldDefinition[] ThemeBaseFieldDefinitions =
        {
            new("AppWindowBackgroundColor", "Sfondo finestra", "Sfondo principale dell'app."),
            new("AppSurfaceColor", "Superficie principale", "Card e pannelli base."),
            new("AppSurfaceMutedColor", "Superficie secondaria", "Pannelli secondari e zone morbide."),
            new("AppSurfaceStrongColor", "Superficie forte", "Zone di footer o superfici piu marcate."),
            new("AppBorderColor", "Bordo principale", "Bordi piu evidenti."),
            new("AppSubtleBorderColor", "Bordo tenue", "Separatori e bordi discreti."),
            new("AppTextPrimaryColor", "Testo principale", "Titoli e testi principali."),
            new("AppMutedTextColor", "Testo secondario", "Label, caption e descrizioni."),
            new("AppAccentColor", "Accento", "Colore principale del tema."),
            new("AppAccentSoftColor", "Accento soft", "Pill, hover e superfici accentate leggere."),
            new("AppAccentDeepColor", "Accento scuro", "Bottoni primari e testi in evidenza."),
            new("AppDangerColor", "Danger", "Bordi e testi di errore principali."),
            new("AppDangerSoftColor", "Danger soft", "Sfondo leggero per azioni distruttive."),
            new("AppOverlayColor", "Overlay", "Oscuramento popup e overlay."),
            new("AppCurrentRowColor", "Riga corrente", "Highlight della riga corrente."),
            new("AppToolbarTrayBackgroundColor", "Tray strumenti", "Sfondo del tray strumenti."),
            new("AppDataGridHoverRowColor", "Hover DataGrid", "Hover righe della scaletta."),
            new("AppLoadingOverlayBackgroundColor", "Overlay caricamento", "Sfondo del loader centrale."),
            new("AppInputErrorColor", "Errore input", "Messaggi e bordi input non validi."),
            new("AppPresentationMessageColor", "Messaggio presentazione", "Testo messaggi dello schermo secondario.")
        };

        private static readonly ThemeFieldDefinition[] ThemeAdvancedGradientFieldDefinitions =
        {
            new("AppPresentationBackgroundStartColor", "Presentazione 1", "Primo stop dello sfondo presentazione."),
            new("AppPresentationBackgroundMidColor", "Presentazione 2", "Stop centrale dello sfondo presentazione."),
            new("AppPresentationBackgroundEndColor", "Presentazione 3", "Ultimo stop dello sfondo presentazione."),
            new("AppMainWindowBackdropStartColor", "Backdrop finestra 1", "Primo stop dello sfondo della main window."),
            new("AppMainWindowBackdropMidColor", "Backdrop finestra 2", "Stop centrale dello sfondo della main window."),
            new("AppMainWindowBackdropEndColor", "Backdrop finestra 3", "Ultimo stop dello sfondo della main window."),
            new("AppHeroSurfaceStartColor", "Hero surface 1", "Primo stop dei pannelli hero."),
            new("AppHeroSurfaceEndColor", "Hero surface 2", "Ultimo stop dei pannelli hero."),
            new("AppInsetSurfaceStartColor", "Inset surface 1", "Primo stop dei pannelli inset."),
            new("AppInsetSurfaceEndColor", "Inset surface 2", "Ultimo stop dei pannelli inset.")
        };

        private static readonly ThemeFieldDefinition[] ThemeAdvancedOrbFieldDefinitions =
        {
            new("AppBackdropOrbPrimaryInnerColor", "Orb primaria interna", "Centro dell'orb primaria di sfondo."),
            new("AppBackdropOrbPrimaryOuterColor", "Orb primaria esterna", "Bordo esterno dell'orb primaria."),
            new("AppBackdropOrbSecondaryInnerColor", "Orb secondaria interna", "Centro dell'orb secondaria di sfondo."),
            new("AppBackdropOrbSecondaryOuterColor", "Orb secondaria esterna", "Bordo esterno dell'orb secondaria."),
            new("AppBackdropOrbTertiaryInnerColor", "Orb terziaria interna", "Centro dell'orb terziaria di sfondo."),
            new("AppBackdropOrbTertiaryOuterColor", "Orb terziaria esterna", "Bordo esterno dell'orb terziaria.")
        };

        public ObservableCollection<ThemeColorPickerItem> ColoriTemaBase { get; } = new();
        public ObservableCollection<ThemeColorPickerItem> ColoriTemaAvanzatiGradienti { get; } = new();
        public ObservableCollection<ThemeColorPickerItem> ColoriTemaAvanzatiOrb { get; } = new();

        public CustomThemePalette ResultPalette { get; private set; } = ThemeManager.CreateDefaultCustomTheme();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ThemeCustomizerWindow(CustomThemePalette palette)
        {
            InitializeComponent();
            DataContext = this;

            CustomThemePalette safePalette = palette.Clone();
            safePalette.Normalizza();

            ResultPalette = safePalette.Clone();
            CaricaPalette(safePalette);
            ApplicaAnteprima();
        }

        private void CaricaPalette(CustomThemePalette palette)
        {
            SostituisciItems(ColoriTemaBase, ThemeBaseFieldDefinitions, palette);
            SostituisciItems(ColoriTemaAvanzatiGradienti, ThemeAdvancedGradientFieldDefinitions, palette);
            SostituisciItems(ColoriTemaAvanzatiOrb, ThemeAdvancedOrbFieldDefinitions, palette);
        }

        private void SostituisciItems(
            ObservableCollection<ThemeColorPickerItem> target,
            IEnumerable<ThemeFieldDefinition> definitions,
            CustomThemePalette palette)
        {
            foreach (ThemeColorPickerItem item in target)
            {
                item.PropertyChanged -= ThemeColorPickerItem_PropertyChanged;
            }

            target.Clear();

            foreach (ThemeFieldDefinition definition in definitions)
            {
                ThemeColorPickerItem item = new(
                    definition.PropertyName,
                    definition.Label,
                    definition.Description,
                    LeggiProprietaTema(palette, definition.PropertyName));

                item.PropertyChanged += ThemeColorPickerItem_PropertyChanged;
                target.Add(item);
            }
        }

        private void ThemeColorPickerItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeColorPickerItem.SelectedColor))
            {
                ApplicaAnteprima();
            }
        }

        private void ApplicaAnteprima()
        {
            CustomThemePalette palette = CostruisciPaletteCorrente();
            ThemeManager.ApplyPaletteToResources(PreviewScope.Resources, ThemeManager.CustomThemeKey, palette);
            ResultPalette = palette;
            OnPropertyChanged(nameof(ResultPalette));
        }

        private CustomThemePalette CostruisciPaletteCorrente()
        {
            CustomThemePalette palette = ThemeManager.CreateDefaultCustomTheme();

            foreach (ThemeColorPickerItem item in GetAllItems())
            {
                if (!string.IsNullOrWhiteSpace(item.HexValue))
                {
                    ScriviProprietaTema(palette, item.PropertyName, item.HexValue);
                }
            }

            palette.Normalizza();
            return palette;
        }

        private IEnumerable<ThemeColorPickerItem> GetAllItems()
        {
            return ColoriTemaBase
                .Concat(ColoriTemaAvanzatiGradienti)
                .Concat(ColoriTemaAvanzatiOrb);
        }

        private static string LeggiProprietaTema(CustomThemePalette palette, string propertyName)
        {
            PropertyInfo? property = typeof(CustomThemePalette).GetProperty(propertyName);
            return property?.GetValue(palette) as string ?? string.Empty;
        }

        private static void ScriviProprietaTema(CustomThemePalette palette, string propertyName, string value)
        {
            PropertyInfo? property = typeof(CustomThemePalette).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(palette, value);
            }
        }

        private void BtnResetTemaPersonalizzato_Click(object sender, RoutedEventArgs e)
        {
            CustomThemePalette palette = ThemeManager.CreateDefaultCustomTheme();
            palette.Normalizza();
            CaricaPalette(palette);
            ApplicaAnteprima();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            ResultPalette = CostruisciPaletteCorrente();
            DialogResult = true;
            Close();
        }

        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
