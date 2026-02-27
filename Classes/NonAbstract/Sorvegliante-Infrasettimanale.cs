using InTempo.Classes.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;

namespace InTempo.Classes.NonAbstract
{
    internal class Sorvegliante_Infrasettimanale : Infrasettimanale
    {
        public override ObservableCollection<Parte> Parti { get; set; } = new ObservableCollection<Parte>();

        public Sorvegliante_Infrasettimanale()
        {
        }


        public void ModificaSchemaParti()
        {
            for (int i = 0; i < Parti.Count; i++)
            {
                if (Parti[i].NomeParte == "Studio biblico di congregazione")
                {
                    Parti[i] = new Parte("Discorso Sorvegliante", new TimeSpan(0, 30, 0), "Discorso Sorvegliante", Parti[i].ColoreParte, new TimeSpan(0, 30, 0), Parti[i].NumeroParte);
                    return;
                }
                if (Parti[i].TipoParte == "Cantico")
                {
                    Parti[i] = new Parte("Cantico Sorvegliante", new TimeSpan(0, 5, 0), "Cantico Sorvegliante", Parti[i].ColoreParte, new TimeSpan(0, 5, 0), Parti[i].NumeroParte);
                    return;
                }
            }
        }

        public async Task CaricaSchema()
        {
            await base.LoadAsync();
            Parti = base.Parti;
            ModificaSchemaParti();
        }


    }
}
