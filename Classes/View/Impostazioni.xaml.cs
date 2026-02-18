using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;

namespace InTempo.Classes.View
{


    public partial class Impostazioni : Window
    {

        public Impostazioni()
        {
            InitializeComponent();

            TimePickerFine.SelectedTime = App.Settings.FineSettimana.OraInizio;
            TimePickerInfra.SelectedTime = App.Settings.Infrasettimanale.OraInizio;
            SelezioneGiorno();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            PrendiGiorno(CmbGiornoInfra.SelectedItem as ComboBoxItem);
            PrendiGiorno(CmbGiornoFine.SelectedItem as ComboBoxItem);
            PrendiOrario();
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

    }

}