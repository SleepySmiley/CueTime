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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CueTime.Classes.View.UserControls
{
    /// <summary>
    /// Interaction logic for VistaSoloScritta.xaml
    /// </summary>
    public partial class VistaSoloScritta : UserControl
    {
        public string Text { get; set; }
        
        public Brush Colore { get; set; }

        public VistaSoloScritta(string testo, Brush colorescritta)
        {
            InitializeComponent();
            Text = testo;
            DataContext = this;
            Colore = colorescritta;

        }
    }
}

