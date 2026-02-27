using InTempo.Classes.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InTempo.Classes.NonAbstract
{
    internal class Sorvegliante_Infrasettimanale : Infrasettimanale
    {
        public override ObservableCollection<Parte> Parti { get; set; } = new ObservableCollection<Parte>();

        public Sorvegliante_Infrasettimanale() 
        {
            Parti = base.Parti;
        }


        public void ModificaSchemaParti(ObservableCollection<Parte> nuoveParti)
        {
            
        }
        
    }
}
