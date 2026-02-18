using System.Configuration;
using System.Data;
using System.Windows;
using InTempo.Classes.Utilities.Impostazioni;

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

            // Carico le impostazioni dal file
            Settings = SettingsStore.Load();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Salvo le impostazioni su file
            SettingsStore.Save(Settings);

            base.OnExit(e);
        }
    }

}
