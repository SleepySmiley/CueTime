using System;
using System.Collections.Generic;
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

        public bool Ritorno { get; set; } = true;





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

        }

        public FinestraPopUP(string TitoloPassato, string TestoPassato, string TestoBtn1, string TestoBtn2)
        {
            InitializeComponent();
            Titolo = TitoloPassato;
            Testo = TestoPassato;
            BtnPredefinito1.Content = TestoBtn1;
            BtnPredefinito2.Content = TestoBtn2;
        }

        public FinestraPopUP(string TitoloPassato, int bottoni)
        {
            InitializeComponent();
            Titolo = TitoloPassato;
            Testo = "";
            switch(bottoni)
            {
                case 1:
                    BtnPredefinito2.Visibility = Visibility.Visible;
                    BtnPredefinito1 .Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    BtnPredefinito1.Visibility = Visibility.Visible;
                    BtnPredefinito2.Visibility = Visibility.Visible;
                    break;
                default:
                    FinestraPopUP Errore = new FinestraPopUP("Errore", "Il numero di bottoni è errato, deve essere 0, 1 o 2", 1);
                    Errore.ShowDialog();
                    break;
            }
        }

        private void BtnPredefinito2_Click(object sender, RoutedEventArgs e)
        {
            Ritorno = true;
        }

        private void BtnPredefinito1_Click(object sender, RoutedEventArgs e)
        {
            Ritorno = false;
        }
    }
}
