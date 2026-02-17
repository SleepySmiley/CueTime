using System;
using System.Windows;
using System.Windows.Controls;

namespace InTempo.Classes.View
{
    public class OrarioAdunanza
    {
        public DayOfWeek GiornoSettimana { get; set; }
        public DateTime OraInizio { get; set; }
    }

    public class ImpostazioniAdunanze
    {
        public OrarioAdunanza Infrasettimanale { get; set; } = new OrarioAdunanza();
        public OrarioAdunanza FineSettimana { get; set; } = new OrarioAdunanza();
    }

    public partial class Impostazioni : Window
    {
        public ImpostazioniAdunanze Configurazione { get; set; } = new ImpostazioniAdunanze();

        public Impostazioni()
        {
            InitializeComponent();
        }

        private void BtnSalva_Click(object sender, RoutedEventArgs e)
        {
            if (CmbGiornoInfra.SelectedItem is ComboBoxItem itemInfra)
            {
                string giorno = itemInfra.Content?.ToString() ?? string.Empty;
                Configurazione.Infrasettimanale.GiornoSettimana = ConvertiGiorno(giorno);
            }
            if (TimePickerInfra.SelectedTime.HasValue)
            {
                Configurazione.Infrasettimanale.OraInizio = TimePickerInfra.SelectedTime.Value;
            }

            if (CmbGiornoFine.SelectedItem is ComboBoxItem itemFine)
            {
                string giorno = itemFine.Content?.ToString() ?? string.Empty;
                Configurazione.FineSettimana.GiornoSettimana = ConvertiGiorno(giorno);
            }
            if (TimePickerFine.SelectedTime.HasValue)
            {
                Configurazione.FineSettimana.OraInizio = TimePickerFine.SelectedTime.Value;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private DayOfWeek ConvertiGiorno(string giorno)
        {
            if (string.IsNullOrEmpty(giorno)) return DayOfWeek.Sunday;

            switch (giorno.ToLower())
            {
                case "lunedì": return DayOfWeek.Monday;
                case "martedì": return DayOfWeek.Tuesday;
                case "mercoledì": return DayOfWeek.Wednesday;
                case "giovedì": return DayOfWeek.Thursday;
                case "venerdì": return DayOfWeek.Friday;
                case "sabato": return DayOfWeek.Saturday;
                case "domenica": return DayOfWeek.Sunday;
                default: return DayOfWeek.Sunday;
            }
        }
    }
}