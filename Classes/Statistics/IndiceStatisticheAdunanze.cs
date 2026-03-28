using System;
using System.Collections.Generic;
using System.Linq;

namespace InTempo.Classes.Statistics
{
    public sealed class IndiceStatisticheAdunanze
    {
        public int Versione { get; set; } = 1;

        public List<VoceIndiceStatisticheAdunanza> Sessioni { get; set; } = new List<VoceIndiceStatisticheAdunanza>();

        public VoceIndiceStatisticheAdunanza? Trova(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            return Sessioni.FirstOrDefault(sessione =>
                string.Equals(sessione.SessionId, sessionId, StringComparison.OrdinalIgnoreCase));
        }

        public void AggiornaOAggiungi(VoceIndiceStatisticheAdunanza voce)
        {
            if (voce == null)
            {
                return;
            }

            int index = Sessioni.FindIndex(sessione =>
                string.Equals(sessione.SessionId, voce.SessionId, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                Sessioni[index] = voce;
            }
            else
            {
                Sessioni.Add(voce);
            }
        }

        public bool Rimuovi(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return false;
            }

            return Sessioni.RemoveAll(sessione =>
                string.Equals(sessione.SessionId, sessionId, StringComparison.OrdinalIgnoreCase)) > 0;
        }

        public void OrdinaPerDataDiscendente()
        {
            Sessioni = Sessioni
                .OrderByDescending(sessione => sessione.InizioUtc)
                .ThenByDescending(sessione => sessione.UltimoAggiornamentoUtc)
                .ToList();
        }

        public IReadOnlyList<VoceIndiceStatisticheAdunanza> SoloAperteOInterrotte()
        {
            return Sessioni
                .Where(sessione => sessione.Stato != StatoSessioneStatistiche.Chiusa)
                .OrderByDescending(sessione => sessione.InizioUtc)
                .ToList();
        }
    }
}
