using InTempo.Classes.NonAbstract;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace InTempo.Classes.View
{
    public partial class ModificaParte : Window
    {
        public Parte ParteCopia { get; set; }

        public Color ColorPickerColor { get; set; } = Colors.Black;

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

            ColorPickerColor = originale.ColoreParte is SolidColorBrush sb ? sb.Color : Colors.Black;

            DataContext = ParteCopia;
        }

        private void BtnFatto_Click(object sender, RoutedEventArgs e)
        {
            ParteCopia.ColoreParte = new SolidColorBrush(ColorPickerColor);
            ParteCopia.TempoScorrevole = ParteCopia.TempoParte;
            DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnChiudiIcona_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtNumero_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (TxtNumero.Text == "")
            {
                ParteCopia.NumeroParte = null;
            }
        }
    }
}
