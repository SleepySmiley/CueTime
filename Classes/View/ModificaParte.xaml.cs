using InTempo.Classes.NonAbstract;
using System;
using System.Windows;
using System.Windows.Media;

namespace InTempo.Classes.View
{
    public partial class ModificaParte : Window
    {
        public Parte ParteCopia { get; set; }

        public ModificaParte()
        {
            InitializeComponent();

            ParteCopia = new Parte(
                "",
                TimeSpan.FromMinutes(5),
                "Parte Utente",
                Brushes.Black,
                TimeSpan.FromMinutes(5),
                null
            );

            DataContext = ParteCopia;
        }

        public ModificaParte(Parte originale)
        {
            InitializeComponent();

            ParteCopia = new Parte(
                originale.NomeParte,
                originale.TempoParte,
                originale.TipoParte,
                originale.ColoreParte,
                originale.TempoScorrevole,
                originale.NumeroParte
            );

            DataContext = ParteCopia;
        }

        private void BtnFatto_Click(object sender, RoutedEventArgs e)
        {
           ParteCopia.TempoScorrevole = ParteCopia.TempoParte;
            DialogResult = true;
        }
    }
}