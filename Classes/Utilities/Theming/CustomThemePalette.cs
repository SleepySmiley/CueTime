namespace InTempo.Classes.Utilities.Theming
{
    public sealed class CustomThemePalette
    {
        public string AppWindowBackgroundColor { get; set; } = "#FFF6F0E9";
        public string AppSurfaceColor { get; set; } = "#FFFDF9F4";
        public string AppSurfaceMutedColor { get; set; } = "#FFF7F0E9";
        public string AppSurfaceStrongColor { get; set; } = "#FFEDE1D3";
        public string AppBorderColor { get; set; } = "#FFD7C8B7";
        public string AppSubtleBorderColor { get; set; } = "#FFE8DDD1";
        public string AppTextPrimaryColor { get; set; } = "#FF172131";
        public string AppMutedTextColor { get; set; } = "#FF72665D";
        public string AppAccentColor { get; set; } = "#FFB56A41";
        public string AppAccentSoftColor { get; set; } = "#FFF6E6D8";
        public string AppAccentDeepColor { get; set; } = "#FF51362A";
        public string AppDangerColor { get; set; } = "#FFCC4B4B";
        public string AppDangerSoftColor { get; set; } = "#14CC4B4B";
        public string AppOverlayColor { get; set; } = "#7A0E1724";
        public string AppCurrentRowColor { get; set; } = "#FFF6EBDD";
        public string AppToolbarTrayBackgroundColor { get; set; } = "#E6FFF9F2";
        public string AppDataGridHoverRowColor { get; set; } = "#55FFFFFF";
        public string AppLoadingOverlayBackgroundColor { get; set; } = "#F2FFF8F2";
        public string AppInputErrorColor { get; set; } = "#FFEF5350";
        public string AppPresentationMessageColor { get; set; } = "#FFE7F0FB";

        public string AppPresentationBackgroundStartColor { get; set; } = "#FF081019";
        public string AppPresentationBackgroundMidColor { get; set; } = "#FF0E1B2D";
        public string AppPresentationBackgroundEndColor { get; set; } = "#FF14293B";
        public string AppMainWindowBackdropStartColor { get; set; } = "#FFF7F0E8";
        public string AppMainWindowBackdropMidColor { get; set; } = "#FFF1E7DD";
        public string AppMainWindowBackdropEndColor { get; set; } = "#FFF7F3EE";
        public string AppHeroSurfaceStartColor { get; set; } = "#FFFFFAF5";
        public string AppHeroSurfaceEndColor { get; set; } = "#FFF6EDE3";
        public string AppInsetSurfaceStartColor { get; set; } = "#F7FFFFFF";
        public string AppInsetSurfaceEndColor { get; set; } = "#FFF8F3ED";
        public string AppBackdropOrbPrimaryInnerColor { get; set; } = "#66E6BB97";
        public string AppBackdropOrbPrimaryOuterColor { get; set; } = "#00E6BB97";
        public string AppBackdropOrbSecondaryInnerColor { get; set; } = "#55B4C2D8";
        public string AppBackdropOrbSecondaryOuterColor { get; set; } = "#00B4C2D8";
        public string AppBackdropOrbTertiaryInnerColor { get; set; } = "#44F3D8BF";
        public string AppBackdropOrbTertiaryOuterColor { get; set; } = "#00F3D8BF";

        public CustomThemePalette Clone()
        {
            return new CustomThemePalette
            {
                AppWindowBackgroundColor = AppWindowBackgroundColor,
                AppSurfaceColor = AppSurfaceColor,
                AppSurfaceMutedColor = AppSurfaceMutedColor,
                AppSurfaceStrongColor = AppSurfaceStrongColor,
                AppBorderColor = AppBorderColor,
                AppSubtleBorderColor = AppSubtleBorderColor,
                AppTextPrimaryColor = AppTextPrimaryColor,
                AppMutedTextColor = AppMutedTextColor,
                AppAccentColor = AppAccentColor,
                AppAccentSoftColor = AppAccentSoftColor,
                AppAccentDeepColor = AppAccentDeepColor,
                AppDangerColor = AppDangerColor,
                AppDangerSoftColor = AppDangerSoftColor,
                AppOverlayColor = AppOverlayColor,
                AppCurrentRowColor = AppCurrentRowColor,
                AppToolbarTrayBackgroundColor = AppToolbarTrayBackgroundColor,
                AppDataGridHoverRowColor = AppDataGridHoverRowColor,
                AppLoadingOverlayBackgroundColor = AppLoadingOverlayBackgroundColor,
                AppInputErrorColor = AppInputErrorColor,
                AppPresentationMessageColor = AppPresentationMessageColor,
                AppPresentationBackgroundStartColor = AppPresentationBackgroundStartColor,
                AppPresentationBackgroundMidColor = AppPresentationBackgroundMidColor,
                AppPresentationBackgroundEndColor = AppPresentationBackgroundEndColor,
                AppMainWindowBackdropStartColor = AppMainWindowBackdropStartColor,
                AppMainWindowBackdropMidColor = AppMainWindowBackdropMidColor,
                AppMainWindowBackdropEndColor = AppMainWindowBackdropEndColor,
                AppHeroSurfaceStartColor = AppHeroSurfaceStartColor,
                AppHeroSurfaceEndColor = AppHeroSurfaceEndColor,
                AppInsetSurfaceStartColor = AppInsetSurfaceStartColor,
                AppInsetSurfaceEndColor = AppInsetSurfaceEndColor,
                AppBackdropOrbPrimaryInnerColor = AppBackdropOrbPrimaryInnerColor,
                AppBackdropOrbPrimaryOuterColor = AppBackdropOrbPrimaryOuterColor,
                AppBackdropOrbSecondaryInnerColor = AppBackdropOrbSecondaryInnerColor,
                AppBackdropOrbSecondaryOuterColor = AppBackdropOrbSecondaryOuterColor,
                AppBackdropOrbTertiaryInnerColor = AppBackdropOrbTertiaryInnerColor,
                AppBackdropOrbTertiaryOuterColor = AppBackdropOrbTertiaryOuterColor
            };
        }

        public void Normalizza()
        {
            AppWindowBackgroundColor = ThemeManager.NormalizeColorOrDefault(AppWindowBackgroundColor, "#FFF6F0E9");
            AppSurfaceColor = ThemeManager.NormalizeColorOrDefault(AppSurfaceColor, "#FFFDF9F4");
            AppSurfaceMutedColor = ThemeManager.NormalizeColorOrDefault(AppSurfaceMutedColor, "#FFF7F0E9");
            AppSurfaceStrongColor = ThemeManager.NormalizeColorOrDefault(AppSurfaceStrongColor, "#FFEDE1D3");
            AppBorderColor = ThemeManager.NormalizeColorOrDefault(AppBorderColor, "#FFD7C8B7");
            AppSubtleBorderColor = ThemeManager.NormalizeColorOrDefault(AppSubtleBorderColor, "#FFE8DDD1");
            AppTextPrimaryColor = ThemeManager.NormalizeColorOrDefault(AppTextPrimaryColor, "#FF172131");
            AppMutedTextColor = ThemeManager.NormalizeColorOrDefault(AppMutedTextColor, "#FF72665D");
            AppAccentColor = ThemeManager.NormalizeColorOrDefault(AppAccentColor, "#FFB56A41");
            AppAccentSoftColor = ThemeManager.NormalizeColorOrDefault(AppAccentSoftColor, "#FFF6E6D8");
            AppAccentDeepColor = ThemeManager.NormalizeColorOrDefault(AppAccentDeepColor, "#FF51362A");
            AppDangerColor = ThemeManager.NormalizeColorOrDefault(AppDangerColor, "#FFCC4B4B");
            AppDangerSoftColor = ThemeManager.NormalizeColorOrDefault(AppDangerSoftColor, "#14CC4B4B");
            AppOverlayColor = ThemeManager.NormalizeColorOrDefault(AppOverlayColor, "#7A0E1724");
            AppCurrentRowColor = ThemeManager.NormalizeColorOrDefault(AppCurrentRowColor, "#FFF6EBDD");
            AppToolbarTrayBackgroundColor = ThemeManager.NormalizeColorOrDefault(AppToolbarTrayBackgroundColor, "#E6FFF9F2");
            AppDataGridHoverRowColor = ThemeManager.NormalizeColorOrDefault(AppDataGridHoverRowColor, "#55FFFFFF");
            AppLoadingOverlayBackgroundColor = ThemeManager.NormalizeColorOrDefault(AppLoadingOverlayBackgroundColor, "#F2FFF8F2");
            AppInputErrorColor = ThemeManager.NormalizeColorOrDefault(AppInputErrorColor, "#FFEF5350");
            AppPresentationMessageColor = ThemeManager.NormalizeColorOrDefault(AppPresentationMessageColor, "#FFE7F0FB");

            AppPresentationBackgroundStartColor = ThemeManager.NormalizeColorOrDefault(AppPresentationBackgroundStartColor, "#FF081019");
            AppPresentationBackgroundMidColor = ThemeManager.NormalizeColorOrDefault(AppPresentationBackgroundMidColor, "#FF0E1B2D");
            AppPresentationBackgroundEndColor = ThemeManager.NormalizeColorOrDefault(AppPresentationBackgroundEndColor, "#FF14293B");
            AppMainWindowBackdropStartColor = ThemeManager.NormalizeColorOrDefault(AppMainWindowBackdropStartColor, "#FFF7F0E8");
            AppMainWindowBackdropMidColor = ThemeManager.NormalizeColorOrDefault(AppMainWindowBackdropMidColor, "#FFF1E7DD");
            AppMainWindowBackdropEndColor = ThemeManager.NormalizeColorOrDefault(AppMainWindowBackdropEndColor, "#FFF7F3EE");
            AppHeroSurfaceStartColor = ThemeManager.NormalizeColorOrDefault(AppHeroSurfaceStartColor, "#FFFFFAF5");
            AppHeroSurfaceEndColor = ThemeManager.NormalizeColorOrDefault(AppHeroSurfaceEndColor, "#FFF6EDE3");
            AppInsetSurfaceStartColor = ThemeManager.NormalizeColorOrDefault(AppInsetSurfaceStartColor, "#F7FFFFFF");
            AppInsetSurfaceEndColor = ThemeManager.NormalizeColorOrDefault(AppInsetSurfaceEndColor, "#FFF8F3ED");
            AppBackdropOrbPrimaryInnerColor = ThemeManager.NormalizeColorOrDefault(AppBackdropOrbPrimaryInnerColor, "#66E6BB97");
            AppBackdropOrbPrimaryOuterColor = ThemeManager.NormalizeColorOrDefault(AppBackdropOrbPrimaryOuterColor, "#00E6BB97");
            AppBackdropOrbSecondaryInnerColor = ThemeManager.NormalizeColorOrDefault(AppBackdropOrbSecondaryInnerColor, "#55B4C2D8");
            AppBackdropOrbSecondaryOuterColor = ThemeManager.NormalizeColorOrDefault(AppBackdropOrbSecondaryOuterColor, "#00B4C2D8");
            AppBackdropOrbTertiaryInnerColor = ThemeManager.NormalizeColorOrDefault(AppBackdropOrbTertiaryInnerColor, "#44F3D8BF");
            AppBackdropOrbTertiaryOuterColor = ThemeManager.NormalizeColorOrDefault(AppBackdropOrbTertiaryOuterColor, "#00F3D8BF");
        }
    }
}
