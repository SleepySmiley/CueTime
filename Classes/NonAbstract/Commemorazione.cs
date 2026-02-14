using InTempo.Classes.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InTempo.Classes.NonAbstract
{
    public class Commemorazione : RiunioniGenerali
    {
        public override ObservableCollection<Parte> Parti { get; set; } = new ObservableCollection<Parte>();

        public Commemorazione() : base(new TimeSpan(1,0,0), 4)
        {
        }

    }
}
