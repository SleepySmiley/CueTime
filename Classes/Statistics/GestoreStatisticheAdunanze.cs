using System;
using System.Collections.Generic;
using System.Linq;
using InTempo.Classes.NonAbstract;

namespace InTempo.Classes.Statistics
{
    public sealed class GestoreStatisticheAdunanze
    {
        private readonly ArchivioStatisticheAdunanze _archivio;
        private readonly TimeProvider _timeProvider;
        private readonly Dictionary<Parte, StatisticheParteAdunanza> _mappaParti = new Dictionary<Parte, StatisticheParteAdunanza>();
        private StatisticheAdunanzaSessione? _sessioneCorrente;
        private Parte? _parteCorrenteAttiva;

        public GestoreStatisticheAdunanze(ArchivioStatisticheAdunanze? archivio = null, TimeProvider? timeProvider = null)
        {
            _archivio = archivio ?? new ArchivioStatisticheAdunanze();
            _timeProvider = timeProvider ?? TimeProvider.System;
            _archivio.RecuperaSessioniInterrotte();
        }

        public bool SessioneAttiva => _sessioneCorrente != null;

        public string PercorsoCartellaStatistiche => _archivio.StatsDirectoryPath;

        public IReadOnlyList<VoceIndiceStatisticheAdunanza> CaricaStoricoLeggero()
        {
            return _archivio.CaricaStoricoLeggero();
        }

        public StatisticheAdunanzaSessione? CaricaDettaglio(string sessionId)
        {
            return _archivio.CaricaDettaglio(sessionId);
        }

        public RisultatiStatisticheStoriche CalcolaStatisticheStoriche()
        {
            return _archivio.CalcolaStatisticheStoriche();
        }

        public void IniziaSessione(Adunanza adunanza)
        {
            if (_sessioneCorrente != null)
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            List<SnapshotParteStatistiche> snapshotParti = adunanza.Parti
                .Select((parte, indice) => SnapshotParteStatistiche.DaParte(parte, indice + 1))
                .ToList();

            _sessioneCorrente = _archivio.CreaSessione(
                adunanza.TipoAdunanzaCorrente,
                adunanza.DataRiferimentoCorrente,
                nowUtc,
                adunanza.TempoTotaleRiferimento > TimeSpan.Zero ? adunanza.TempoTotaleRiferimento : adunanza.CalcolaTempoTotaleParti(),
                snapshotParti);

            _mappaParti.Clear();
            for (int i = 0; i < adunanza.Parti.Count; i++)
            {
                _mappaParti[adunanza.Parti[i]] = _sessioneCorrente.Parti[i];
            }

            if (adunanza.Current != null)
            {
                ApriParteCorrente(adunanza.Current, nowUtc);
                _sessioneCorrente.CambiParte.Add(new EventoCambioParteStatistiche
                {
                    TimestampUtc = nowUtc,
                    Tipo = TipoCambioParteStatistiche.AvvioSessione,
                    ParteSuccessiva = adunanza.Current.NomeParte,
                    IndiceParteSuccessiva = adunanza.Parti.IndexOf(adunanza.Current)
                });
            }

            PersistiSessione();
        }

        public void TerminaSessione(MotivoChiusuraStatistiche motivo)
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            ChiudiParteCorrente(nowUtc);

            EventoPausaStatistiche? pausaAperta = _sessioneCorrente.Pause.LastOrDefault(pausa => !pausa.FineUtc.HasValue);
            if (pausaAperta != null)
            {
                pausaAperta.FineUtc = nowUtc;
            }

            _sessioneCorrente.FineUtc = nowUtc;
            _sessioneCorrente.UltimoAggiornamentoUtc = nowUtc;
            _sessioneCorrente.RicalcolaRiepilogo(nowUtc);
            _sessioneCorrente.MotivoChiusura = DeterminaMotivoChiusura(motivo);
            _sessioneCorrente.Stato = _sessioneCorrente.MotivoChiusura == MotivoChiusuraStatistiche.Crash
                ? StatoSessioneStatistiche.Interrotta
                : StatoSessioneStatistiche.Chiusa;
            _sessioneCorrente.RicalcolaRiepilogo(nowUtc);
            if (_sessioneCorrente.EAdunanzaNulla())
            {
                _archivio.EliminaSessione(_sessioneCorrente);
            }
            else
            {
                PersistiSessione();
            }

            _sessioneCorrente = null;
            _parteCorrenteAttiva = null;
            _mappaParti.Clear();
        }

        public void RegistraPausa(MotivoPausaStatistiche motivo)
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            ChiudiParteCorrente(nowUtc);

            EventoPausaStatistiche? pausaAperta = _sessioneCorrente.Pause.LastOrDefault(pausa => !pausa.FineUtc.HasValue);
            if (pausaAperta == null)
            {
                _sessioneCorrente.Pause.Add(new EventoPausaStatistiche
                {
                    InizioUtc = nowUtc,
                    Motivo = motivo
                });
                _sessioneCorrente.UltimoAggiornamentoUtc = nowUtc;
                PersistiSessione();
            }
        }

        public void RegistraRipresa(Parte? parteCorrente)
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            EventoPausaStatistiche? pausaAperta = _sessioneCorrente.Pause.LastOrDefault(pausa => !pausa.FineUtc.HasValue);
            if (pausaAperta != null)
            {
                pausaAperta.FineUtc = nowUtc;
            }

            if (parteCorrente != null)
            {
                ApriParteCorrente(parteCorrente, nowUtc);
            }

            _sessioneCorrente.UltimoAggiornamentoUtc = nowUtc;
            PersistiSessione();
        }

        public void RegistraCambioParte(
            Parte? partePrecedente,
            Parte? parteSuccessiva,
            TipoCambioParteStatistiche tipoCambio,
            int? indicePartePrecedente,
            int? indiceParteSuccessiva)
        {
            if (_sessioneCorrente == null || ReferenceEquals(partePrecedente, parteSuccessiva))
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            ChiudiParteCorrente(nowUtc);

            _sessioneCorrente.CambiParte.Add(new EventoCambioParteStatistiche
            {
                TimestampUtc = nowUtc,
                Tipo = tipoCambio,
                PartePrecedente = partePrecedente?.NomeParte,
                ParteSuccessiva = parteSuccessiva?.NomeParte,
                IndicePartePrecedente = indicePartePrecedente,
                IndiceParteSuccessiva = indiceParteSuccessiva
            });

            if (parteSuccessiva != null)
            {
                ApriParteCorrente(parteSuccessiva, nowUtc);
            }

            _sessioneCorrente.UltimoAggiornamentoUtc = nowUtc;
            PersistiSessione();
        }

        public void RegistraScorrimentoTimer(
            Parte parteCorrente,
            TimeSpan intervallo,
            TimeSpan tempoPrima,
            TimeSpan tempoDopo,
            DateTimeOffset fineIntervalloUtc)
        {
            if (_sessioneCorrente == null || intervallo <= TimeSpan.Zero)
            {
                return;
            }

            StatisticheParteAdunanza statisticheParte = OttieniORegistraParte(parteCorrente);
            statisticheParte.DurataRealeEffettiva += intervallo;

            if (tempoPrima > TimeSpan.Zero && tempoDopo < TimeSpan.Zero)
            {
                TimeSpan quotaFuoriTempo = tempoDopo.Duration();
                statisticheParte.TempoTotaleSottoZero += quotaFuoriTempo;
                statisticheParte.TimestampInizioSottoZeroUtc ??= fineIntervalloUtc - quotaFuoriTempo;
            }
            else if (tempoPrima <= TimeSpan.Zero && tempoDopo <= TimeSpan.Zero)
            {
                statisticheParte.TempoTotaleSottoZero += intervallo;
                statisticheParte.TimestampInizioSottoZeroUtc ??= fineIntervalloUtc - intervallo;
            }

            _sessioneCorrente.UltimoAggiornamentoUtc = fineIntervalloUtc;
            _sessioneCorrente.RicalcolaRiepilogo(fineIntervalloUtc);
            PersistiSessione();
        }

        public void RegistraAggiuntaParte(Parte parteAggiunta)
        {
            if (_sessioneCorrente == null || _mappaParti.ContainsKey(parteAggiunta))
            {
                return;
            }

            StatisticheParteAdunanza statisticheParte = new StatisticheParteAdunanza
            {
                NomeParte = parteAggiunta.NomeParte,
                NomeParteAllAvvioAdunanza = parteAggiunta.NomeParte,
                TipoParte = parteAggiunta.TipoParte,
                NumeroParte = parteAggiunta.NumeroParte,
                DurataPrevistaAllAvvio = null,
                EStataAggiuntaInCorso = true,
                OrdineOriginale = 0
            };

            _sessioneCorrente.Parti.Add(statisticheParte);
            _mappaParti[parteAggiunta] = statisticheParte;
            _sessioneCorrente.UltimoAggiornamentoUtc = GetNowUtc();
            PersistiSessione();
        }

        public void RegistraRimozioneParte(Parte parteRimossa, bool eraParteCorrente)
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            StatisticheParteAdunanza statisticheParte = OttieniORegistraParte(parteRimossa);

            if (eraParteCorrente)
            {
                ChiudiParteCorrente(nowUtc);
            }

            statisticheParte.EStataRimossa = true;
            statisticheParte.EStataSaltata = !statisticheParte.OraEsattaInizioUtc.HasValue;
            statisticheParte.OraEsattaFineUtc ??= nowUtc;
            _sessioneCorrente.UltimoAggiornamentoUtc = nowUtc;
            PersistiSessione();
        }

        public void RegistraModificaParte(Parte parteModificata, SnapshotParteStatistiche snapshotPrima, SnapshotParteStatistiche snapshotDopo)
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            DateTimeOffset nowUtc = GetNowUtc();
            StatisticheParteAdunanza statisticheParte = OttieniORegistraParte(parteModificata);

            statisticheParte.Modifiche.Add(new ModificaParteStatistiche
            {
                ValorePrecedente = snapshotPrima,
                ValoreNuovo = snapshotDopo,
                TimestampUtc = nowUtc
            });
            statisticheParte.NomeParte = parteModificata.NomeParte;
            statisticheParte.TipoParte = parteModificata.TipoParte;
            statisticheParte.NumeroParte = parteModificata.NumeroParte;
            _sessioneCorrente.UltimoAggiornamentoUtc = nowUtc;
            PersistiSessione();
        }

        public void RegistraResetParte(Parte parteResettata)
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            StatisticheParteAdunanza statisticheParte = OttieniORegistraParte(parteResettata);
            statisticheParte.NomeParte = parteResettata.NomeParte;
            _sessioneCorrente.UltimoAggiornamentoUtc = GetNowUtc();
            PersistiSessione();
        }

        private DateTimeOffset GetNowUtc()
        {
            return _timeProvider.GetUtcNow();
        }

        private StatisticheParteAdunanza OttieniORegistraParte(Parte parte)
        {
            if (_mappaParti.TryGetValue(parte, out StatisticheParteAdunanza? statistica))
            {
                return statistica;
            }

            statistica = new StatisticheParteAdunanza
            {
                NomeParte = parte.NomeParte,
                NomeParteAllAvvioAdunanza = parte.NomeParte,
                TipoParte = parte.TipoParte,
                NumeroParte = parte.NumeroParte,
                DurataPrevistaAllAvvio = parte.TempoParte,
                EStataAggiuntaInCorso = true,
                OrdineOriginale = 0
            };

            _sessioneCorrente?.Parti.Add(statistica);
            _mappaParti[parte] = statistica;
            return statistica;
        }

        private void ApriParteCorrente(Parte parte, DateTimeOffset nowUtc)
        {
            StatisticheParteAdunanza statisticheParte = OttieniORegistraParte(parte);
            statisticheParte.NomeParte = parte.NomeParte;
            statisticheParte.TipoParte = parte.TipoParte;
            statisticheParte.NumeroParte = parte.NumeroParte;
            statisticheParte.OraEsattaInizioUtc ??= nowUtc;
            statisticheParte.DurataPrevistaQuandoParteInizia ??= parte.TempoParte;
            statisticheParte.OrdineEffettivo ??= _sessioneCorrente!.Parti.Count(item => item.OraEsattaInizioUtc.HasValue);

            IntervalloEsecuzioneParteStatistiche? intervalloAperto = statisticheParte.IntervalliEsecuzione.LastOrDefault(intervallo => !intervallo.FineUtc.HasValue);
            if (intervalloAperto == null)
            {
                statisticheParte.IntervalliEsecuzione.Add(new IntervalloEsecuzioneParteStatistiche
                {
                    InizioUtc = nowUtc
                });
            }

            _parteCorrenteAttiva = parte;
            _sessioneCorrente!.UltimoAggiornamentoUtc = nowUtc;
        }

        private void ChiudiParteCorrente(DateTimeOffset nowUtc)
        {
            if (_sessioneCorrente == null || _parteCorrenteAttiva == null || !_mappaParti.TryGetValue(_parteCorrenteAttiva, out StatisticheParteAdunanza? statistica))
            {
                return;
            }

            IntervalloEsecuzioneParteStatistiche? intervalloAperto = statistica.IntervalliEsecuzione.LastOrDefault(intervallo => !intervallo.FineUtc.HasValue);
            if (intervalloAperto != null)
            {
                intervalloAperto.FineUtc = nowUtc;
            }

            statistica.OraEsattaFineUtc = nowUtc;
            statistica.RicalcolaDerivati(nowUtc);
            _parteCorrenteAttiva = null;
        }

        private void PersistiSessione()
        {
            if (_sessioneCorrente == null)
            {
                return;
            }

            _sessioneCorrente.RicalcolaRiepilogo(_sessioneCorrente.FineUtc ?? _sessioneCorrente.UltimoAggiornamentoUtc);
            _archivio.SalvaSessione(_sessioneCorrente);
        }

        private MotivoChiusuraStatistiche DeterminaMotivoChiusura(MotivoChiusuraStatistiche motivoRichiesto)
        {
            if (motivoRichiesto == MotivoChiusuraStatistiche.ChiusuraApplicazione || motivoRichiesto == MotivoChiusuraStatistiche.Crash)
            {
                return motivoRichiesto;
            }

            if (_sessioneCorrente != null && _sessioneCorrente.SessioneCompletaPerScaletta)
            {
                return MotivoChiusuraStatistiche.Completata;
            }

            return motivoRichiesto == MotivoChiusuraStatistiche.Reset
                ? MotivoChiusuraStatistiche.Interrotta
                : motivoRichiesto;
        }
    }
}
