using System.Configuration;
using System.Data;
using System.Windows;
using CueTime.Classes.Utilities.Impostazioni;
using CueTime.Classes.Utilities.Theming;

namespace CueTime
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

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            if (Current.MainWindow is MainWindow finestraPrincipale)
            {
                finestraPrincipale.AssicuraSalvataggioStatistichePrimaDellaChiusura();
            }

            base.OnSessionEnding(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Current.MainWindow is MainWindow finestraPrincipale)
            {
                finestraPrincipale.AssicuraSalvataggioStatistichePrimaDellaChiusura();
            }

            Settings.TemaSelezionato = ThemeManager.ApplyTheme(Settings.TemaSelezionato, Settings.TemaPersonalizzato);
            SettingsStore.Save(Settings);

            base.OnExit(e);
        }
    }

}

