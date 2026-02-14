using InTempo.Classes.NonAbstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace InTempo.Classes.Abstract
{
    public abstract class RiunioniGenerali
    {

        protected TimeSpan TempoTotale { get; }
        protected int NumeroParti { get; }

        public abstract ObservableCollection<Parte> Parti { get; set; }
        protected RiunioniGenerali(TimeSpan t, int NParti)
        {
            TempoTotale = t;
            NumeroParti = NParti;
        }

        public TimeSpan GetTempoTotale()
        {
            return TempoTotale;
        }

        
    }
}
