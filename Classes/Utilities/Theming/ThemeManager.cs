using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.Utilities.Theming
{
    public static class ThemeManager
    {
        public const string DefaultThemeKey = "terracotta-calda";
        public const string CustomThemeKey = "personalizzato";

        private static readonly IReadOnlyList<AppThemeDefinition> _builtInThemes = new[]
        {
            new AppThemeDefinition(
                "terracotta-calda",
                "Terracotta Calda",
                "La palette attuale: morbida, elegante e avvolgente.",
                "Themes/Palettes/TerracottaCalda.xaml",
                "#FFF7F0E8",
                "#FFFDF9F4",
                "#FFB56A41"),
            new AppThemeDefinition(
                "petrolio-moderna",
                "Petrolio Moderna",
                "Piu tecnica e pulita, con accento verde petrolio.",
                "Themes/Palettes/PetrolioModerna.xaml",
                "#FFF0F5F4",
                "#FFFCFEFD",
                "#FF2E7A78"),
            new AppThemeDefinition(
                "salvia-morbida",
                "Salvia Morbida",
                "Naturale e rilassata, con toni sabbia e verde salvia.",
                "Themes/Palettes/SalviaMorbida.xaml",
                "#FFF6F1E8",
                "#FFFFFCF7",
                "#FF76845F"),
            new AppThemeDefinition(
                "ardesia-bronzo",
                "Ardesia Bronzo",
                "Neutra e raffinata, con superfici fredde e accento bronzo.",
                "Themes/Palettes/ArdesiaBronzo.xaml",
                "#FFF2F4F6",
                "#FFFEFEFE",
                "#FF9B6848")
        };

        public static IReadOnlyList<AppThemeDefinition> AvailableThemes => GetAvailableThemes();

        public static CustomThemePalette CreateDefaultCustomTheme()
        {
            return new CustomThemePalette();
        }

        public static IReadOnlyList<AppThemeDefinition> GetAvailableThemes(CustomThemePalette? customTheme = null)
        {
            List<AppThemeDefinition> themes = _builtInThemes.ToList();
            themes.Add(CreateCustomThemeDefinition(customTheme));
            return themes;
        }

        public static AppThemeDefinition GetThemeOrDefault(string? key, CustomThemePalette? customTheme = null)
        {
            if (string.Equals(key, CustomThemeKey, StringComparison.OrdinalIgnoreCase))
            {
                return CreateCustomThemeDefinition(customTheme);
            }

            return _builtInThemes.FirstOrDefault(theme =>
                       string.Equals(theme.Key, key, StringComparison.OrdinalIgnoreCase))
                   ?? _builtInThemes.First(theme => theme.Key == DefaultThemeKey);
        }

        public static string ApplyTheme(string? key, CustomThemePalette? customTheme = null)
        {
            if (string.Equals(key, CustomThemeKey, StringComparison.OrdinalIgnoreCase))
            {
                CustomThemePalette palette = (customTheme ?? CreateDefaultCustomTheme()).Clone();
                palette.Normalizza();

                if (Application.Current is null)
                {
                    return CustomThemeKey;
                }

                if (TryApplyPalette(BuildCustomPalette(palette)))
                {
                    return CustomThemeKey;
                }

                TryApplyPalette(LoadPalette(GetThemeOrDefault(DefaultThemeKey)));
                return DefaultThemeKey;
            }

            AppThemeDefinition theme = GetThemeOrDefault(key);
            if (Application.Current is null)
            {
                return theme.Key;
            }

            if (TryApplyPalette(LoadPalette(theme)))
            {
                return theme.Key;
            }

            TryApplyPalette(LoadPalette(GetThemeOrDefault(DefaultThemeKey)));
            return DefaultThemeKey;
        }

        public static ResourceDictionary CreatePaletteResources(string? key, CustomThemePalette? customTheme = null)
        {
            if (string.Equals(key, CustomThemeKey, StringComparison.OrdinalIgnoreCase))
            {
                CustomThemePalette palette = (customTheme ?? CreateDefaultCustomTheme()).Clone();
                palette.Normalizza();
                return BuildCustomPalette(palette);
            }

            AppThemeDefinition theme = GetThemeOrDefault(key);
            return LoadPalette(theme);
        }

        public static void ApplyPaletteToResources(ResourceDictionary targetResources, string? key, CustomThemePalette? customTheme = null)
        {
            ResourceDictionary palette = CreatePaletteResources(key, customTheme);

            foreach (object rawKey in palette.Keys)
            {
                if (rawKey is not string resourceKey)
                {
                    continue;
                }

                object incoming = palette[resourceKey];
                targetResources[resourceKey] = CloneIfNeeded(incoming);
            }
        }

        public static string NormalizeColorOrDefault(string? value, string fallback)
        {
            return TryNormalizeColor(value, out string normalized)
                ? normalized
                : NormalizeColorOrDefault(fallback, "#FFFFFFFF");
        }

        public static bool TryNormalizeColor(string? value, out string normalized)
        {
            normalized = string.Empty;

            string candidate = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return false;
            }

            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(candidate)!;
                normalized = color.ToString();
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile normalizzare il colore '{candidate}'.", ex);
                return false;
            }
        }

        private static AppThemeDefinition CreateCustomThemeDefinition(CustomThemePalette? customTheme)
        {
            CustomThemePalette palette = (customTheme ?? CreateDefaultCustomTheme()).Clone();
            palette.Normalizza();

            return new AppThemeDefinition(
                CustomThemeKey,
                "Personalizzato",
                "Totale: colori base, gradienti, orb e supporto avanzato.",
                string.Empty,
                palette.AppMainWindowBackdropStartColor,
                palette.AppSurfaceColor,
                palette.AppAccentColor);
        }

        private static bool TryApplyPalette(ResourceDictionary palette)
        {
            if (Application.Current is null)
            {
                return false;
            }

            try
            {
                foreach (object rawKey in palette.Keys)
                {
                    if (rawKey is not string resourceKey)
                    {
                        continue;
                    }

                    object incoming = palette[resourceKey];
                    Application.Current.Resources[resourceKey] = CloneIfNeeded(incoming);
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Errore durante l'applicazione della palette tema alle risorse dell'applicazione.", ex);
                return false;
            }
        }

        private static ResourceDictionary LoadPalette(AppThemeDefinition theme)
        {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "InTempo";
            string uri = $"pack://application:,,,/{assemblyName};component/{theme.PalettePath}";

            return new ResourceDictionary
            {
                Source = new Uri(uri, UriKind.Absolute)
            };
        }

        private static ResourceDictionary BuildCustomPalette(CustomThemePalette palette)
        {
            palette.Normalizza();

            ResourceDictionary paletteDictionary = new ResourceDictionary();
            AddSolidBrush(paletteDictionary, "AppWindowBackgroundBrush", palette.AppWindowBackgroundColor);
            AddSolidBrush(paletteDictionary, "AppSurfaceBrush", palette.AppSurfaceColor);
            AddSolidBrush(paletteDictionary, "AppSurfaceMutedBrush", palette.AppSurfaceMutedColor);
            AddSolidBrush(paletteDictionary, "AppSurfaceStrongBrush", palette.AppSurfaceStrongColor);
            AddSolidBrush(paletteDictionary, "AppBorderBrush", palette.AppBorderColor);
            AddSolidBrush(paletteDictionary, "AppSubtleBorderBrush", palette.AppSubtleBorderColor);
            AddSolidBrush(paletteDictionary, "AppTextPrimaryBrush", palette.AppTextPrimaryColor);
            AddSolidBrush(paletteDictionary, "AppMutedTextBrush", palette.AppMutedTextColor);
            AddSolidBrush(paletteDictionary, "AppAccentBrush", palette.AppAccentColor);
            AddSolidBrush(paletteDictionary, "AppAccentSoftBrush", palette.AppAccentSoftColor);
            AddSolidBrush(paletteDictionary, "AppAccentDeepBrush", palette.AppAccentDeepColor);
            AddSolidBrush(paletteDictionary, "AppDangerBrush", palette.AppDangerColor);
            AddSolidBrush(paletteDictionary, "AppDangerSoftBrush", palette.AppDangerSoftColor);
            AddSolidBrush(paletteDictionary, "AppOverlayBrush", palette.AppOverlayColor);
            AddSolidBrush(paletteDictionary, "AppCurrentRowBrush", palette.AppCurrentRowColor);
            AddSolidBrush(paletteDictionary, "AppToolbarTrayBackgroundBrush", palette.AppToolbarTrayBackgroundColor);
            AddSolidBrush(paletteDictionary, "AppDataGridHoverRowBrush", palette.AppDataGridHoverRowColor);
            AddSolidBrush(paletteDictionary, "AppLoadingOverlayBackgroundBrush", palette.AppLoadingOverlayBackgroundColor);
            AddSolidBrush(paletteDictionary, "AppInputErrorBrush", palette.AppInputErrorColor);
            AddSolidBrush(paletteDictionary, "AppPresentationMessageBrush", palette.AppPresentationMessageColor);

            paletteDictionary["AppPresentationBackgroundBrush"] = CreateLinearGradientBrush(
                (palette.AppPresentationBackgroundStartColor, 0d),
                (palette.AppPresentationBackgroundMidColor, 0.55d),
                (palette.AppPresentationBackgroundEndColor, 1d));

            paletteDictionary["AppMainWindowBackdropBrush"] = CreateLinearGradientBrush(
                (palette.AppMainWindowBackdropStartColor, 0d),
                (palette.AppMainWindowBackdropMidColor, 0.48d),
                (palette.AppMainWindowBackdropEndColor, 1d));

            paletteDictionary["AppHeroSurfaceBrush"] = CreateLinearGradientBrush(
                (palette.AppHeroSurfaceStartColor, 0d),
                (palette.AppHeroSurfaceEndColor, 1d));

            paletteDictionary["AppInsetSurfaceBrush"] = CreateLinearGradientBrush(
                (palette.AppInsetSurfaceStartColor, 0d),
                (palette.AppInsetSurfaceEndColor, 1d));

            paletteDictionary["AppBackdropOrbPrimaryBrush"] = CreateRadialGradientBrush(
                palette.AppBackdropOrbPrimaryInnerColor,
                palette.AppBackdropOrbPrimaryOuterColor);

            paletteDictionary["AppBackdropOrbSecondaryBrush"] = CreateRadialGradientBrush(
                palette.AppBackdropOrbSecondaryInnerColor,
                palette.AppBackdropOrbSecondaryOuterColor);

            paletteDictionary["AppBackdropOrbTertiaryBrush"] = CreateRadialGradientBrush(
                palette.AppBackdropOrbTertiaryInnerColor,
                palette.AppBackdropOrbTertiaryOuterColor);

            return paletteDictionary;
        }

        private static void AddSolidBrush(ResourceDictionary paletteDictionary, string key, string color)
        {
            paletteDictionary[key] = new SolidColorBrush(ParseColor(color));
        }

        private static LinearGradientBrush CreateLinearGradientBrush(params (string Color, double Offset)[] stops)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            foreach ((string color, double offset) in stops)
            {
                brush.GradientStops.Add(new GradientStop(ParseColor(color), offset));
            }

            return brush;
        }

        private static RadialGradientBrush CreateRadialGradientBrush(string innerColor, string outerColor)
        {
            RadialGradientBrush brush = new RadialGradientBrush
            {
                RadiusX = 0.8,
                RadiusY = 0.8
            };

            brush.GradientStops.Add(new GradientStop(ParseColor(innerColor), 0));
            brush.GradientStops.Add(new GradientStop(ParseColor(outerColor), 1));
            return brush;
        }

        private static Color ParseColor(string color)
        {
            return (Color)ColorConverter.ConvertFromString(color)!;
        }

        private static object CloneIfNeeded(object resource)
        {
            if (resource is Freezable freezable)
            {
                return freezable.CloneCurrentValue();
            }

            return resource;
        }
    }
}
