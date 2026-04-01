using System;
using System.Collections.Generic;
using System.Linq;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.NonAbstract
{
    public class SorveglianteFinesettimanale : Finesettimanale
    {
        private static readonly TimeSpan DurataCantico = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DurataParteSorvegliante = TimeSpan.FromMinutes(30);

        public void ModificaSchemaParti()
        {
            IReadOnlyList<Parte> partiOriginali = Parti.ToList();
            ParteFactory factory = new ParteFactory();
            IReadOnlyList<Parte> schemaWeekendBase = factory.BuildWeekendStock().ToList();

            Parte canticoInizialeSource = TrovaParteConNumero(partiOriginali, 1) ?? schemaWeekendBase[0];
            Parte discorsoPubblicoSource = TrovaParteConNome(partiOriginali, "Discorso pubblico") ?? TrovaParteConNumero(partiOriginali, 2) ?? schemaWeekendBase[1];
            Parte canticoTorreDiGuardiaSource = RisolviCanticoTorreDiGuardiaSource(partiOriginali, schemaWeekendBase);
            Parte studioTorreDiGuardiaSource = TrovaParteConNome(partiOriginali, "Studio Torre di Guardia") ?? schemaWeekendBase[3];
            Parte? discorsoSorveglianteSource = TrovaParteConNome(partiOriginali, "Discorso Sorvegliante");
            Parte? canticoSorveglianteSource = TrovaCanticoSorvegliante(partiOriginali, discorsoSorveglianteSource != null);

            SostituisciParti(
                CreaParteNormalizzata(
                    canticoInizialeSource,
                    canticoInizialeSource.NomeParte,
                    ParteFactory.TypeCantico,
                    DurataCantico,
                    1),
                CreaParteNormalizzata(
                    discorsoPubblicoSource,
                    "Discorso pubblico",
                    ParteFactory.TypeDiscorso,
                    DurataParteSorvegliante,
                    2),
                CreaParteNormalizzata(
                    canticoTorreDiGuardiaSource,
                    canticoTorreDiGuardiaSource.NomeParte,
                    ParteFactory.TypeCantico,
                    DurataCantico,
                    3),
                CreaParteNormalizzata(
                    studioTorreDiGuardiaSource,
                    "Studio Torre di Guardia",
                    ParteFactory.TypeStudio,
                    DurataParteSorvegliante,
                    4),
                CreaParteNormalizzata(
                    discorsoSorveglianteSource ?? studioTorreDiGuardiaSource,
                    "Discorso Sorvegliante",
                    ParteFactory.TypeDiscorso,
                    DurataParteSorvegliante,
                    5),
                CreaParteNormalizzata(
                    canticoSorveglianteSource ?? schemaWeekendBase[4],
                    canticoSorveglianteSource?.NomeParte ?? "Cantico Sorvegliante",
                    ParteFactory.TypeCantico,
                    DurataCantico,
                    6));
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

        private void SostituisciParti(params Parte[] nuoveParti)
        {
            Parti.Clear();

            foreach (Parte parte in nuoveParti)
            {
                Parti.Add(parte);
            }
        }

        private static Parte? TrovaParteConNumero(IEnumerable<Parte> parti, int numeroParte)
        {
            return parti.FirstOrDefault(parte => parte.NumeroParte == numeroParte);
        }

        private static Parte? TrovaParteConNome(IEnumerable<Parte> parti, string nomeParte)
        {
            return parti.FirstOrDefault(parte => parte.NomeParte.Equals(nomeParte, StringComparison.OrdinalIgnoreCase));
        }

        private Parte RisolviCanticoTorreDiGuardiaSource(IReadOnlyList<Parte> partiOriginali, IReadOnlyList<Parte> schemaWeekendBase)
        {
            Parte sorgente = TrovaParteConNumero(partiOriginali, 3) ?? schemaWeekendBase[2];
            if (HaNumeroCantico(sorgente.NomeParte))
            {
                return sorgente;
            }

            if (TryResolveCanticoTorreDiGuardiaDaParser(sorgente, out Parte parteDalParser))
            {
                return parteDalParser;
            }

            return sorgente;
        }

        private static Parte? TrovaCanticoSorvegliante(IEnumerable<Parte> parti, bool schemaSorveglianteGiaPresente)
        {
            Parte? canticoEsplicito = TrovaParteConNome(parti, "Cantico Sorvegliante");
            if (canticoEsplicito != null)
            {
                return canticoEsplicito;
            }

            if (!schemaSorveglianteGiaPresente)
            {
                return null;
            }

            return parti.LastOrDefault(parte =>
                parte.NomeParte.StartsWith("Cantico", StringComparison.OrdinalIgnoreCase)
                || parte.TipoParte.Equals(ParteFactory.TypeCantico, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryResolveCanticoTorreDiGuardiaDaParser(Parte modello, out Parte parteDalParser)
        {
            parteDalParser = modello;

            try
            {
                string? studyHtml = TryLeggiHtmlStudioTorreDiGuardiaDaCache();
                if (string.IsNullOrWhiteSpace(studyHtml))
                {
                    return false;
                }

                HtmlParteParser parser = new HtmlParteParser();
                WeekendSongSelection songs = parser.ExtractWeekendSongsFromWtStudyHtml(studyHtml);
                if (!songs.Song2.HasValue)
                {
                    return false;
                }

                parteDalParser = new Parte(
                    $"Cantico {songs.Song2.Value}",
                    DurataCantico,
                    ParteFactory.TypeCantico,
                    modello.ColoreParte,
                    DurataCantico,
                    3);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("Impossibile recuperare dalla cache il cantico della Torre di Guardia per il fine settimana del sorvegliante.", ex);
                parteDalParser = modello;
                return false;
            }
        }

        private string? TryLeggiHtmlStudioTorreDiGuardiaDaCache()
        {
            WebPartsCache cache = new WebPartsCache();
            DateTime referenceDate = DataRiferimento.Date;
            (int year, int week) = WebPartsCache.GetIsoWeek(referenceDate);
            string currentStudyPath = cache.GetWeekendStudyCachePath(year, week);

            if (cache.TryReadText(currentStudyPath, out string currentStudyHtml) && !string.IsNullOrWhiteSpace(currentStudyHtml))
            {
                return currentStudyHtml;
            }

            return null;
        }

        private static bool HaNumeroCantico(string nomeParte)
        {
            return nomeParte.StartsWith("Cantico ", StringComparison.OrdinalIgnoreCase)
                && nomeParte.Length > "Cantico ".Length
                && int.TryParse(nomeParte["Cantico ".Length..], out _);
        }

        private static Parte CreaParteNormalizzata(
            Parte modello,
            string nomeParte,
            string tipoParte,
            TimeSpan durata,
            int numeroParte)
        {
            TimeSpan tempoScorrevole = modello.TempoScorrevole;
            if (tempoScorrevole > durata)
            {
                tempoScorrevole = durata;
            }

            return new Parte(
                nomeParte,
                durata,
                tipoParte,
                modello.ColoreParte,
                tempoScorrevole <= TimeSpan.Zero && modello.TempoParte <= TimeSpan.Zero ? durata : tempoScorrevole,
                numeroParte);
        }
    }
}
