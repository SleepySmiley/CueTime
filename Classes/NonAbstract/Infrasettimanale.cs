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
            Parti.Clear();
            var loaded = await WebPartsLoader.CaricaInfrasettimanaleAsync().ConfigureAwait(false);
            foreach (var p in loaded)
                Parti.Add(p);
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
