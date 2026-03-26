using InTempo.Classes.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InTempo.Classes.View
{
    /// <summary>
    /// Logica di interazione per FinestraPopUP.xaml
    /// </summary>
    public partial class FinestraPopUP : Window
    {
        public string Titolo { get; set; }
        public string TestoMessaggio { get; set; } = string.Empty;
        public string TestoInput { get; set; } = string.Empty;

        public int TipologiaFinestra { get; set; }

        public int? NumeroInserito { get; private set; }

        private FinestraTimer? _finestratimer;
        private bool _richiedeNumeroInteroPositivo;

        public FinestraPopUP(string TitoloPassato, string TestoPassato, int Bottoni)
        {
            InitializeComponent();

            switch (Bottoni)
            {
                case 0:
                    BtnPredefinito1.Visibility = Visibility.Collapsed;
                    BtnPredefinito2.Visibility = Visibility.Collapsed;

                    break;
                case 1:
                    BtnPredefinito2.Visibility = Visibility.Visible;
                    BtnPredefinito1.Visibility = Visibility.Collapsed;

                    break;
                case 2:
                    BtnPredefinito1.Visibility = Visibility.Visible;
                    BtnPredefinito2.Visibility = Visibility.Visible;
                    break;
            }
            Titolo = TitoloPassato;
            TestoMessaggio = TestoPassato;
            DataContext = this;
            txbTesto.Visibility = Visibility.Visible;
            txtTesto.IsReadOnly = true;
            TipologiaFinestra = 0;

        }

        public FinestraPopUP(string TitoloPassato, string TestoPassato, string TestoBtn1, string TestoBtn2)
        {
            InitializeComponent();
            Titolo = TitoloPassato;
            TestoMessaggio = TestoPassato;
            txbTesto.Visibility = Visibility.Visible;
            BtnPredefinito1.Content = TestoBtn1;
            BtnPredefinito2.Content = TestoBtn2;
            DataContext = this;
            txtTesto.IsReadOnly = true;
            TipologiaFinestra = 1;
        }

        public FinestraPopUP(string TitoloPassato, string TestoBtn1, string TestoBtn2, FinestraTimer finestraDaPassare)
        {
            InitializeComponent();
            Titolo = TitoloPassato;
            txtTesto.Visibility = Visibility.Visible;
            TestoInput = "";
            DataContext = this;
            BtnPredefinito1.Content = TestoBtn1;
            BtnPredefinito2.Content = TestoBtn2;
            TipologiaFinestra = 2;
            _finestratimer = finestraDaPassare;
        }

        public FinestraPopUP(string TitoloPassato, string TestoPassato, string TestoBtn1, string TestoBtn2, bool richiedeNumeroInteroPositivo)
        {
            InitializeComponent();
            Titolo = TitoloPassato;
            TestoMessaggio = TestoPassato;
            TestoInput = string.Empty;
            DataContext = this;
            BtnPredefinito1.Content = TestoBtn1;
            BtnPredefinito2.Content = TestoBtn2;
            txbTesto.Visibility = Visibility.Visible;
            txtTesto.Visibility = Visibility.Visible;
            txtTesto.IsReadOnly = false;
            TipologiaFinestra = 3;
            _richiedeNumeroInteroPositivo = richiedeNumeroInteroPositivo;
            Loaded += (_, _) => txtTesto.Focus();
        }

        private void BtnPredefinito2_Click(object sender, RoutedEventArgs e)
        {
            if(TipologiaFinestra == 0)
            {
                DialogResult = true;
                Close();
            }
            else if(TipologiaFinestra == 1)
            {
                DialogResult = true;
                Close();
            }
            else if(TipologiaFinestra == 2)
            {
                ApplicaMessaggio(2);
            }
            else if (TipologiaFinestra == 3)
            {
                if (!TryConfermaInput())
                {
                    return;
                }

                DialogResult = true;
                Close();

            }

        }

        private void BtnPredefinito1_Click(object sender, RoutedEventArgs e)
        {
            if (TipologiaFinestra == 0)
            {
                DialogResult = false;
                Close();
            }
            else if (TipologiaFinestra == 1)
            {
                DialogResult = false;
                Close();
            }
            else if (TipologiaFinestra == 2)
            {
                ApplicaMessaggio(3);
            }
            else if (TipologiaFinestra == 3)
            {
                DialogResult = false;
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(TipologiaFinestra == 2)
            {
                _finestratimer?.CambiaVista(1, "", Brushes.White);
            }
                 
        }

        private bool TryConfermaInput()
        {
            string input = txtTesto.Text.Trim();

            if (_richiedeNumeroInteroPositivo)
            {
                if (!int.TryParse(input, out int numero) || numero <= 0)
                {
                    MostraErroreInput("Inserisci un numero intero valido maggiore di zero.");
                    return false;
                }

                NumeroInserito = numero;
            }
            else
            {
                TestoInput = input;
            }

            NascondiErroreInput();
            return true;
        }

        private void MostraErroreInput(string messaggio)
        {
            txtErroreInput.Text = messaggio;
            txtErroreInput.Visibility = Visibility.Visible;
        }

        private void NascondiErroreInput()
        {
            txtErroreInput.Text = string.Empty;
            txtErroreInput.Visibility = Visibility.Collapsed;
        }

        private void ApplicaMessaggio(int tipoVista)
        {
            string input = txtTesto.Text;
            _finestratimer?.CambiaVista(tipoVista, input, Brushes.White);
            txtTesto.Focus();
            txtTesto.SelectAll();
        }

        private void txtTesto_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtErroreInput.Visibility == Visibility.Visible)
            {
                NascondiErroreInput();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void BtnChiudiIcona_Click(object sender, RoutedEventArgs e)
        {
            if (TipologiaFinestra == 2)
            {
                Close();
                return;
            }

            this.DialogResult = false;
            this.Close();
        }
    }
}
