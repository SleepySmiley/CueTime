using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using InTempo.Classes.Abstract;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.NonAbstract
{
    public class Finesettimanale : RiunioniGenerali
    {

        public override ObservableCollection<Parte> Parti { get; set; } = new();

        public async Task LoadAsync()
        {
            var loaded = await WebPartsLoader.CaricaFineSettimanaAsync();
            Parti = loaded;
        }


        public Finesettimanale() : base(new TimeSpan(1,45,0), 5)
        {
        }

        public Parte this[int i]
        {
            get { return Parti[i]; }
            set { Parti[i] = value; }
        }


    }
}
