using System;

namespace InTempo.Classes.Statistics
{
    public enum TipoAdunanzaStatistiche
    {
        Sconosciuta = 0,
        Infrasettimanale = 1,
        Finesettimanale = 2,
        Sorvegliante = 3,
        Commemorazione = 4
    }

    public enum TipoCambioParteStatistiche
    {
        AvvioSessione = 0,
        ManualeAvanti = 1,
        ManualeIndietro = 2,
        Automatico = 3,
        RimozioneParteCorrente = 4,
        Tecnico = 5
    }

    public enum MotivoPausaStatistiche
    {
        Utente = 0,
        AggiuntaParte = 1,
        ModificaParte = 2,
        RimozioneParte = 3,
        Tecnica = 4
    }

    public enum MotivoChiusuraStatistiche
    {
        Completata = 0,
        Interrotta = 1,
        Reset = 2,
        ChiusuraApplicazione = 3,
        Crash = 4
    }

    public enum StatoSessioneStatistiche
    {
        Aperta = 0,
        Chiusa = 1,
        Interrotta = 2
    }

    public sealed class VoceIndiceStatisticheAdunanza
    {
        public string SessionId { get; set; } = string.Empty;

        public string NomeFileDettaglio { get; set; } = string.Empty;

        public TipoAdunanzaStatistiche TipoAdunanza { get; set; } = TipoAdunanzaStatistiche.Sconosciuta;

        public DateTimeOffset InizioUtc { get; set; }

        public DateTimeOffset? FineUtc { get; set; }

        public DateTimeOffset UltimoAggiornamentoUtc { get; set; }

        public StatoSessioneStatistiche Stato { get; set; } = StatoSessioneStatistiche.Aperta;

        public MotivoChiusuraStatistiche? MotivoChiusura { get; set; }

        public DateTime DataRiferimentoAdunanza { get; set; }

        public TimeSpan DurataProgrammatoTotale { get; set; }

        public TimeSpan DurataRealeTotale { get; set; }

        public TimeSpan ScostamentoFinale { get; set; }

        public bool TerminataInOrario { get; set; }

        public bool SessioneCompletaPerScaletta { get; set; }

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

        public static VoceIndiceStatisticheAdunanza DaSessione(StatisticheAdunanzaSessione sessione)
        {
            if (sessione == null)
            {
                throw new ArgumentNullException(nameof(sessione));
            }

            sessione.RicalcolaRiepilogo(sessione.FineUtc ?? sessione.UltimoAggiornamentoUtc);

            return new VoceIndiceStatisticheAdunanza
            {
                SessionId = sessione.SessionId,
                NomeFileDettaglio = sessione.NomeFileDettaglio,
                TipoAdunanza = sessione.TipoAdunanza,
                InizioUtc = sessione.InizioUtc,
                FineUtc = sessione.FineUtc,
                UltimoAggiornamentoUtc = sessione.UltimoAggiornamentoUtc,
                Stato = sessione.Stato,
                MotivoChiusura = sessione.MotivoChiusura,
                DataRiferimentoAdunanza = sessione.DataRiferimentoAdunanza,
                DurataProgrammatoTotale = sessione.DurataProgrammatoTotale,
                DurataRealeTotale = sessione.DurataRealeTotale,
                ScostamentoFinale = sessione.ScostamentoFinale,
                TerminataInOrario = sessione.TerminataInOrario,
                SessioneCompletaPerScaletta = sessione.SessioneCompletaPerScaletta,
                NumeroTotaleParti = sessione.NumeroTotaleParti,
                NumeroPartiInOrario = sessione.NumeroPartiInOrario,
                NumeroPartiFuoriTempo = sessione.NumeroPartiFuoriTempo,
                PercentualePartiInOrario = sessione.PercentualePartiInOrario,
                TempoTotaleSforamenti = sessione.TempoTotaleSforamenti,
                TempoProgrammatoPersoPerPartiRimosse = sessione.TempoProgrammatoPersoPerPartiRimosse,
                NumeroPause = sessione.NumeroPause,
                NumeroRiprese = sessione.NumeroRiprese,
                NumeroCambiManualiAvanti = sessione.NumeroCambiManualiAvanti,
                NumeroCambiManualiIndietro = sessione.NumeroCambiManualiIndietro,
                NumeroCambiAutomatici = sessione.NumeroCambiAutomatici,
                NumeroModificheTotali = sessione.NumeroModificheTotali,
                NumeroPartiEliminate = sessione.NumeroPartiEliminate,
                NumeroPartiAggiunte = sessione.NumeroPartiAggiunte,
                NomeParteConSforamentoMassimo = sessione.NomeParteConSforamentoMassimo,
                SforamentoMassimoParte = sessione.SforamentoMassimoParte
            };
        }
    }
}
