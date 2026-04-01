using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using InTempo.Classes.Utilities.Impostazioni;
using InTempo.Classes.Utilities.Monitors;
using InTempo.Classes.Utilities.Theming;
using MonitorInfo = InTempo.Classes.Utilities.Monitors.Monitor;

namespace InTempo.Classes.View
{
    public partial class Impostazioni : Window, INotifyPropertyChanged
    {
        private readonly ImpostazioniAdunanze _settings;
        private readonly ImpostazioniAdunanze _workingSettings;
        private bool _suppressThemeSelectionChanged;
        private string _lastThemeSelectionKey = ThemeManager.DefaultThemeKey;
        private CustomThemePalette _workingCustomThemePalette = ThemeManager.CreateDefaultCustomTheme();

        public TimerLogics Logiche { get; }

        public IReadOnlyList<AppThemeDefinition> TemiDisponibili { get; private set; } = Array.Empty<AppThemeDefinition>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public Impostazioni(TimerLogics logichePassate, ImpostazioniAdunanze settings)
        {
            InitializeComponent();
            DataContext = this;

            Logiche = logichePassate;
            _settings = settings;
            _workingSettings = settings.Clone();
            _workingCustomThemePalette = _workingSettings.TemaPersonalizzato?.Clone() ?? ThemeManager.CreateDefaultCustomTheme();
            _workingCustomThemePalette.Normalizza();

            TimePickerFine.SelectedTime = _workingSettings.FineSettimana.OraInizio;
            TimePickerInfra.SelectedTime = _workingSettings.Infrasettimanale.OraInizio;
            SelezioneGiorno();
            SelezionaMonitorScelto();
            AggiornaListaSalvataggi();
            CaricaDateSorvegliante();
            AggiornaTemiDisponibili();
            CaricaTemaSelezionato();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (!PrendiGiorni())
            {
                return;
            }

            if (!PrendiOrario())
            {
                return;
            }

            if (!SalvaMonitor())
            {
                return;
            }

            if (!RaccogliDateSorvegliante())
            {
                return;
            }

            if (!SalvaTemaSelezionato())
            {
                return;
            }

            _settings.CopyFrom(_workingSettings);
            SettingsStore.Save(_settings);
            DialogResult = true;
            Close();
        }

        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool PrendiGiorni()
        {
            if (!TryGetDayOfWeekFromCombo(CmbGiornoInfra.SelectedItem as ComboBoxItem, "Infrasettimanale", out DayOfWeek giornoInfra))
            {
                return false;
            }

            if (!TryGetDayOfWeekFromCombo(CmbGiornoFine.SelectedItem as ComboBoxItem, "Fine Settimana", out DayOfWeek giornoFine))
            {
                return false;
            }

            _workingSettings.Infrasettimanale.GiornoSettimana = giornoInfra;
            _workingSettings.FineSettimana.GiornoSettimana = giornoFine;
            return true;
        }

        private bool TryGetDayOfWeekFromCombo(ComboBoxItem? cmbGiorno, string tipoAdunanza, out DayOfWeek giornoSettimana)
        {
            giornoSettimana = DayOfWeek.Sunday;

            if (cmbGiorno == null)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Dati mancanti",
                    $"Seleziona il giorno per l'adunanza {tipoAdunanza}.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return false;
            }

            if (!int.TryParse(cmbGiorno.Tag?.ToString(), out int tag) || tag < 0 || tag > 6)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Dati non validi",
                    $"Il giorno selezionato per l'adunanza {tipoAdunanza} non è valido.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return false;
            }

            giornoSettimana = (DayOfWeek)tag;
            return true;
        }

        public bool PrendiOrario()
        {
            if (TimePickerInfra.SelectedTime == null)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Dati mancanti",
                    "Seleziona l'orario per l'Infrasettimanale.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return false;
            }

            if (TimePickerFine.SelectedTime == null)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Dati mancanti",
                    "Seleziona l'orario per il Fine Settimana.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return false;
            }

            _workingSettings.Infrasettimanale.OraInizio = (DateTime)TimePickerInfra.SelectedTime;
            _workingSettings.FineSettimana.OraInizio = (DateTime)TimePickerFine.SelectedTime;

            return true;
        }

        public void SelezioneGiorno()
        {
            foreach (ComboBoxItem item in CmbGiornoFine.Items)
            {
                if (int.TryParse(item.Tag?.ToString(), out int tag) && tag == (int)_workingSettings.FineSettimana.GiornoSettimana)
                {
                    CmbGiornoFine.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in CmbGiornoInfra.Items)
            {
                if (int.TryParse(item.Tag?.ToString(), out int tag) && tag == (int)_workingSettings.Infrasettimanale.GiornoSettimana)
                {
                    CmbGiornoInfra.SelectedItem = item;
                    break;
                }
            }
        }

        public void SelezionaMonitorScelto()
        {
            string nomeMonitorSalvato = _workingSettings.MonitorScelto?.Nome ?? string.Empty;

            foreach (MonitorInfo monitor in GestoreMonitor.Monitors)
            {
                if (monitor.Nome == nomeMonitorSalvato)
                {
                    cmbSchermi.SelectedItem = monitor;
                    break;
                }
            }

            if (cmbSchermi.SelectedItem == null)
            {
                cmbSchermi.SelectedItem = GestoreMonitor.Monitors.FirstOrDefault(m => m.EPrimario)
                    ?? GestoreMonitor.Monitors.FirstOrDefault();
            }
        }

        public bool SalvaMonitor()
        {
            if (cmbSchermi.SelectedItem is MonitorInfo monitorSelezionato)
            {
                _workingSettings.MonitorScelto = monitorSelezionato.Clone();
                return true;
            }

            FinestraPopUP errore = new FinestraPopUP(
                "Errore",
                "Seleziona un monitor valido prima di salvare.",
                ConfigurazionePulsantiPopup.Ok);
            errore.ShowDialog();
            return false;
        }

        private void btnIdentifica_Click(object sender, RoutedEventArgs e)
        {
            foreach (MonitorInfo monitor in GestoreMonitor.Monitors)
            {
                string numeroVero = Regex.Match(monitor.Nome, @"\d+").Value;

                if (string.IsNullOrEmpty(numeroVero))
                {
                    numeroVero = "?";
                }

                FinestraIdentifica fin = new FinestraIdentifica(numeroVero)
                {
                    Left = monitor.AreaTotale.Left,
                    Top = monitor.AreaTotale.Top,
                    Width = monitor.AreaTotale.Width,
                    Height = monitor.AreaTotale.Height
                };

                fin.Show();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnSalvaAdunanza_Click(object sender, RoutedEventArgs e)
        {
            string nomefile = TxtNomeSalvataggio.Text.Trim();

            if (string.IsNullOrEmpty(nomefile))
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Attenzione",
                    "Inserisci un nome per il salvataggio.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return;
            }

            bool successo = GestoreSalvataggi.SalvaAdunanza(Logiche.AdunanzaCorrente, nomefile);

            if (successo)
            {
                FinestraPopUP successoPopup = new FinestraPopUP(
                    "Completato",
                    $"Adunanza '{nomefile}' salvata con successo!",
                    ConfigurazionePulsantiPopup.Ok);
                successoPopup.ShowDialog();
                TxtNomeSalvataggio.Clear();
                AggiornaListaSalvataggi();
            }
            else
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Errore",
                    "Si è verificato un problema durante il salvataggio.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
            }
        }

        private void AggiornaListaSalvataggi()
        {
            ListAdunanzeSalvate.ItemsSource = GestoreSalvataggi.OttieniListaSalvataggi();
        }

        private void BtnCaricaAdunanza_Click(object sender, RoutedEventArgs e)
        {
            if (ListAdunanzeSalvate.SelectedItem == null)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Attenzione",
                    "Seleziona un'adunanza dalla lista prima di caricare.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return;
            }

            CaricaAdunanzaSelezionata();
        }

        private void ListAdunanzeSalvate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListAdunanzeSalvate.SelectedItem != null)
            {
                CaricaAdunanzaSelezionata();
            }
        }

        private void CaricaAdunanzaSelezionata()
        {
            string nomeFile = ListAdunanzeSalvate.SelectedItem as string ?? string.Empty;
            Adunanza? adunanzaCaricata = GestoreSalvataggi.CaricaAdunanza(nomeFile);

            if (adunanzaCaricata != null)
            {
                bool wasRunning = Logiche.IsRunning;
                Logiche.StopTimer();

                Logiche.AdunanzaCorrente.Parti.Clear();

                foreach (Parte parte in adunanzaCaricata.Parti)
                {
                    Logiche.AdunanzaCorrente.Parti.Add(parte);
                }

                Logiche.AdunanzaCorrente.TempoTotaleRiferimento = adunanzaCaricata.TempoTotaleRiferimento;
                Logiche.AdunanzaCorrente.TempoConsumatoPartiRimosse = adunanzaCaricata.TempoConsumatoPartiRimosse;
                Logiche.AdunanzaCorrente.NormalizzaTracciamentoResiduo();

                Parte? current = Logiche.AdunanzaCorrente.Parti.FirstOrDefault(p => p.IsCurrent)
                                 ?? Logiche.AdunanzaCorrente.Parti.FirstOrDefault();

                Logiche.AdunanzaCorrente.Current = current;
                Logiche.AggiornaGrafica();

                if (wasRunning)
                {
                    Logiche.StartTimer();
                }

                FinestraPopUP successo = new FinestraPopUP(
                    "Completato",
                    $"Adunanza '{nomeFile}' caricata con successo!",
                    ConfigurazionePulsantiPopup.Ok);
                successo.ShowDialog();

                DialogResult = true;
                Close();
                return;
            }

            FinestraPopUP errorePopup = new FinestraPopUP(
                "Errore",
                "Impossibile caricare il file selezionato.",
                ConfigurazionePulsantiPopup.Ok);
            errorePopup.ShowDialog();
        }

        private void BtnEliminaAdunanza_Click(object sender, RoutedEventArgs e)
        {
            if (ListAdunanzeSalvate.SelectedItem == null)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Attenzione",
                    "Seleziona un'adunanza dalla lista prima di eliminare.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return;
            }

            string nomeFile = ListAdunanzeSalvate.SelectedItem as string ?? string.Empty;

            bool eliminato = GestoreSalvataggi.EliminaAdunanza(nomeFile);

            if (eliminato)
            {
                FinestraPopUP successo = new FinestraPopUP(
                    "Eliminata",
                    $"L'adunanza '{nomeFile}' è stata eliminata.",
                    ConfigurazionePulsantiPopup.Ok);
                successo.ShowDialog();
                AggiornaListaSalvataggi();
                return;
            }

            FinestraPopUP errorePopup = new FinestraPopUP(
                "Errore",
                "Impossibile eliminare il file selezionato.",
                ConfigurazionePulsantiPopup.Ok);
            errorePopup.ShowDialog();
        }

        private bool RaccogliDateSorvegliante()
        {
            DateTime? data1 = DatePrimaVisita.SelectedDate;
            DateTime? data2 = DateSecondaVisita.SelectedDate;

            if (data1 == null || data2 == null)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Dati mancanti",
                    "Seleziona entrambe le date per le visite del sorvegliante.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return false;
            }

            if (data1 > data2)
            {
                FinestraPopUP errore = new FinestraPopUP(
                    "Dati non validi",
                    "La prima data non può essere successiva alla seconda data.",
                    ConfigurazionePulsantiPopup.Ok);
                errore.ShowDialog();
                return false;
            }

            DateTime lunedi1 = ToMonday(data1.Value);
            DateTime lunedi2 = ToMonday(data2.Value);

            _workingSettings.DateVisitaSorvegliante = new[] { lunedi1, lunedi2 };
            DatePrimaVisita.SelectedDate = lunedi1;
            DateSecondaVisita.SelectedDate = lunedi2;
            return true;
        }

        private static DateTime ToMonday(DateTime date)
        {
            int diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.Date.AddDays(-diff);
        }

        private void CaricaDateSorvegliante()
        {
            DateTime[] dateVisita = _workingSettings.DateVisitaSorvegliante ?? Array.Empty<DateTime>();

            DatePrimaVisita.SelectedDate =
                dateVisita.Length > 0 && ImpostazioniAdunanze.IsDataVisitaValida(dateVisita[0])
                    ? dateVisita[0]
                    : null;

            DateSecondaVisita.SelectedDate =
                dateVisita.Length > 1 && ImpostazioniAdunanze.IsDataVisitaValida(dateVisita[1])
                    ? dateVisita[1]
                    : null;
        }

        private void AggiornaTemiDisponibili()
        {
            TemiDisponibili = ThemeManager.GetAvailableThemes(_workingCustomThemePalette);
            OnPropertyChanged(nameof(TemiDisponibili));
        }

        private void CaricaTemaSelezionato()
        {
            string temaSelezionato = ThemeManager.GetThemeOrDefault(_workingSettings.TemaSelezionato, _workingCustomThemePalette).Key;
            ImpostaTemaSelezionatoSenzaEventi(temaSelezionato);
            _lastThemeSelectionKey = temaSelezionato;
        }

        private void ImpostaTemaSelezionatoSenzaEventi(string themeKey)
        {
            _suppressThemeSelectionChanged = true;
            LstTemi.SelectedValue = themeKey;
            _suppressThemeSelectionChanged = false;
        }

        private bool SalvaTemaSelezionato()
        {
            _workingCustomThemePalette.Normalizza();
            _workingSettings.TemaPersonalizzato = _workingCustomThemePalette.Clone();
            string temaSelezionato = LstTemi.SelectedValue as string ?? ThemeManager.DefaultThemeKey;
            string temaRichiesto = ThemeManager.GetThemeOrDefault(temaSelezionato, _workingSettings.TemaPersonalizzato).Key;
            _workingSettings.TemaSelezionato = ThemeManager.ApplyTheme(temaRichiesto, _workingSettings.TemaPersonalizzato);
            return true;
        }

        private void LstTemi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressThemeSelectionChanged)
            {
                return;
            }

            string temaSelezionato = LstTemi.SelectedValue as string ?? ThemeManager.DefaultThemeKey;
            if (!string.Equals(temaSelezionato, ThemeManager.CustomThemeKey, StringComparison.OrdinalIgnoreCase))
            {
                _lastThemeSelectionKey = temaSelezionato;
            }
        }

        private void AggiornaEditorTemaPersonalizzato()
        {
            string temaSelezionato = LstTemi.SelectedValue as string ?? ThemeManager.DefaultThemeKey;
            bool isCustomThemeSelected = string.Equals(temaSelezionato, ThemeManager.CustomThemeKey, StringComparison.OrdinalIgnoreCase);

            if (!isCustomThemeSelected)
            {
                _lastThemeSelectionKey = temaSelezionato;
                return;
            }

            ThemeCustomizerWindow finestra = new ThemeCustomizerWindow(_workingCustomThemePalette.Clone())
            {
                Owner = this
            };

            bool? risultato = finestra.ShowDialog();
            if (risultato == true)
            {
                _workingCustomThemePalette = finestra.ResultPalette.Clone();
                _workingCustomThemePalette.Normalizza();
                _workingSettings.TemaPersonalizzato = _workingCustomThemePalette.Clone();
                AggiornaTemiDisponibili();
                ImpostaTemaSelezionatoSenzaEventi(ThemeManager.CustomThemeKey);
                _lastThemeSelectionKey = ThemeManager.CustomThemeKey;
                return;
            }

            string temaRipristino = ThemeManager.GetThemeOrDefault(_lastThemeSelectionKey, _workingCustomThemePalette).Key;
            ImpostaTemaSelezionatoSenzaEventi(temaRipristino);
        }

        private void LstTemi_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool shouldOpenCustomEditor =
                TryGetThemeListItemFromOriginalSource(e.OriginalSource, out ListBoxItem? clickedItem)
                && clickedItem.IsSelected
                && clickedItem.DataContext is AppThemeDefinition theme
                && string.Equals(theme.Key, ThemeManager.CustomThemeKey, StringComparison.OrdinalIgnoreCase);

            if (shouldOpenCustomEditor)
            {
                AggiornaEditorTemaPersonalizzato();
            }
        }

        private void LstTemi_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Space)
                && string.Equals(LstTemi.SelectedValue as string, ThemeManager.CustomThemeKey, StringComparison.OrdinalIgnoreCase))
            {
                AggiornaEditorTemaPersonalizzato();
                e.Handled = true;
            }
        }

        private static bool TryGetThemeListItemFromOriginalSource(object originalSource, [NotNullWhen(true)] out ListBoxItem? listBoxItem)
        {
            DependencyObject? source = originalSource as DependencyObject;
            while (source != null && source is not ListBoxItem)
            {
                source = VisualTreeHelper.GetParent(source);
            }

            listBoxItem = source as ListBoxItem;
            return listBoxItem != null;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
