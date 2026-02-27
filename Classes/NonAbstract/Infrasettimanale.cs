using InTempo.Classes.Abstract;
using InTempo.Classes.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace InTempo.Classes.NonAbstract
{
    public class Infrasettimanale : RiunioniGenerali
    {
        public override ObservableCollection<Parte> Parti { get; set; } = new();

        public async Task LoadAsync()
        {
            var loaded = await WebPartsLoader.CaricaInfrasettimanaleAsync();
            Parti = loaded;
        }

       
        public Infrasettimanale() : base(new TimeSpan(1,45,0), 14)
        {
        }

        public Parte this[int i]
        {
            get { return Parti[i]; }
            set { Parti[i] = value; }
        }

    }
}
