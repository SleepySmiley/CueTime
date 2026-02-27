using System;

namespace InTempo.Classes.NonAbstract
{
    internal class Sorvegliante_Infrasettimanale : Infrasettimanale
    {
        public Sorvegliante_Infrasettimanale() { }

        public void ModificaSchemaParti()
        {
            for (int i = 0; i < Parti.Count; i++)
            {
                if (Parti[i].NomeParte.Equals("Studio biblico di congregazione", StringComparison.OrdinalIgnoreCase))
                {
                    Parti[i] = new Parte(
                        "Discorso Sorvegliante",
                        TimeSpan.FromMinutes(30),
                        "Discorso Sorvegliante",
                        Parti[i].ColoreParte,
                        TimeSpan.FromMinutes(30),
                        Parti[i].NumeroParte);
                }

                if (Parti[i].TipoParte.Equals("Cantico", StringComparison.OrdinalIgnoreCase))
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
