using CueTime.Classes.Utilities;
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
    /// Interaction logic for VistaMista.xaml
    /// </summary>
    public partial class VistaMista : UserControl
    {
        public TimerLogics TimerLogics { get; set; }

        public string Text { get; set; }

        public VistaMista(string Testo, TimerLogics Timer)
        {
            InitializeComponent();
            Text = Testo;
            TimerLogics = Timer;
            this.DataContext = this;

            
        }
    }
}

