using System.Configuration;
using System.Data;
using System.Windows;
using InTempo.Classes.Utilities.Impostazioni;
using InTempo.Classes.Utilities.Theming;

namespace InTempo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ImpostazioniAdunanze Settings { get; private set; } = new ImpostazioniAdunanze();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Settings = SettingsStore.Load();
            Settings.TemaSelezionato = ThemeManager.ApplyTheme(Settings.TemaSelezionato, Settings.TemaPersonalizzato);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.TemaSelezionato = ThemeManager.ApplyTheme(Settings.TemaSelezionato, Settings.TemaPersonalizzato);
            SettingsStore.Save(Settings);

            base.OnExit(e);
        }
    }

}
