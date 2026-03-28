using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using InTempo.Classes.NonAbstract;
using InTempo.Classes.Utilities;

namespace InTempo.Classes.Statistics
{
    public sealed class SnapshotParteStatistiche
    {
        public string NomeParte { get; set; } = string.Empty;

        public string TipoParte { get; set; } = string.Empty;

        public string ColoreSalvato { get; set; } = "#FF000000";

        public int? NumeroParte { get; set; }

        public TimeSpan TempoParte { get; set; }

        public TimeSpan TempoScorrevole { get; set; }

        public int OrdineOriginale { get; set; }

        public static SnapshotParteStatistiche DaParte(Parte parte, int ordineOriginale)
        {
            return new SnapshotParteStatistiche
            {
                NomeParte = parte?.NomeParte ?? string.Empty,
                TipoParte = parte?.TipoParte ?? string.Empty,
                ColoreSalvato = parte?.ColoreSalvato ?? "#FF000000",
                NumeroParte = parte?.NumeroParte,
                TempoParte = parte?.TempoParte ?? TimeSpan.Zero,
                TempoScorrevole = parte?.TempoScorrevole ?? TimeSpan.Zero,
                OrdineOriginale = ordineOriginale
            };
        }
    }

    public sealed class ModificaParteStatistiche
    {
        public SnapshotParteStatistiche ValorePrecedente { get; set; } = new SnapshotParteStatistiche();

        public SnapshotParteStatistiche ValoreNuovo { get; set; } = new SnapshotParteStatistiche();

        public DateTimeOffset TimestampUtc { get; set; }
    }

    public sealed class IntervalloEsecuzioneParteStatistiche
    {
        public DateTimeOffset InizioUtc { get; set; }

        public DateTimeOffset? FineUtc { get; set; }

        [JsonIgnore]
        public TimeSpan Durata => (FineUtc ?? InizioUtc) - InizioUtc;
    }

    public sealed class EventoPausaStatistiche
    {
        public DateTimeOffset InizioUtc { get; set; }

        public DateTimeOffset? FineUtc { get; set; }

        public MotivoPausaStatistiche Motivo { get; set; } = MotivoPausaStatistiche.Utente;

        [JsonIgnore]
        public TimeSpan Durata => FineUtc.HasValue ? FineUtc.Value - InizioUtc : TimeSpan.Zero;
    }

    public sealed class EventoCambioParteStatistiche
    {
        public DateTimeOffset TimestampUtc { get; set; }

        public TipoCambioParteStatistiche Tipo { get; set; } = TipoCambioParteStatistiche.Tecnico;

        public string? PartePrecedente { get; set; }

        public string? ParteSuccessiva { get; set; }

        public int? IndicePartePrecedente { get; set; }

        public int? IndiceParteSuccessiva { get; set; }
    }

    public sealed class StatisticaParteAggregata
    {
        public string NomeParte { get; set; } = string.Empty;

        public int NumeroSforamenti { get; set; }

        public TimeSpan TempoTotaleSforamento { get; set; }

        public TimeSpan TempoMedioSforamento { get; set; }

        public TimeSpan SforamentoMassimo { get; set; }
    }

    public sealed class CampioneAndamentoAdunanza
    {
        public string SessionId { get; set; } = string.Empty;

        public DateTimeOffset InizioUtc { get; set; }

        public TipoAdunanzaStatistiche TipoAdunanza { get; set; } = TipoAdunanzaStatistiche.Sconosciuta;

        public TimeSpan ScostamentoFinale { get; set; }

        public bool TerminataInOrario { get; set; }
    }

    public sealed class StatisticheParteAdunanza
    {
        public string NomeParte { get; set; } = string.Empty;

        public string NomeParteAllAvvioAdunanza { get; set; } = string.Empty;

        public string TipoParte { get; set; } = string.Empty;

        public int? NumeroParte { get; set; }

        public TimeSpan? DurataPrevistaAllAvvio { get; set; }

        public TimeSpan? DurataPrevistaQuandoParteInizia { get; set; }

        public TimeSpan DurataRealeEffettiva { get; set; }

        public TimeSpan? DifferenzaPrevistaAllAvvioEReale { get; set; }

        public TimeSpan? DifferenzaPrevistaAllInizioParteEReale { get; set; }

        public bool EAndataFuoriTempo { get; set; }

        public TimeSpan TempoTotaleSottoZero { get; set; }

        public DateTimeOffset? TimestampInizioSottoZeroUtc { get; set; }

        public bool EStataModificata { get; set; }

        public int NumeroModifiche { get; set; }

        public List<ModificaParteStatistiche> Modifiche { get; set; } = new List<ModificaParteStatistiche>();

        public bool EStataSaltata { get; set; }

        public bool EStataRimossa { get; set; }

        public bool EStataAggiuntaInCorso { get; set; }

        public int OrdineOriginale { get; set; }

        public int? OrdineEffettivo { get; set; }

        public DateTimeOffset? OraEsattaInizioUtc { get; set; }

        public DateTimeOffset? OraEsattaFineUtc { get; set; }

        public List<IntervalloEsecuzioneParteStatistiche> IntervalliEsecuzione { get; set; } = new List<IntervalloEsecuzioneParteStatistiche>();

        public void RicalcolaDerivati(DateTimeOffset? riferimentoUtc = null)
        {
            TimeSpan durataCalcolata = TimeSpan.Zero;

            foreach (IntervalloEsecuzioneParteStatistiche intervallo in IntervalliEsecuzione)
            {
                DateTimeOffset fine = intervallo.FineUtc ?? riferimentoUtc ?? intervallo.InizioUtc;
                if (fine > intervallo.InizioUtc)
                {
                    durataCalcolata += fine - intervallo.InizioUtc;
                }
            }

            if (durataCalcolata > TimeSpan.Zero)
            {
                DurataRealeEffettiva = durataCalcolata;
            }

            DifferenzaPrevistaAllAvvioEReale = DurataPrevistaAllAvvio.HasValue
                ? DurataRealeEffettiva - DurataPrevistaAllAvvio.Value
                : null;

            DifferenzaPrevistaAllInizioParteEReale = DurataPrevistaQuandoParteInizia.HasValue
                ? DurataRealeEffettiva - DurataPrevistaQuandoParteInizia.Value
                : null;

            EAndataFuoriTempo = TempoTotaleSottoZero > TimeSpan.Zero;
            NumeroModifiche = Modifiche.Count;
            EStataModificata = NumeroModifiche > 0;
        }
    }

    public sealed class StatisticheAdunanzaSessione
    {
        private static readonly TimeSpan SogliaMinimaAdunanzaReale = TimeSpan.FromMinutes(5);

        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");

        public string NomeFileDettaglio { get; set; } = string.Empty;

        public TipoAdunanzaStatistiche TipoAdunanza { get; set; } = TipoAdunanzaStatistiche.Sconosciuta;

        public DateTime DataRiferimentoAdunanza { get; set; }

        public DateTimeOffset InizioUtc { get; set; }

        public DateTimeOffset? FineUtc { get; set; }

        public DateTimeOffset UltimoAggiornamentoUtc { get; set; }

        public StatoSessioneStatistiche Stato { get; set; } = StatoSessioneStatistiche.Aperta;

        public MotivoChiusuraStatistiche? MotivoChiusura { get; set; }

        public bool SessioneCompletaPerScaletta { get; set; }

        public TimeSpan DurataProgrammatoTotale { get; set; }

        public TimeSpan DurataRealeTotale { get; set; }

        public TimeSpan ScostamentoFinale { get; set; }

        public bool TerminataInOrario { get; set; }

        public int NumeroTotaleParti { get; set; }

        public int NumeroPartiInOrario { get; set; }

        public int NumeroPartiFuoriTempo { get; set; }

        public double PercentualePartiInOrario { get; set; }

        public TimeSpan TempoTotaleSforamenti { get; set; }

        public TimeSpan TempoProgrammatoPersoPerPartiRimosse { get; set; }

        public int NumeroPause { get; set; }

        public int NumeroRiprese { get; set; }

        public int NumeroCambiManualiAvanti { get; set; }

        public int NumeroCambiManualiIndietro { get; set; }

        public int NumeroCambiAutomatici { get; set; }

        public int NumeroModificheTotali { get; set; }

        public int NumeroPartiEliminate { get; set; }

        public int NumeroPartiAggiunte { get; set; }

        public string? NomeParteConSforamentoMassimo { get; set; }

        public TimeSpan SforamentoMassimoParte { get; set; }

        public List<StatisticheParteAdunanza> Parti { get; set; } = new List<StatisticheParteAdunanza>();

        public List<EventoPausaStatistiche> Pause { get; set; } = new List<EventoPausaStatistiche>();

        public List<EventoCambioParteStatistiche> CambiParte { get; set; } = new List<EventoCambioParteStatistiche>();

        public void RicalcolaRiepilogo(DateTimeOffset? riferimentoUtc = null)
        {
            foreach (StatisticheParteAdunanza parte in Parti)
            {
                parte.RicalcolaDerivati(riferimentoUtc);
            }

            NumeroTotaleParti = Parti.Count;
            NumeroPartiInOrario = Parti.Count(parte => parte.OraEsattaFineUtc.HasValue && !parte.EAndataFuoriTempo);
            NumeroPartiFuoriTempo = Parti.Count(parte => parte.OraEsattaFineUtc.HasValue && parte.EAndataFuoriTempo);
            PercentualePartiInOrario = NumeroTotaleParti == 0
                ? 0
                : (double)NumeroPartiInOrario * 100d / NumeroTotaleParti;
            TempoTotaleSforamenti = Parti.Aggregate(TimeSpan.Zero, (totale, parte) => totale + parte.TempoTotaleSottoZero);
            TempoProgrammatoPersoPerPartiRimosse = Parti
                .Where(parte => parte.EStataRimossa)
                .Aggregate(TimeSpan.Zero, (totale, parte) => totale + (parte.DurataPrevistaAllAvvio ?? TimeSpan.Zero));
            NumeroPause = Pause.Count;
            NumeroRiprese = Pause.Count(pausa => pausa.FineUtc.HasValue);
            NumeroCambiManualiAvanti = CambiParte.Count(cambio => cambio.Tipo == TipoCambioParteStatistiche.ManualeAvanti);
            NumeroCambiManualiIndietro = CambiParte.Count(cambio => cambio.Tipo == TipoCambioParteStatistiche.ManualeIndietro);
            NumeroCambiAutomatici = CambiParte.Count(cambio => cambio.Tipo == TipoCambioParteStatistiche.Automatico);
            NumeroModificheTotali = Parti.Sum(parte => parte.NumeroModifiche);
            NumeroPartiEliminate = Parti.Count(parte => parte.EStataRimossa);
            NumeroPartiAggiunte = Parti.Count(parte => parte.EStataAggiuntaInCorso);
            DurataRealeTotale = Parti.Aggregate(TimeSpan.Zero, (totale, parte) => totale + parte.DurataRealeEffettiva);
            ScostamentoFinale = DurataRealeTotale - DurataProgrammatoTotale;
            TerminataInOrario = ScostamentoFinale <= TimeSpan.Zero;
            SessioneCompletaPerScaletta = Parti.All(parte => parte.OraEsattaInizioUtc.HasValue || parte.EStataRimossa || parte.EStataSaltata);

            StatisticheParteAdunanza? partePeggiore = Parti
                .OrderByDescending(parte => parte.TempoTotaleSottoZero)
                .FirstOrDefault();

            if (partePeggiore != null && partePeggiore.TempoTotaleSottoZero > TimeSpan.Zero)
            {
                NomeParteConSforamentoMassimo = partePeggiore.NomeParte;
                SforamentoMassimoParte = partePeggiore.TempoTotaleSottoZero;
            }
            else
            {
                NomeParteConSforamentoMassimo = null;
                SforamentoMassimoParte = TimeSpan.Zero;
            }
        }

        public bool EAdunanzaNulla()
        {
            int numeroPartiEffettivamenteAvviate = Parti.Count(parte => parte.OraEsattaInizioUtc.HasValue);
            bool haSoloPrimaParteAvviata = numeroPartiEffettivamenteAvviate <= 1;
            bool nessunCambioRealeDiScaletta = CambiParte.All(cambio => cambio.Tipo == TipoCambioParteStatistiche.AvvioSessione);
            bool nessunaAlterazioneManuale =
                NumeroModificheTotali == 0
                && NumeroPartiEliminate == 0
                && NumeroPartiAggiunte == 0;
            bool nessunoSforamento = TempoTotaleSforamenti <= TimeSpan.Zero;
            bool durataTroppoBreve = DurataRealeTotale < SogliaMinimaAdunanzaReale;

            return numeroPartiEffettivamenteAvviate == 0
                || (durataTroppoBreve
                    && haSoloPrimaParteAvviata
                    && nessunCambioRealeDiScaletta
                    && nessunaAlterazioneManuale
                    && nessunoSforamento);
        }
    }

    public sealed class RisultatiStatisticheStoriche
    {
        public Dictionary<TipoAdunanzaStatistiche, TimeSpan> MediaRitardoAnticipoPerTipo { get; set; } = new Dictionary<TipoAdunanzaStatistiche, TimeSpan>();

        public Dictionary<TipoAdunanzaStatistiche, double> MediaPartiFuoriTempoPerTipo { get; set; } = new Dictionary<TipoAdunanzaStatistiche, double>();

        public double PercentualeAdunanzeFiniteInOrarioSulTotaleStorico { get; set; }

        public Dictionary<TipoAdunanzaStatistiche, double> PercentualeAdunanzeFiniteInOrarioPerTipo { get; set; } = new Dictionary<TipoAdunanzaStatistiche, double>();

        public List<StatisticaParteAggregata> Top5PartiChePiuSpessoSforano { get; set; } = new List<StatisticaParteAggregata>();

        public List<StatisticaParteAggregata> Top5PartiConSforamentoMedioPiuAlto { get; set; } = new List<StatisticaParteAggregata>();

        public List<CampioneAndamentoAdunanza> AndamentoUltime10Adunanze { get; set; } = new List<CampioneAndamentoAdunanza>();

        public TimeSpan MediaRitardoAnticipoInfrasettimanale { get; set; }

        public TimeSpan MediaRitardoAnticipoFinesettimanale { get; set; }

        public double MediaNumeroPausePerAdunanza { get; set; }

        public double MediaNumeroModifichePerAdunanza { get; set; }

        public VoceIndiceStatisticheAdunanza? AdunanzaMiglioreAssoluta { get; set; }

        public VoceIndiceStatisticheAdunanza? AdunanzaPeggioreAssoluta { get; set; }

        public string? MeseConPiuAdunanzeInOrario { get; set; }

        public int StreakCorrenteAdunanzeConsecutiveFiniteInOrario { get; set; }

        public static RisultatiStatisticheStoriche Calcola(IEnumerable<StatisticheAdunanzaSessione> sessioni)
        {
            List<StatisticheAdunanzaSessione> elenco = sessioni
                .Where(sessione => sessione != null)
                .OrderBy(sessione => sessione.InizioUtc)
                .ToList();

            RisultatiStatisticheStoriche risultati = new RisultatiStatisticheStoriche();

            if (elenco.Count == 0)
            {
                return risultati;
            }

            risultati.MediaRitardoAnticipoPerTipo = elenco
                .GroupBy(sessione => sessione.TipoAdunanza)
                .ToDictionary(
                    gruppo => gruppo.Key,
                    gruppo => TimeSpan.FromTicks((long)Math.Round(gruppo.Average(sessione => sessione.ScostamentoFinale.Ticks))));

            risultati.MediaPartiFuoriTempoPerTipo = elenco
                .GroupBy(sessione => sessione.TipoAdunanza)
                .ToDictionary(
                    gruppo => gruppo.Key,
                    gruppo => gruppo.Average(sessione => sessione.NumeroPartiFuoriTempo));

            risultati.PercentualeAdunanzeFiniteInOrarioSulTotaleStorico =
                (double)elenco.Count(sessione => sessione.TerminataInOrario) * 100d / elenco.Count;

            risultati.PercentualeAdunanzeFiniteInOrarioPerTipo = elenco
                .GroupBy(sessione => sessione.TipoAdunanza)
                .ToDictionary(
                    gruppo => gruppo.Key,
                    gruppo => (double)gruppo.Count(sessione => sessione.TerminataInOrario) * 100d / gruppo.Count());

            List<StatisticheParteAdunanza> tutteLeParti = elenco.SelectMany(sessione => sessione.Parti).ToList();

            risultati.Top5PartiChePiuSpessoSforano = tutteLeParti
                .Where(parte => parte.TempoTotaleSottoZero > TimeSpan.Zero)
                .GroupBy(parte => NormalizzaNomeParte(parte.NomeParteAllAvvioAdunanza))
                .Select(gruppo => CreaStatisticaParteAggregata(gruppo.Key, gruppo))
                .OrderByDescending(voce => voce.NumeroSforamenti)
                .ThenByDescending(voce => voce.TempoTotaleSforamento)
                .Take(5)
                .ToList();

            risultati.Top5PartiConSforamentoMedioPiuAlto = tutteLeParti
                .Where(parte => parte.TempoTotaleSottoZero > TimeSpan.Zero)
                .GroupBy(parte => NormalizzaNomeParte(parte.NomeParteAllAvvioAdunanza))
                .Select(gruppo => CreaStatisticaParteAggregata(gruppo.Key, gruppo))
                .OrderByDescending(voce => voce.TempoMedioSforamento)
                .ThenByDescending(voce => voce.TempoTotaleSforamento)
                .Take(5)
                .ToList();

            risultati.AndamentoUltime10Adunanze = elenco
                .TakeLast(10)
                .Select(sessione => new CampioneAndamentoAdunanza
                {
                    SessionId = sessione.SessionId,
                    InizioUtc = sessione.InizioUtc,
                    TipoAdunanza = sessione.TipoAdunanza,
                    ScostamentoFinale = sessione.ScostamentoFinale,
                    TerminataInOrario = sessione.TerminataInOrario
                })
                .ToList();

            risultati.MediaRitardoAnticipoInfrasettimanale = CalcolaMediaScostamento(elenco, TipoAdunanzaStatistiche.Infrasettimanale);
            risultati.MediaRitardoAnticipoFinesettimanale = CalcolaMediaScostamento(elenco, TipoAdunanzaStatistiche.Finesettimanale);
            risultati.MediaNumeroPausePerAdunanza = elenco.Average(sessione => sessione.NumeroPause);
            risultati.MediaNumeroModifichePerAdunanza = elenco.Average(sessione => sessione.NumeroModificheTotali);
            risultati.AdunanzaMiglioreAssoluta = elenco.OrderBy(sessione => sessione.ScostamentoFinale).Select(VoceIndiceStatisticheAdunanza.DaSessione).FirstOrDefault();
            risultati.AdunanzaPeggioreAssoluta = elenco.OrderByDescending(sessione => sessione.ScostamentoFinale).Select(VoceIndiceStatisticheAdunanza.DaSessione).FirstOrDefault();
            risultati.MeseConPiuAdunanzeInOrario = elenco
                .Where(sessione => sessione.TerminataInOrario)
                .GroupBy(sessione => sessione.InizioUtc.ToLocalTime().ToString("yyyy-MM"))
                .OrderByDescending(gruppo => gruppo.Count())
                .Select(gruppo => gruppo.Key)
                .FirstOrDefault();
            risultati.StreakCorrenteAdunanzeConsecutiveFiniteInOrario = elenco
                .OrderByDescending(sessione => sessione.InizioUtc)
                .TakeWhile(sessione => sessione.TerminataInOrario)
                .Count();

            return risultati;
        }

        private static StatisticaParteAggregata CreaStatisticaParteAggregata(string nomeParte, IEnumerable<StatisticheParteAdunanza> gruppo)
        {
            List<StatisticheParteAdunanza> parti = gruppo.ToList();
            long mediaTicks = (long)Math.Round(parti.Average(parte => parte.TempoTotaleSottoZero.Ticks));

            return new StatisticaParteAggregata
            {
                NomeParte = nomeParte,
                NumeroSforamenti = parti.Count,
                TempoTotaleSforamento = parti.Aggregate(TimeSpan.Zero, (totale, parte) => totale + parte.TempoTotaleSottoZero),
                TempoMedioSforamento = TimeSpan.FromTicks(mediaTicks),
                SforamentoMassimo = parti.Max(parte => parte.TempoTotaleSottoZero)
            };
        }

        private static string NormalizzaNomeParte(string? nomeParte)
        {
            if (string.IsNullOrWhiteSpace(nomeParte))
            {
                return "Parte sconosciuta";
            }

            string nome = nomeParte.Trim();
            if (nome.StartsWith("Cantico ", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(nome["Cantico ".Length..], out _))
            {
                return "Cantico";
            }

            return nome;
        }

        private static TimeSpan CalcolaMediaScostamento(IEnumerable<StatisticheAdunanzaSessione> sessioni, TipoAdunanzaStatistiche tipo)
        {
            List<StatisticheAdunanzaSessione> filtrate = sessioni.Where(sessione => sessione.TipoAdunanza == tipo).ToList();
            if (filtrate.Count == 0)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromTicks((long)Math.Round(filtrate.Average(sessione => sessione.ScostamentoFinale.Ticks)));
        }
    }

    public sealed class ArchivioStatisticheAdunanze
    {
        private static readonly object SyncRoot = new object();
        private static readonly System.Text.Json.JsonSerializerOptions SerializerOptions = AtomicJsonFileStore.CreateDefaultOptions();
        private readonly string _statsDirectoryPath;
        private readonly string _indexPath;

        public ArchivioStatisticheAdunanze(string? statsDirectoryPath = null)
        {
            _statsDirectoryPath = ResolveStatsDirectoryPath(statsDirectoryPath);
            _indexPath = Path.Combine(_statsDirectoryPath, "index.json");
            Directory.CreateDirectory(_statsDirectoryPath);
        }

        public string StatsDirectoryPath => _statsDirectoryPath;

        public string IndexPath => _indexPath;

        public StatisticheAdunanzaSessione CreaSessione(
            TipoAdunanzaStatistiche tipoAdunanza,
            DateTime dataRiferimentoAdunanza,
            DateTimeOffset inizioUtc,
            TimeSpan durataProgrammatoTotale,
            IEnumerable<SnapshotParteStatistiche>? snapshotParti = null)
        {
            StatisticheAdunanzaSessione sessione = new StatisticheAdunanzaSessione
            {
                SessionId = Guid.NewGuid().ToString("N"),
                TipoAdunanza = tipoAdunanza,
                DataRiferimentoAdunanza = dataRiferimentoAdunanza.Date,
                InizioUtc = inizioUtc,
                UltimoAggiornamentoUtc = inizioUtc,
                DurataProgrammatoTotale = durataProgrammatoTotale,
                Stato = StatoSessioneStatistiche.Aperta
            };

            sessione.NomeFileDettaglio = CreaNomeFileDettaglio(sessione.InizioUtc);

            if (snapshotParti != null)
            {
                foreach (SnapshotParteStatistiche snapshot in snapshotParti)
                {
                    sessione.Parti.Add(new StatisticheParteAdunanza
                    {
                        NomeParte = snapshot.NomeParte,
                        NomeParteAllAvvioAdunanza = snapshot.NomeParte,
                        TipoParte = snapshot.TipoParte,
                        NumeroParte = snapshot.NumeroParte,
                        DurataPrevistaAllAvvio = snapshot.TempoParte,
                        OrdineOriginale = snapshot.OrdineOriginale
                    });
                }
            }

            sessione.RicalcolaRiepilogo(inizioUtc);
            return sessione;
        }

        public IndiceStatisticheAdunanze CaricaIndice()
        {
            lock (SyncRoot)
            {
                IndiceStatisticheAdunanze indice = AtomicJsonFileStore.Load<IndiceStatisticheAdunanze>(_indexPath, SerializerOptions)
                    ?? new IndiceStatisticheAdunanze();
                indice.OrdinaPerDataDiscendente();
                return indice;
            }
        }

        public IReadOnlyList<VoceIndiceStatisticheAdunanza> CaricaStoricoLeggero()
        {
            return CaricaIndice().Sessioni;
        }

        public StatisticheAdunanzaSessione? CaricaDettaglio(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return null;
            }

            lock (SyncRoot)
            {
                VoceIndiceStatisticheAdunanza? voce = CaricaIndice().Trova(sessionId);
                return voce == null ? null : CaricaDettaglioDaVoce(voce);
            }
        }

        public bool SalvaSessione(StatisticheAdunanzaSessione sessione)
        {
            if (sessione == null)
            {
                return false;
            }

            lock (SyncRoot)
            {
                try
                {
                    sessione.NomeFileDettaglio = NormalizzaNomeFileDettaglio(sessione.NomeFileDettaglio, sessione.InizioUtc);
                    sessione.RicalcolaRiepilogo(sessione.FineUtc ?? sessione.UltimoAggiornamentoUtc);
                    AtomicJsonFileStore.Save(Path.Combine(_statsDirectoryPath, sessione.NomeFileDettaglio), sessione, SerializerOptions);

                    IndiceStatisticheAdunanze indice = AtomicJsonFileStore.Load<IndiceStatisticheAdunanze>(_indexPath, SerializerOptions)
                        ?? new IndiceStatisticheAdunanze();
                    indice.AggiornaOAggiungi(VoceIndiceStatisticheAdunanza.DaSessione(sessione));
                    indice.OrdinaPerDataDiscendente();
                    AtomicJsonFileStore.Save(_indexPath, indice, SerializerOptions);
                    return true;
                }
                catch (Exception ex)
                {
                    AppLogger.LogError("Errore durante il salvataggio delle statistiche di adunanza.", ex);
                    return false;
                }
            }
        }

        public bool EliminaSessione(StatisticheAdunanzaSessione sessione)
        {
            if (sessione == null)
            {
                return false;
            }

            lock (SyncRoot)
            {
                try
                {
                    string nomeFile = NormalizzaNomeFileDettaglio(sessione.NomeFileDettaglio, sessione.InizioUtc);
                    string percorsoDettaglio = Path.Combine(_statsDirectoryPath, nomeFile);
                    if (File.Exists(percorsoDettaglio))
                    {
                        File.Delete(percorsoDettaglio);
                    }

                    IndiceStatisticheAdunanze indice = AtomicJsonFileStore.Load<IndiceStatisticheAdunanze>(_indexPath, SerializerOptions)
                        ?? new IndiceStatisticheAdunanze();
                    indice.Rimuovi(sessione.SessionId);
                    indice.OrdinaPerDataDiscendente();
                    AtomicJsonFileStore.Save(_indexPath, indice, SerializerOptions);
                    return true;
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Errore durante l'eliminazione della sessione statistiche '{sessione.SessionId}'.", ex);
                    return false;
                }
            }
        }

        public IReadOnlyList<VoceIndiceStatisticheAdunanza> RecuperaSessioniInterrotte()
        {
            lock (SyncRoot)
            {
                IndiceStatisticheAdunanze indice = AtomicJsonFileStore.Load<IndiceStatisticheAdunanze>(_indexPath, SerializerOptions)
                    ?? new IndiceStatisticheAdunanze();
                List<VoceIndiceStatisticheAdunanza> recuperate = new List<VoceIndiceStatisticheAdunanza>();
                bool modificato = false;

                foreach (VoceIndiceStatisticheAdunanza voce in indice.SoloAperteOInterrotte())
                {
                    StatisticheAdunanzaSessione? sessione = CaricaDettaglioDaVoce(voce);
                    if (sessione == null)
                    {
                        continue;
                    }

                    if (sessione.Stato == StatoSessioneStatistiche.Chiusa || sessione.FineUtc.HasValue)
                    {
                        continue;
                    }

                    DateTimeOffset chiusuraStimata = sessione.UltimoAggiornamentoUtc == default
                        ? sessione.InizioUtc
                        : sessione.UltimoAggiornamentoUtc;
                    sessione.FineUtc = chiusuraStimata;
                    sessione.UltimoAggiornamentoUtc = chiusuraStimata;
                    sessione.MotivoChiusura = MotivoChiusuraStatistiche.Crash;
                    sessione.Stato = StatoSessioneStatistiche.Interrotta;
                    sessione.RicalcolaRiepilogo(chiusuraStimata);

                    if (sessione.EAdunanzaNulla())
                    {
                        string percorsoDettaglio = Path.Combine(_statsDirectoryPath, sessione.NomeFileDettaglio);
                        if (File.Exists(percorsoDettaglio))
                        {
                            File.Delete(percorsoDettaglio);
                        }

                        indice.Rimuovi(sessione.SessionId);
                        modificato = true;
                        continue;
                    }

                    AtomicJsonFileStore.Save(Path.Combine(_statsDirectoryPath, sessione.NomeFileDettaglio), sessione, SerializerOptions);
                    VoceIndiceStatisticheAdunanza voceAggiornata = VoceIndiceStatisticheAdunanza.DaSessione(sessione);
                    indice.AggiornaOAggiungi(voceAggiornata);
                    recuperate.Add(voceAggiornata);
                    modificato = true;
                }

                if (modificato)
                {
                    indice.OrdinaPerDataDiscendente();
                    AtomicJsonFileStore.Save(_indexPath, indice, SerializerOptions);
                }

                return recuperate;
            }
        }

        public RisultatiStatisticheStoriche CalcolaStatisticheStoriche()
        {
            lock (SyncRoot)
            {
                List<StatisticheAdunanzaSessione> sessioni = CaricaIndice()
                    .Sessioni
                    .Select(CaricaDettaglioDaVoce)
                    .Where(sessione => sessione != null)
                    .Cast<StatisticheAdunanzaSessione>()
                    .ToList();
                return RisultatiStatisticheStoriche.Calcola(sessioni);
            }
        }

        private StatisticheAdunanzaSessione? CaricaDettaglioDaVoce(VoceIndiceStatisticheAdunanza voce)
        {
            string nomeFile = NormalizzaNomeFileDettaglio(voce.NomeFileDettaglio, voce.InizioUtc);
            return AtomicJsonFileStore.Load<StatisticheAdunanzaSessione>(Path.Combine(_statsDirectoryPath, nomeFile), SerializerOptions);
        }

        private static string ResolveStatsDirectoryPath(string? statsDirectoryPath)
        {
            if (!string.IsNullOrWhiteSpace(statsDirectoryPath))
            {
                return statsDirectoryPath;
            }

            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = Path.GetTempPath();
            }

            return Path.Combine(basePath, "InTempo", "stats");
        }

        private string CreaNomeFileDettaglio(DateTimeOffset inizioUtc)
        {
            string baseName = $"adunanza_{inizioUtc.ToLocalTime():yyyyMMdd_HHmm}.json";
            string candidate = Path.Combine(_statsDirectoryPath, baseName);
            int suffix = 2;

            while (File.Exists(candidate))
            {
                candidate = Path.Combine(_statsDirectoryPath, $"adunanza_{inizioUtc.ToLocalTime():yyyyMMdd_HHmm}_{suffix}.json");
                suffix++;
            }

            return Path.GetFileName(candidate);
        }

        private static string NormalizzaNomeFileDettaglio(string? nomeFile, DateTimeOffset inizioUtc)
        {
            string fallback = $"adunanza_{inizioUtc.ToLocalTime():yyyyMMdd_HHmm}.json";
            if (string.IsNullOrWhiteSpace(nomeFile))
            {
                return fallback;
            }

            string trimmed = nomeFile.Trim();
            if (!trimmed.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                trimmed += ".json";
            }

            string baseName = Path.GetFileName(trimmed);
            if (!string.Equals(trimmed, baseName, StringComparison.Ordinal) || baseName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return fallback;
            }

            return baseName;
        }
    }
}
