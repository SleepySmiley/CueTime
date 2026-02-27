using System;

namespace InTempo.Classes.NonAbstract
{
    public class Sorvegliante_Finesettimanale : Finesettimanale
    {
        public Sorvegliante_Finesettimanale() { }

        public void ModificaSchemaParti()
        {
            for (int i = 0; i < Parti.Count; i++)
            {
                if (Parti[i].NomeParte.Equals("Studio Torre di Guardia", StringComparison.OrdinalIgnoreCase))
                {
                    Parti[i] = new Parte(
                        "Discorso Sorvegliante",
                        TimeSpan.FromMinutes(30),
                        "Discorso Sorvegliante",
                        Parti[i].ColoreParte,
                        TimeSpan.FromMinutes(30),
                        Parti[i].NumeroParte);
                }


                if (i == Parti.Count - 1)
                {
                    Parti[i] = new Parte(
                        "Cantico Sorvegliante",
                        TimeSpan.FromMinutes(5),
                        "Cantico Sorvegliante",
                        Parti[i].ColoreParte,
                        TimeSpan.FromMinutes(5),
                        Parti[i].NumeroParte);
                }
            }
        }

        public async Task CaricaSchema()
        {
            await base.LoadAsync();
            ModificaSchemaParti();
        }
    }
}
