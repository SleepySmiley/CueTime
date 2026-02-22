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
        public string Testo { get; set; }

        public int TipologiaFinestra { get; set; }

        private FinestraTimer _finestratimer;




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
            Testo = TestoPassato;
            DataContext = this;
            txbTesto.Visibility = Visibility.Visible;
            txtTesto.IsReadOnly = true;
            TipologiaFinestra = 0;

        }

        public FinestraPopUP(string TitoloPassato, string TestoPassato, string TestoBtn1, string TestoBtn2)
        {
            InitializeComponent();
            Titolo = TitoloPassato;
            Testo = TestoPassato;
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
            Testo = "";
            DataContext = this;
            BtnPredefinito1.Content = TestoBtn1;
            BtnPredefinito2.Content = TestoBtn2;
            TipologiaFinestra = 2;
            _finestratimer = finestraDaPassare;

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
                string input = txtTesto.Text;

                _finestratimer.CambiaVista(2, input);

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
                string input = txtTesto.Text;

                _finestratimer.CambiaVista(3, input);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(TipologiaFinestra == 2)
            {
                if (TimerLogics.IsRunning)
                    _finestratimer.CambiaVista(1, "");
                else
                    _finestratimer.CambiaVista(4, "");
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
            this.DialogResult = false;
            this.Close();
        }
    }
}
