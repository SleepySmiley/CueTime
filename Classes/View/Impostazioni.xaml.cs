using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;
using InTempo.Classes.Utilities.Monitors;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InTempo.Classes.View
{
    public partial class Impostazioni : Window
    {
        public TimerLogics Logiche { get; set; }

        public Impostazioni(TimerLogics Logichepassate)
        {
            InitializeComponent();
            this.DataContext = this;
            TimePickerFine.SelectedTime = App.Settings.FineSettimana.OraInizio;
            TimePickerInfra.SelectedTime = App.Settings.Infrasettimanale.OraInizio;
            SelezioneGiorno();
            SelezionaMonitorScelto();
            Logiche = Logichepassate;
            AggiornaListaSalvataggi();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (!PrendiGiorno(CmbGiornoInfra.SelectedItem as ComboBoxItem, "Infrasettimanale"))
                return;

            if (!PrendiGiorno(CmbGiornoFine.SelectedItem as ComboBoxItem, "Fine Settimana"))
                return;

            if (!PrendiOrario())
                return;

            if (!SalvaMonitor())
                return;

            if(!RaccogliDateSorvegliante())
                return;

            this.DialogResult = true;
            this.Close();
        }

        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public bool PrendiGiorno(ComboBoxItem? cmbGiorno, string tipoAdunanza)
        {
            if (cmbGiorno == null)
            {
                FinestraPopUP Errore = new FinestraPopUP("Dati mancanti", $"Seleziona il giorno per l'adunanza {tipoAdunanza}.", 1);
                Errore.ShowDialog();
                return false;
            }

            switch (cmbGiorno.Tag)
            {
                case "1":
                    App.Settings.Infrasettimanale.GiornoSettimana = DayOfWeek.Monday;
                    break;
                case "2":
                    App.Settings.Infrasettimanale.GiornoSettimana = DayOfWeek.Tuesday;
                    break;
                case "3":
                    App.Settings.Infrasettimanale.GiornoSettimana = DayOfWeek.Wednesday;
                    break;
                case "4":
                    App.Settings.Infrasettimanale.GiornoSettimana = DayOfWeek.Thursday;
                    break;
                case "5":
                    App.Settings.Infrasettimanale.GiornoSettimana = DayOfWeek.Friday;
                    break;
                case "6":
                    App.Settings.FineSettimana.GiornoSettimana = DayOfWeek.Saturday;
                    break;
                case "0":
                    App.Settings.FineSettimana.GiornoSettimana = DayOfWeek.Sunday;
                    break;
            }

            return true; 
        }

        public bool PrendiOrario()
        {
            if (TimePickerInfra.SelectedTime == null)
            {
                FinestraPopUP Errore = new FinestraPopUP("Dati mancanti", "Seleziona l'orario per l'Infrasettimanale.", 1);
                Errore.ShowDialog();
                return false;
            }

            if (TimePickerFine.SelectedTime == null)
            {
                FinestraPopUP Errore = new FinestraPopUP("Dati mancanti", "Seleziona l'orario per il Fine Settimana.", 1);
                Errore.ShowDialog();
                return false;
            }

            App.Settings.Infrasettimanale.OraInizio = (DateTime)TimePickerInfra.SelectedTime;
            App.Settings.FineSettimana.OraInizio = (DateTime)TimePickerFine.SelectedTime;

            return true; 
        }

        public void SelezioneGiorno()
        {
            foreach (ComboBoxItem item in CmbGiornoFine.Items)
            {
                if (!int.TryParse(item.Tag?.ToString(), out int tag))
                    continue;

                if (tag == (int)App.Settings.FineSettimana.GiornoSettimana)
                {
                    CmbGiornoFine.SelectedItem = item;
                    break;
                }
            }

            foreach (ComboBoxItem item in CmbGiornoInfra.Items)
            {
                if (!int.TryParse(item.Tag?.ToString(), out int tag))
                    continue;

                if (tag == (int)App.Settings.Infrasettimanale.GiornoSettimana)
                {
                    CmbGiornoInfra.SelectedItem = item;
                    break;
                }
            }
        }

        public void SelezionaMonitorScelto()
        {
            string NomeMonitorSalvato = App.Settings.MonitorScelto.Nome;

            foreach (var monitor in GestoreMonitor.Monitors)
            {
                if (monitor.Nome == NomeMonitorSalvato)
                {
                    cmbSchermi.SelectedItem = monitor;
                    break;
                }
            }
        }

        public bool SalvaMonitor()
        {
            if (cmbSchermi.SelectedItem is Utilities.Monitors.Monitor monitorSelezionato)
            {
                App.Settings.MonitorScelto = monitorSelezionato;
                return true; 
            }
            else
            {
                FinestraPopUP Errore = new FinestraPopUP("Errore", "Seleziona un monitor valido prima di salvare.", 1);
                Errore.ShowDialog();
                return false; 
            }
        }

        private void btnIdentifica_Click(object sender, RoutedEventArgs e)
        {
            foreach (var monitor in GestoreMonitor.Monitors)
            {
                string numeroVero = Regex.Match(monitor.Nome, @"\d+").Value;

                if (string.IsNullOrEmpty(numeroVero))
                {
                    numeroVero = "?";
                }

                FinestraIdentifica fin = new FinestraIdentifica(numeroVero);

                fin.Left = monitor.AreaTotale.Left;
                fin.Top = monitor.AreaTotale.Top;
                fin.Width = monitor.AreaTotale.Width;
                fin.Height = monitor.AreaTotale.Height;

                fin.Show();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BtnSalvaAdunanza_Click(object sender, RoutedEventArgs e)
        {
            string nomefile = TxtNomeSalvataggio.Text.Trim();

            if (string.IsNullOrEmpty(nomefile))
            {
                FinestraPopUP Errore = new FinestraPopUP("Attenzione", "Inserisci un nome per il salvataggio.", 1);
                Errore.ShowDialog();
                return;
            }

            bool successo = GestoreSalvataggi.SalvaAdunanza(Logiche.AdunanzaCorrente, nomefile);

            if (successo)
            {
                FinestraPopUP Successo = new FinestraPopUP("Completato", $"Adunanza '{nomefile}' salvata con successo!", 1);
                Successo.ShowDialog();
                TxtNomeSalvataggio.Clear();
                AggiornaListaSalvataggi();
            }
            else
            {
                FinestraPopUP Errore = new FinestraPopUP("Errore", "Si è verificato un problema durante il salvataggio.", 1);
                Errore.ShowDialog();
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
                FinestraPopUP Errore = new FinestraPopUP("Attenzione", "Seleziona un'adunanza dalla lista prima di caricare.", 1);
                Errore.ShowDialog();
                return;
            }

            string nomeFile = ListAdunanzeSalvate.SelectedItem as string ?? string.Empty;
            Adunanza? adunanzaCaricata = Utilities.GestoreSalvataggi.CaricaAdunanza(nomeFile);

            if (adunanzaCaricata != null)
            {
                Logiche.AdunanzaCorrente.Parti.Clear();

                foreach (var parte in adunanzaCaricata.Parti)
                {
                    Logiche.AdunanzaCorrente.Parti.Add(parte);
                }

                FinestraPopUP Successo = new FinestraPopUP("Completato", $"Adunanza '{nomeFile}' caricata con successo!", 1);
                Successo.ShowDialog();

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                FinestraPopUP Errore = new FinestraPopUP("Errore", "Impossibile caricare il file selezionato.", 1);
                Errore.ShowDialog();
            }
        }

        private void BtnEliminaAdunanza_Click(object sender, RoutedEventArgs e)
        {
            if (ListAdunanzeSalvate.SelectedItem == null)
            {
                FinestraPopUP Errore = new FinestraPopUP("Attenzione", "Seleziona un'adunanza dalla lista prima di eliminare.", 1);
                Errore.ShowDialog();
                return;
            }

            string nomeFile = ListAdunanzeSalvate.SelectedItem as string ?? string.Empty;

            bool eliminato = GestoreSalvataggi.EliminaAdunanza(nomeFile);

            if (eliminato)
            {
                FinestraPopUP Successo = new FinestraPopUP("Eliminata", $"L'adunanza '{nomeFile}' è stata eliminata.", 1);
                Successo.ShowDialog();

                AggiornaListaSalvataggi();
            }
            else
            {
                FinestraPopUP Errore = new FinestraPopUP("Errore", "Impossibile eliminare il file selezionato.", 1);
                Errore.ShowDialog();
            }
        }

        public bool RaccogliDateSorvegliante()
        {
            DateTime? data1 = DatePrimaVisita.SelectedDate;
            DateTime? data2 = DateSecondaVisita.SelectedDate;

            if(data1 == null || data2 == null)
            {
                FinestraPopUP Errore = new FinestraPopUP("Dati mancanti", "Seleziona entrambe le date per le visite del sorvegliante.", 1);
                Errore.ShowDialog();
                return false;
            }
            else if(data1 > data2)
            {
                FinestraPopUP Errore = new FinestraPopUP("Dati non validi", "La prima data non può essere successiva alla seconda data.", 1);
                Errore.ShowDialog();
                return false;
            }
            else
            {
                App.Settings.DateVisitaSorvegliante[0] = data1.Value;
                App.Settings.DateVisitaSorvegliante[1] = data2.Value;
                return true;
            }


        }
    }
}