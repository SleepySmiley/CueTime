using System;
using System.Linq;

namespace InTempo.Classes.NonAbstract
{
    internal class SorveglianteInfrasettimanale : Infrasettimanale
    {
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

            SpostaCommentiConclusiviPrimaDelDiscorsoSorvegliante();
        }

        public async Task CaricaSchema()
        {
            await LoadAsync();
            ModificaSchemaParti();
        }

        public void CaricaSchemaDaCache()
        {
            LoadFromCache();
            ModificaSchemaParti();
        }

        private void SpostaCommentiConclusiviPrimaDelDiscorsoSorvegliante()
        {
            int indiceDiscorso = Parti
                .Select((parte, indice) => new { parte, indice })
                .FirstOrDefault(item => item.parte.NomeParte.Equals("Discorso Sorvegliante", StringComparison.OrdinalIgnoreCase))
                ?.indice ?? -1;

            int indiceCommenti = Parti
                .Select((parte, indice) => new { parte, indice })
                .FirstOrDefault(item => item.parte.NomeParte.Equals("Commenti conclusivi", StringComparison.OrdinalIgnoreCase))
                ?.indice ?? -1;

            if (indiceDiscorso < 0 || indiceCommenti < 0 || indiceCommenti < indiceDiscorso)
            {
                return;
            }

            Parte commentiConclusivi = Parti[indiceCommenti];
            Parte discorsoSorvegliante = Parti[indiceDiscorso];

            if (commentiConclusivi.NumeroParte.HasValue && discorsoSorvegliante.NumeroParte.HasValue)
            {
                int? numeroTemporaneo = commentiConclusivi.NumeroParte;
                commentiConclusivi.NumeroParte = discorsoSorvegliante.NumeroParte;
                discorsoSorvegliante.NumeroParte = numeroTemporaneo;
            }

            Parti.Move(indiceCommenti, indiceDiscorso);
        }
    }
}
