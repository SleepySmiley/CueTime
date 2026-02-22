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
        public Impostazioni()
        {
            InitializeComponent();
            this.DataContext = this;
            TimePickerFine.SelectedTime = App.Settings.FineSettimana.OraInizio;
            TimePickerInfra.SelectedTime = App.Settings.Infrasettimanale.OraInizio;
            SelezioneGiorno();
            SelezionaMonitorScelto();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            PrendiGiorno(CmbGiornoInfra.SelectedItem as ComboBoxItem);
            PrendiGiorno(CmbGiornoFine.SelectedItem as ComboBoxItem);
            PrendiOrario();
            SalvaMonitor();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public void PrendiGiorno(ComboBoxItem cmbGiorno)
        {
            if (cmbGiorno == null)
            {
                throw new ArgumentNullException("Giorno non selezionato");
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



        }

        public void PrendiOrario()
        {
            if(TimePickerInfra.SelectedTime == null)
            {
                throw new ArgumentNullException("Orario Infrasettimanale non selezionato");
            }
            App.Settings.Infrasettimanale.OraInizio = (DateTime)TimePickerInfra.SelectedTime;
            if(TimePickerFine.SelectedTime == null)
            {
                throw new ArgumentNullException("Orario Finesettimanale non selezionato");
            }
            App.Settings.FineSettimana.OraInizio = (DateTime)TimePickerFine.SelectedTime;
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

            foreach(var monitor in GestoreMonitor.Monitors)
            {
                if(monitor.Nome == NomeMonitorSalvato)
                {
                    cmbSchermi.SelectedItem = monitor;
                    break;
                }
            }


        }

        public void SalvaMonitor()
        {
            if(cmbSchermi.SelectedItem is Utilities.Monitors.Monitor monitorSelezionato)
                {
                    App.Settings.MonitorScelto = monitorSelezionato;
                }
                else
                {
                    throw new ArgumentException("Monitor selezionato non valido");
            }
        }

        private void btnIdentifica_Click(object sender, RoutedEventArgs e)
        {
            foreach (var monitor in GestoreMonitor.Monitors)
            {
                // Estraiamo solo i numeri dal nome del monitor (es. da "\\.\DISPLAY2" tiriamo fuori "2")
                string numeroVero = Regex.Match(monitor.Nome, @"\d+").Value;

                // Se per qualche motivo strano non trova numeri, mettiamo un "?" di sicurezza
                if (string.IsNullOrEmpty(numeroVero))
                {
                    numeroVero = "?";
                }

                // Creiamo la finestra passandole il numero reale
                FinestraIdentifica fin = new FinestraIdentifica(numeroVero);

                // La posizioniamo
                fin.Left = monitor.AreaTotale.Left;
                fin.Top = monitor.AreaTotale.Top;
                fin.Width = monitor.AreaTotale.Width;
                fin.Height = monitor.AreaTotale.Height;

                // Mostriamo la finestra
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
    }

}