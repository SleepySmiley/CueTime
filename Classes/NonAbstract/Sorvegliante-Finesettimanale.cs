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


        public void ModificaSchemaParti()
        {

            for (int i = 0; i < Parti.Count; i++)
            {
                if (Parti[i].NomeParte == "Studio Torre di Guardia")
                {
                    Parti[i] = new Parte("Discorso Sorvegliante", new TimeSpan(0, 30, 0), "Discorso Sorvegliante", Parti[i].ColoreParte, new TimeSpan(0, 30, 0), Parti[i].NumeroParte);
                }
                if (Parti[i].TipoParte == "Cantico")
                {
                    Parti[i] = new Parte("Cantico Sorvegliante", new TimeSpan(0, 5, 0), "Cantico Sorvegliante", Parti[i].ColoreParte, new TimeSpan(0, 5, 0), Parti[i].NumeroParte);
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
