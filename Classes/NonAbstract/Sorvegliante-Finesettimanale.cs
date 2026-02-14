using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InTempo.Classes.NonAbstract
{
    public class Sorvegliante_Finesettimanale : Finesettimanale
    {
        public override ObservableCollection<Parte> Parti { get; set; } = new ObservableCollection<Parte>();

        public Sorvegliante_Finesettimanale() { }
    }
}
