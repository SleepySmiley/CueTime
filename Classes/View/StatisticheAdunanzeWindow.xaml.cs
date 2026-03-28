using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using InTempo.Classes.Statistics;
using MahApps.Metro.IconPacks;

namespace InTempo.Classes.View
{
    public partial class StatisticheAdunanzeWindow : Window, INotifyPropertyChanged
    {
        private const double ChartHeightMax = 68d;
        private const double HorizontalBarMax = 420d;
        private const double RankingBarMax = 340d;

        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private static readonly TipoAdunanzaStatistiche[] SupportedTypes =
        {
            TipoAdunanzaStatistiche.Infrasettimanale,
            TipoAdunanzaStatistiche.Finesettimanale,
            TipoAdunanzaStatistiche.Sorvegliante,
            TipoAdunanzaStatistiche.Commemorazione
        };

        private readonly GestoreStatisticheAdunanze _gestoreStatistiche;
        private SessioneStoricoItemViewModel? _sessioneSelezionata;
        private string _rangeStoricoText = "Nessuna sessione registrata";
        private string _notaFooter = "In attesa del primo salvataggio";
        private string _dettaglioTitolo = "Nessuna sessione selezionata";
        private string _dettaglioSottotitolo = string.Empty;
        private string _dettaglioBadge = "Storico";
        private string _dettaglioMotivoChiusura = "Nessuna chiusura disponibile";
        private string _dettaglioParteCritica = "Nessuna parte critica";
        private string _dettaglioStatoScaletta = "Nessun dettaglio disponibile";

        public StatisticheAdunanzeWindow(GestoreStatisticheAdunanze gestoreStatistiche)
        {
            InitializeComponent();
            _gestoreStatistiche = gestoreStatistiche ?? throw new ArgumentNullException(nameof(gestoreStatistiche));
            DataContext = this;
            RefreshData();
        }

        public ObservableCollection<StatisticaCardItemViewModel> MetrichePanoramica { get; } = new ObservableCollection<StatisticaCardItemViewModel>();

        public ObservableCollection<StatisticaCardItemViewModel> PuntiChiavePanoramica { get; } = new ObservableCollection<StatisticaCardItemViewModel>();

        public ObservableCollection<GraficoAndamentoItemViewModel> AndamentoUltimeAdunanze { get; } = new ObservableCollection<GraficoAndamentoItemViewModel>();

        public ObservableCollection<BarraStatisticheItemViewModel> PercentualiInOrarioPerTipo { get; } = new ObservableCollection<BarraStatisticheItemViewModel>();

        public ObservableCollection<BarraStatisticheItemViewModel> MediaScostamentoPerTipo { get; } = new ObservableCollection<BarraStatisticheItemViewModel>();

        public ObservableCollection<BarraStatisticheItemViewModel> MediaPartiFuoriTempoPerTipo { get; } = new ObservableCollection<BarraStatisticheItemViewModel>();

        public ObservableCollection<BarraStatisticheItemViewModel> Top5PartiPiuSpessoFuoriTempo { get; } = new ObservableCollection<BarraStatisticheItemViewModel>();

        public ObservableCollection<BarraStatisticheItemViewModel> Top5PartiSforamentoMedio { get; } = new ObservableCollection<BarraStatisticheItemViewModel>();

        public ObservableCollection<SessioneStoricoItemViewModel> StoricoSessioni { get; } = new ObservableCollection<SessioneStoricoItemViewModel>();

        public ObservableCollection<StatisticaCardItemViewModel> MetricheSessioneSelezionata { get; } = new ObservableCollection<StatisticaCardItemViewModel>();

        public ObservableCollection<ParteDettaglioItemViewModel> PartiDettaglio { get; } = new ObservableCollection<ParteDettaglioItemViewModel>();

        public ObservableCollection<PausaDettaglioItemViewModel> PauseDettaglio { get; } = new ObservableCollection<PausaDettaglioItemViewModel>();

        public ObservableCollection<CambioParteDettaglioItemViewModel> CambiDettaglio { get; } = new ObservableCollection<CambioParteDettaglioItemViewModel>();

        public ObservableCollection<ModificaParteDettaglioItemViewModel> ModificheDettaglio { get; } = new ObservableCollection<ModificaParteDettaglioItemViewModel>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public SessioneStoricoItemViewModel? SessioneSelezionata
        {
            get => _sessioneSelezionata;
            set
            {
                if (ReferenceEquals(_sessioneSelezionata, value))
                {
                    return;
                }

                _sessioneSelezionata = value;
                OnPropertyChanged();
                AggiornaDettaglioSessione();
                AggiornaVisibilita();
            }
        }

        public string RangeStoricoText
        {
            get => _rangeStoricoText;
            private set
            {
                if (_rangeStoricoText == value)
                {
                    return;
                }

                _rangeStoricoText = value;
                OnPropertyChanged();
            }
        }

        public string NotaFooter
        {
            get => _notaFooter;
            private set
            {
                if (_notaFooter == value)
                {
                    return;
                }

                _notaFooter = value;
                OnPropertyChanged();
            }
        }

        public string PercorsoArchivio => _gestoreStatistiche.PercorsoCartellaStatistiche;

        public string DettaglioTitolo
        {
            get => _dettaglioTitolo;
            private set
            {
                if (_dettaglioTitolo == value)
                {
                    return;
                }

                _dettaglioTitolo = value;
                OnPropertyChanged();
            }
        }

        public string DettaglioSottotitolo
        {
            get => _dettaglioSottotitolo;
            private set
            {
                if (_dettaglioSottotitolo == value)
                {
                    return;
                }

                _dettaglioSottotitolo = value;
                OnPropertyChanged();
            }
        }

        public string DettaglioBadge
        {
            get => _dettaglioBadge;
            private set
            {
                if (_dettaglioBadge == value)
                {
                    return;
                }

                _dettaglioBadge = value;
                OnPropertyChanged();
            }
        }

        public string DettaglioMotivoChiusura
        {
            get => _dettaglioMotivoChiusura;
            private set
            {
                if (_dettaglioMotivoChiusura == value)
                {
                    return;
                }

                _dettaglioMotivoChiusura = value;
                OnPropertyChanged();
            }
        }

        public string DettaglioParteCritica
        {
            get => _dettaglioParteCritica;
            private set
            {
                if (_dettaglioParteCritica == value)
                {
                    return;
                }

                _dettaglioParteCritica = value;
                OnPropertyChanged();
            }
        }

        public string DettaglioStatoScaletta
        {
            get => _dettaglioStatoScaletta;
            private set
            {
                if (_dettaglioStatoScaletta == value)
                {
                    return;
                }

                _dettaglioStatoScaletta = value;
                OnPropertyChanged();
            }
        }

        public bool HasStorico => StoricoSessioni.Count > 0;

        public Visibility ContenutoStoricoVisibility => HasStorico ? Visibility.Visible : Visibility.Collapsed;

        public Visibility StatoVuotoVisibility => HasStorico ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DettaglioSessioneVisibility => SessioneSelezionata != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility DettaglioSessioneVuotoVisibility => SessioneSelezionata != null ? Visibility.Collapsed : Visibility.Visible;

        public Visibility AndamentoGraficoVisibility => AndamentoUltimeAdunanze.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AndamentoGraficoVuotoVisibility => AndamentoUltimeAdunanze.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility TopFrequenzaVisibility => Top5PartiPiuSpessoFuoriTempo.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TopFrequenzaVuotoVisibility => Top5PartiPiuSpessoFuoriTempo.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility TopMedioVisibility => Top5PartiSforamentoMedio.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility TopMedioVuotoVisibility => Top5PartiSforamentoMedio.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        public string LegendaConfronti => "Legenda rapida: in orario = sessione chiusa entro il tempo; scostamento positivo = ritardo, negativo = anticipo; parti fuori tempo = parti concluse sotto zero.";

        public string LegendaStoricoDettaglio => "Nel dettaglio: scostamento = differenza finale rispetto al programmato; sotto zero = tempo totale oltre il previsto; pause/riprese = stop e riavvii del timer.";

        public PackIconMaterialKind IconaPulsanteFinestra => WindowState == WindowState.Maximized
            ? PackIconMaterialKind.WindowRestore
            : PackIconMaterialKind.WindowMaximize;

        public string TestoPulsanteFinestra => WindowState == WindowState.Maximized
            ? "Ripristina"
            : "Ingrandisci";

        public void RefreshData()
        {
            string? selectedSessionId = SessioneSelezionata?.SessionId;
            IReadOnlyList<VoceIndiceStatisticheAdunanza> storico = _gestoreStatistiche.CaricaStoricoLeggero();
            RisultatiStatisticheStoriche risultati = _gestoreStatistiche.CalcolaStatisticheStoriche();

            AggiornaRangeStorico(storico);
            AggiornaMetrichePanoramica(storico, risultati);
            AggiornaPuntiChiave(risultati);
            AggiornaGraficoAndamento(risultati.AndamentoUltime10Adunanze);
            AggiornaConfrontiPerTipo(risultati);
            AggiornaGraduatorieParti(risultati);
            AggiornaStoricoSessioni(storico);

            NotaFooter = HasStorico
                ? $"{StoricoSessioni.Count} sessioni indicizzate"
                : "In attesa del primo salvataggio";

            SessioneSelezionata = StoricoSessioni.FirstOrDefault(item => string.Equals(item.SessionId, selectedSessionId, StringComparison.Ordinal))
                ?? StoricoSessioni.FirstOrDefault();

            AggiornaVisibilita();
        }

        private void AggiornaMetrichePanoramica(IReadOnlyList<VoceIndiceStatisticheAdunanza> storico, RisultatiStatisticheStoriche risultati)
        {
            DateTimeOffset? ultimaSessione = storico.FirstOrDefault()?.InizioUtc;
            ReplaceCollection(
                MetrichePanoramica,
                new[]
                {
                    new StatisticaCardItemViewModel("Sessioni registrate", storico.Count.ToString(ItalianCulture), "Totale adunanze salvate nello storico."),
                    new StatisticaCardItemViewModel("In orario", FormatPercent(risultati.PercentualeAdunanzeFiniteInOrarioSulTotaleStorico), "Quota complessiva di adunanze concluse entro il tempo."),
                    new StatisticaCardItemViewModel("Streak attuale", risultati.StreakCorrenteAdunanzeConsecutiveFiniteInOrario.ToString(ItalianCulture), "Numero di adunanze consecutive concluse in orario."),
                    new StatisticaCardItemViewModel("Media pause", FormatDouble(risultati.MediaNumeroPausePerAdunanza), "Pause medie registrate per ogni adunanza."),
                    new StatisticaCardItemViewModel("Media modifiche", FormatDouble(risultati.MediaNumeroModifichePerAdunanza), "Interventi medi sulla scaletta in corso d'opera."),
                    new StatisticaCardItemViewModel("Ultima adunanza", ultimaSessione.HasValue ? FormatDateTime(ultimaSessione) : "Nessuna", "Ultima sessione presente nell'archivio.")
                });
        }

        private void AggiornaPuntiChiave(RisultatiStatisticheStoriche risultati)
        {
            ReplaceCollection(
                PuntiChiavePanoramica,
                new[]
                {
                    CreaCardMigliorePeggiore("Migliore assoluta", risultati.AdunanzaMiglioreAssoluta, true),
                    CreaCardMigliorePeggiore("Peggiore assoluta", risultati.AdunanzaPeggioreAssoluta, false),
                    new StatisticaCardItemViewModel(
                        "Infra vs fine settimana",
                        $"{FormatSignedDuration(risultati.MediaRitardoAnticipoInfrasettimanale)} / {FormatSignedDuration(risultati.MediaRitardoAnticipoFinesettimanale)}",
                        "Scostamento medio tra infrasettimanale e fine settimana."),
                    new StatisticaCardItemViewModel(
                        "Mese top",
                        FormatMonthKey(risultati.MeseConPiuAdunanzeInOrario),
                        "Mese con più adunanze concluse in orario.")
                });
        }

        private void AggiornaGraficoAndamento(IReadOnlyList<CampioneAndamentoAdunanza> andamento)
        {
            long maxTicks = Math.Max(1L, andamento.Select(item => Math.Abs(item.ScostamentoFinale.Ticks)).DefaultIfEmpty(0L).Max());
            List<GraficoAndamentoItemViewModel> items = andamento
                .Select(item => new GraficoAndamentoItemViewModel(
                    item.InizioUtc.ToLocalTime().ToString("dd MMM", ItalianCulture),
                    item.ScostamentoFinale > TimeSpan.Zero ? Scale(Math.Abs(item.ScostamentoFinale.Ticks), maxTicks, ChartHeightMax) : 0,
                    item.ScostamentoFinale < TimeSpan.Zero ? Scale(Math.Abs(item.ScostamentoFinale.Ticks), maxTicks, ChartHeightMax) : 0,
                    FormatSignedDuration(item.ScostamentoFinale),
                    FormatTipoAdunanza(item.TipoAdunanza)))
                .ToList();

            ReplaceCollection(AndamentoUltimeAdunanze, items);
        }

        private void AggiornaConfrontiPerTipo(RisultatiStatisticheStoriche risultati)
        {
            double maxPartiFuoriTempo = Math.Max(1d, risultati.MediaPartiFuoriTempoPerTipo.Values.DefaultIfEmpty(0d).Max());
            long maxScostamentoTicks = Math.Max(1L, risultati.MediaRitardoAnticipoPerTipo.Values.Select(value => Math.Abs(value.Ticks)).DefaultIfEmpty(0L).Max());

            ReplaceCollection(
                PercentualiInOrarioPerTipo,
                SupportedTypes.Select(tipo =>
                {
                    double valore = risultati.PercentualeAdunanzeFiniteInOrarioPerTipo.TryGetValue(tipo, out double percentuale) ? percentuale : 0d;
                    return new BarraStatisticheItemViewModel(
                        FormatTipoAdunanza(tipo),
                        "Quota di sessioni chiuse entro il tempo previsto.",
                        FormatPercent(valore),
                        Scale(valore, 100d, HorizontalBarMax));
                }));

            ReplaceCollection(
                MediaScostamentoPerTipo,
                SupportedTypes.Select(tipo =>
                {
                    TimeSpan scostamento = risultati.MediaRitardoAnticipoPerTipo.TryGetValue(tipo, out TimeSpan valore) ? valore : TimeSpan.Zero;
                    return new BarraStatisticheItemViewModel(
                        FormatTipoAdunanza(tipo),
                        scostamento > TimeSpan.Zero ? "Tendenza media al ritardo." : "Tendenza media all'anticipo o all'equilibrio.",
                        FormatSignedDuration(scostamento),
                        Scale(Math.Abs(scostamento.Ticks), maxScostamentoTicks, HorizontalBarMax),
                        scostamento > TimeSpan.Zero);
                }));

            ReplaceCollection(
                MediaPartiFuoriTempoPerTipo,
                SupportedTypes.Select(tipo =>
                {
                    double valore = risultati.MediaPartiFuoriTempoPerTipo.TryGetValue(tipo, out double media) ? media : 0d;
                    return new BarraStatisticheItemViewModel(
                        FormatTipoAdunanza(tipo),
                        "Numero medio di parti concluse sotto zero.",
                        $"{FormatDouble(valore)} parti",
                        Scale(valore, maxPartiFuoriTempo, HorizontalBarMax));
                }));
        }

        private void AggiornaGraduatorieParti(RisultatiStatisticheStoriche risultati)
        {
            int maxFrequenza = Math.Max(1, risultati.Top5PartiChePiuSpessoSforano.Select(item => item.NumeroSforamenti).DefaultIfEmpty(0).Max());
            long maxMedioTicks = Math.Max(1L, risultati.Top5PartiConSforamentoMedioPiuAlto.Select(item => Math.Abs(item.TempoMedioSforamento.Ticks)).DefaultIfEmpty(0L).Max());

            ReplaceCollection(
                Top5PartiPiuSpessoFuoriTempo,
                risultati.Top5PartiChePiuSpessoSforano.Select(item => new BarraStatisticheItemViewModel(
                    item.NomeParte,
                    $"Totale sforamento {FormatDuration(item.TempoTotaleSforamento)}",
                    $"{item.NumeroSforamenti} volte",
                    Scale(item.NumeroSforamenti, maxFrequenza, RankingBarMax))));

            ReplaceCollection(
                Top5PartiSforamentoMedio,
                risultati.Top5PartiConSforamentoMedioPiuAlto.Select(item => new BarraStatisticheItemViewModel(
                    item.NomeParte,
                    $"Picco massimo {FormatDuration(item.SforamentoMassimo)}",
                    FormatDuration(item.TempoMedioSforamento),
                    Scale(Math.Abs(item.TempoMedioSforamento.Ticks), maxMedioTicks, RankingBarMax))));
        }

        private void AggiornaStoricoSessioni(IReadOnlyList<VoceIndiceStatisticheAdunanza> storico)
        {
            ReplaceCollection(
                StoricoSessioni,
                storico.Select(item => new SessioneStoricoItemViewModel(
                    item.SessionId,
                    $"{FormatTipoAdunanza(item.TipoAdunanza)} • {item.DataRiferimentoAdunanza:dd MMM yyyy}",
                    $"{FormatDateTime(item.InizioUtc)} • {FormatDateTime(item.FineUtc)}",
                    item.TerminataInOrario ? "In orario" : "Fuori tempo",
                    $"{item.NumeroPartiInOrario}/{item.NumeroTotaleParti} parti in orario",
                    FormatSignedDuration(item.ScostamentoFinale),
                    item.ScostamentoFinale > TimeSpan.Zero ? GetBrush("AppDangerBrush", Brushes.IndianRed) : GetBrush("AppAccentDeepBrush", Brushes.DarkSlateGray))));
        }

        private void AggiornaDettaglioSessione()
        {
            if (SessioneSelezionata == null)
            {
                ReplaceCollection(MetricheSessioneSelezionata, Array.Empty<StatisticaCardItemViewModel>());
                ReplaceCollection(PartiDettaglio, Array.Empty<ParteDettaglioItemViewModel>());
                ReplaceCollection(PauseDettaglio, Array.Empty<PausaDettaglioItemViewModel>());
                ReplaceCollection(CambiDettaglio, Array.Empty<CambioParteDettaglioItemViewModel>());
                ReplaceCollection(ModificheDettaglio, Array.Empty<ModificaParteDettaglioItemViewModel>());
                DettaglioTitolo = "Nessuna sessione selezionata";
                DettaglioSottotitolo = string.Empty;
                DettaglioBadge = "Storico";
                DettaglioMotivoChiusura = "Nessuna chiusura disponibile";
                DettaglioParteCritica = "Nessuna parte critica";
                DettaglioStatoScaletta = "Nessun dettaglio disponibile";
                return;
            }

            StatisticheAdunanzaSessione? sessione = _gestoreStatistiche.CaricaDettaglio(SessioneSelezionata.SessionId);
            if (sessione == null)
            {
                return;
            }

            DettaglioTitolo = $"{FormatTipoAdunanza(sessione.TipoAdunanza)} • {sessione.DataRiferimentoAdunanza:dddd dd MMMM yyyy}";
            DettaglioSottotitolo = $"Inizio reale {FormatDateTime(sessione.InizioUtc)} • Fine reale {FormatDateTime(sessione.FineUtc)}";
            DettaglioBadge = sessione.TerminataInOrario ? "In orario" : "Fuori tempo";
            DettaglioMotivoChiusura = $"Chiusura: {FormatMotivoChiusura(sessione.MotivoChiusura)}";
            DettaglioParteCritica = string.IsNullOrWhiteSpace(sessione.NomeParteConSforamentoMassimo)
                ? "Parte critica: nessuna"
                : $"Parte critica: {sessione.NomeParteConSforamentoMassimo} ({FormatDuration(sessione.SforamentoMassimoParte)})";
            DettaglioStatoScaletta = sessione.SessioneCompletaPerScaletta ? "Scaletta completata" : "Scaletta incompleta";

            ReplaceCollection(
                MetricheSessioneSelezionata,
                new[]
                {
                    new StatisticaCardItemViewModel("Programmato", FormatDuration(sessione.DurataProgrammatoTotale), "Durata prevista all'avvio della sessione."),
                    new StatisticaCardItemViewModel("Reale", FormatDuration(sessione.DurataRealeTotale), "Tempo reale eseguito sulle parti svolte."),
                    new StatisticaCardItemViewModel("Scostamento", FormatSignedDuration(sessione.ScostamentoFinale), "Positivo = ritardo. Negativo = anticipo."),
                    new StatisticaCardItemViewModel("In orario", sessione.NumeroPartiInOrario.ToString(ItalianCulture), "Parti concluse senza andare sotto zero."),
                    new StatisticaCardItemViewModel("Fuori tempo", sessione.NumeroPartiFuoriTempo.ToString(ItalianCulture), "Parti concluse con sforamento."),
                    new StatisticaCardItemViewModel("Sotto zero", FormatDuration(sessione.TempoTotaleSforamenti), "Tempo totale trascorso oltre il previsto."),
                    new StatisticaCardItemViewModel("Pause / riprese", $"{sessione.NumeroPause} / {sessione.NumeroRiprese}", "Quante volte il timer e stato fermato e riavviato."),
                    new StatisticaCardItemViewModel("Cambi manuali", $"{sessione.NumeroCambiManualiAvanti} / {sessione.NumeroCambiManualiIndietro}", "Passaggi manuali avanti / indietro tra le parti."),
                    new StatisticaCardItemViewModel("Modifiche", sessione.NumeroModificheTotali.ToString(ItalianCulture), "Interventi sulla scaletta durante l'adunanza."),
                    new StatisticaCardItemViewModel("Parti +/-", $"{sessione.NumeroPartiAggiunte} / {sessione.NumeroPartiEliminate}", "Parti aggiunte / rimosse in corso d'opera."),
                    new StatisticaCardItemViewModel("Tempo perso", FormatDuration(sessione.TempoProgrammatoPersoPerPartiRimosse), "Tempo programmato perso per parti rimosse.")
                });

            ReplaceCollection(
                PartiDettaglio,
                sessione.Parti
                    .OrderBy(item => item.OrdineEffettivo ?? int.MaxValue)
                    .ThenBy(item => item.OrdineOriginale)
                    .Select(item => new ParteDettaglioItemViewModel(
                        item.OrdineOriginale.ToString(ItalianCulture),
                        item.OrdineEffettivo?.ToString(ItalianCulture) ?? "—",
                        item.NumeroParte?.ToString(ItalianCulture) ?? "—",
                        item.NomeParte,
                        item.TipoParte,
                        FormatDuration(item.DurataPrevistaAllAvvio),
                        FormatDuration(item.DurataPrevistaQuandoParteInizia),
                        FormatDuration(item.DurataRealeEffettiva),
                        FormatSignedDuration(item.DifferenzaPrevistaAllAvvioEReale),
                        FormatSignedDuration(item.DifferenzaPrevistaAllInizioParteEReale),
                        item.EAndataFuoriTempo ? "Sì" : "No",
                        FormatDuration(item.TempoTotaleSottoZero),
                        FormatDateTime(item.TimestampInizioSottoZeroUtc),
                        item.NumeroModifiche.ToString(ItalianCulture),
                        item.EStataSaltata ? "Sì" : "No",
                        item.EStataRimossa ? "Sì" : "No",
                        item.EStataAggiuntaInCorso ? "Sì" : "No",
                        FormatDateTime(item.OraEsattaInizioUtc),
                        FormatDateTime(item.OraEsattaFineUtc))));

            ReplaceCollection(
                PauseDettaglio,
                sessione.Pause.Select(item => new PausaDettaglioItemViewModel(
                    FormatMotivoPausa(item.Motivo),
                    FormatDateTime(item.InizioUtc),
                    FormatDateTime(item.FineUtc),
                    FormatDuration(item.Durata))));

            ReplaceCollection(
                CambiDettaglio,
                sessione.CambiParte.Select(item => new CambioParteDettaglioItemViewModel(
                    FormatDateTime(item.TimestampUtc),
                    FormatTipoCambio(item.Tipo),
                    item.PartePrecedente ?? "—",
                    item.ParteSuccessiva ?? "—",
                    $"{FormatIndice(item.IndicePartePrecedente)} -> {FormatIndice(item.IndiceParteSuccessiva)}")));

            ReplaceCollection(
                ModificheDettaglio,
                sessione.Parti
                    .SelectMany(parte => parte.Modifiche.Select(modifica => new ModificaParteDettaglioItemViewModel(
                        FormatDateTime(modifica.TimestampUtc),
                        parte.NomeParteAllAvvioAdunanza,
                        modifica.ValorePrecedente.NomeParte,
                        modifica.ValoreNuovo.NomeParte,
                        FormatDuration(modifica.ValorePrecedente.TempoParte),
                        FormatDuration(modifica.ValoreNuovo.TempoParte),
                        $"{modifica.ValorePrecedente.OrdineOriginale} -> {modifica.ValoreNuovo.OrdineOriginale}")))
                    .OrderBy(item => item.Ora)
                    .ToList());
        }

        private void AggiornaRangeStorico(IReadOnlyList<VoceIndiceStatisticheAdunanza> storico)
        {
            if (storico.Count == 0)
            {
                RangeStoricoText = "Nessuna sessione registrata";
                return;
            }

            DateTimeOffset newest = storico.First().InizioUtc.ToLocalTime();
            DateTimeOffset oldest = storico.Last().InizioUtc.ToLocalTime();
            RangeStoricoText = $"{oldest:dd MMM yyyy} - {newest:dd MMM yyyy}";
        }

        private void AggiornaVisibilita()
        {
            OnPropertyChanged(nameof(HasStorico));
            OnPropertyChanged(nameof(ContenutoStoricoVisibility));
            OnPropertyChanged(nameof(StatoVuotoVisibility));
            OnPropertyChanged(nameof(DettaglioSessioneVisibility));
            OnPropertyChanged(nameof(DettaglioSessioneVuotoVisibility));
            OnPropertyChanged(nameof(AndamentoGraficoVisibility));
            OnPropertyChanged(nameof(AndamentoGraficoVuotoVisibility));
            OnPropertyChanged(nameof(TopFrequenzaVisibility));
            OnPropertyChanged(nameof(TopFrequenzaVuotoVisibility));
            OnPropertyChanged(nameof(TopMedioVisibility));
            OnPropertyChanged(nameof(TopMedioVuotoVisibility));
            OnPropertyChanged(nameof(PercorsoArchivio));
        }

        private void BtnAggiorna_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void BtnChiudi_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnMassimizzaRipristina_Click(object sender, RoutedEventArgs e)
        {
            AlternaStatoFinestra();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                AlternaStatoFinestra();
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }

            DragMove();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IconaPulsanteFinestra));
            OnPropertyChanged(nameof(TestoPulsanteFinestra));
        }

        private void AlternaStatoFinestra()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            OnPropertyChanged(nameof(IconaPulsanteFinestra));
            OnPropertyChanged(nameof(TestoPulsanteFinestra));
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            OnPropertyChanged(nameof(IconaPulsanteFinestra));
            OnPropertyChanged(nameof(TestoPulsanteFinestra));
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            OnPropertyChanged(nameof(IconaPulsanteFinestra));
            OnPropertyChanged(nameof(TestoPulsanteFinestra));
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (SessioneSelezionata == null && StoricoSessioni.Count > 0)
            {
                SessioneSelezionata = StoricoSessioni[0];
            }
        }

        private void StoricoListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SessioneSelezionata = StoricoListBox.SelectedItem as SessioneStoricoItemViewModel;
        }

        private StatisticaCardItemViewModel CreaCardMigliorePeggiore(string titolo, VoceIndiceStatisticheAdunanza? voce, bool migliore)
        {
            if (voce == null)
            {
                return new StatisticaCardItemViewModel(titolo, "Nessuna", "Nessuna sessione disponibile per questo indicatore.");
            }

            return new StatisticaCardItemViewModel(
                titolo,
                FormatSignedDuration(voce.ScostamentoFinale),
                $"{FormatTipoAdunanza(voce.TipoAdunanza)} • {voce.DataRiferimentoAdunanza:dd MMM yyyy}{(migliore ? " migliore" : " peggiore")}");
        }

        private Brush GetBrush(string key, Brush fallback)
        {
            return TryFindResource(key) as Brush ?? fallback;
        }

        private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
        {
            target.Clear();
            foreach (T item in source)
            {
                target.Add(item);
            }
        }

        private static double Scale(double value, double maxValue, double maxPixels)
        {
            if (maxValue <= 0d || value <= 0d)
            {
                return 0d;
            }

            double scaled = value / maxValue * maxPixels;
            return Math.Max(6d, Math.Min(maxPixels, scaled));
        }

        private static string FormatTipoAdunanza(TipoAdunanzaStatistiche tipo)
        {
            return tipo switch
            {
                TipoAdunanzaStatistiche.Infrasettimanale => "Infrasettimanale",
                TipoAdunanzaStatistiche.Finesettimanale => "Finesettimanale",
                TipoAdunanzaStatistiche.Sorvegliante => "Sorvegliante",
                TipoAdunanzaStatistiche.Commemorazione => "Commemorazione",
                _ => "Sconosciuta"
            };
        }

        private static string FormatTipoCambio(TipoCambioParteStatistiche tipo)
        {
            return tipo switch
            {
                TipoCambioParteStatistiche.AvvioSessione => "Avvio sessione",
                TipoCambioParteStatistiche.ManualeAvanti => "Manuale avanti",
                TipoCambioParteStatistiche.ManualeIndietro => "Manuale indietro",
                TipoCambioParteStatistiche.Automatico => "Automatico",
                TipoCambioParteStatistiche.RimozioneParteCorrente => "Rimozione parte corrente",
                _ => "Tecnico"
            };
        }

        private static string FormatMotivoPausa(MotivoPausaStatistiche motivo)
        {
            return motivo switch
            {
                MotivoPausaStatistiche.AggiuntaParte => "Aggiunta parte",
                MotivoPausaStatistiche.ModificaParte => "Modifica parte",
                MotivoPausaStatistiche.RimozioneParte => "Rimozione parte",
                MotivoPausaStatistiche.Tecnica => "Tecnica",
                _ => "Utente"
            };
        }

        private static string FormatMotivoChiusura(MotivoChiusuraStatistiche? motivo)
        {
            return motivo switch
            {
                MotivoChiusuraStatistiche.Completata => "Completata",
                MotivoChiusuraStatistiche.Interrotta => "Interrotta",
                MotivoChiusuraStatistiche.Reset => "Reset",
                MotivoChiusuraStatistiche.ChiusuraApplicazione => "Chiusura applicazione",
                MotivoChiusuraStatistiche.Crash => "Ripristino post-crash",
                _ => "Non disponibile"
            };
        }

        private static string FormatMonthKey(string? monthKey)
        {
            if (string.IsNullOrWhiteSpace(monthKey))
            {
                return "Nessuno";
            }

            if (DateTime.TryParseExact($"{monthKey}-01", "yyyy-MM-dd", ItalianCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return parsed.ToString("MMMM yyyy", ItalianCulture);
            }

            return monthKey;
        }

        private static string FormatPercent(double value) => $"{value:0.#}%";

        private static string FormatDouble(double value) => value.ToString("0.##", ItalianCulture);

        private static string FormatIndice(int? index) => index.HasValue ? (index.Value + 1).ToString(ItalianCulture) : "—";

        private static string FormatDateTime(DateTimeOffset? value)
        {
            return value.HasValue
                ? value.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss", ItalianCulture)
                : "—";
        }

        private static string FormatDuration(TimeSpan? value) => value.HasValue ? FormatDuration(value.Value) : "—";

        private static string FormatDuration(TimeSpan value)
        {
            TimeSpan absolute = value.Duration();
            int totalHours = (int)Math.Floor(absolute.TotalHours);
            return $"{totalHours:00}:{absolute.Minutes:00}:{absolute.Seconds:00}";
        }

        private static string FormatSignedDuration(TimeSpan? value) => value.HasValue ? FormatSignedDuration(value.Value) : "—";

        private static string FormatSignedDuration(TimeSpan value)
        {
            string sign = value > TimeSpan.Zero ? "+" : value < TimeSpan.Zero ? "-" : string.Empty;
            return $"{sign}{FormatDuration(value)}";
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class StatisticaCardItemViewModel
    {
        public StatisticaCardItemViewModel(string titolo, string valore, string descrizione)
        {
            Titolo = titolo;
            Valore = valore;
            Descrizione = descrizione;
        }

        public string Titolo { get; }
        public string Valore { get; }
        public string Descrizione { get; }
        public string TooltipText => $"{Titolo}: {Descrizione}";
    }

    public sealed class GraficoAndamentoItemViewModel
    {
        public GraficoAndamentoItemViewModel(string etichettaBreve, double altezzaRitardo, double altezzaAnticipo, string scostamentoText, string tipoText)
        {
            EtichettaBreve = etichettaBreve;
            AltezzaRitardo = altezzaRitardo;
            AltezzaAnticipo = altezzaAnticipo;
            ScostamentoText = scostamentoText;
            TipoText = tipoText;
        }

        public string EtichettaBreve { get; }
        public double AltezzaRitardo { get; }
        public double AltezzaAnticipo { get; }
        public string ScostamentoText { get; }
        public string TipoText { get; }
    }

    public sealed class BarraStatisticheItemViewModel
    {
        public BarraStatisticheItemViewModel(string titolo, string descrizione, string valoreText, double larghezzaBarra, bool critico = false)
        {
            Titolo = titolo;
            Descrizione = descrizione;
            ValoreText = valoreText;
            LarghezzaBarra = larghezzaBarra;
            Critico = critico;
        }

        public string Titolo { get; }
        public string Descrizione { get; }
        public string ValoreText { get; }
        public double LarghezzaBarra { get; }
        public bool Critico { get; }
        public string TooltipText => $"{Titolo}: {Descrizione} Valore {ValoreText}.";
    }

    public sealed class SessioneStoricoItemViewModel
    {
        public SessioneStoricoItemViewModel(string sessionId, string titolo, string sottotitolo, string badge, string riepilogoParti, string scostamentoText, Brush scostamentoBrush)
        {
            SessionId = sessionId;
            Titolo = titolo;
            Sottotitolo = sottotitolo;
            Badge = badge;
            RiepilogoParti = riepilogoParti;
            ScostamentoText = scostamentoText;
            ScostamentoBrush = scostamentoBrush;
        }

        public string SessionId { get; }
        public string Titolo { get; }
        public string Sottotitolo { get; }
        public string Badge { get; }
        public string RiepilogoParti { get; }
        public string ScostamentoText { get; }
        public Brush ScostamentoBrush { get; }
        public string TooltipText => $"{Titolo}\n{Sottotitolo}\n{RiepilogoParti}\nScostamento finale {ScostamentoText}";
    }

    public sealed class ParteDettaglioItemViewModel
    {
        public ParteDettaglioItemViewModel(string ordineOriginale, string ordineEffettivo, string numeroParte, string nomeParte, string tipoParte, string durataPrevistaAllAvvio, string durataPrevistaQuandoParteInizia, string durataReale, string deltaAvvio, string deltaInizio, string fuoriTempo, string tempoSottoZero, string inizioSottoZero, string numeroModifiche, string saltata, string rimossa, string aggiunta, string oraInizio, string oraFine)
        {
            OrdineOriginale = ordineOriginale;
            OrdineEffettivo = ordineEffettivo;
            NumeroParte = numeroParte;
            NomeParte = nomeParte;
            TipoParte = tipoParte;
            DurataPrevistaAllAvvio = durataPrevistaAllAvvio;
            DurataPrevistaQuandoParteInizia = durataPrevistaQuandoParteInizia;
            DurataReale = durataReale;
            DeltaAvvio = deltaAvvio;
            DeltaInizio = deltaInizio;
            FuoriTempo = fuoriTempo;
            TempoSottoZero = tempoSottoZero;
            InizioSottoZero = inizioSottoZero;
            NumeroModifiche = numeroModifiche;
            Saltata = saltata;
            Rimossa = rimossa;
            Aggiunta = aggiunta;
            OraInizio = oraInizio;
            OraFine = oraFine;

            OrdiniDisplay = ordineEffettivo == "—" || ordineEffettivo == "-" ? ordineOriginale : $"{ordineOriginale} -> {ordineEffettivo}";
            PrevistaDisplay = durataPrevistaQuandoParteInizia;
            DeltaDisplay = deltaInizio;
            StatoParte = CreaStatoParte(numeroModifiche, saltata, rimossa, aggiunta);
            TooltipText = $"Prevista avvio {durataPrevistaAllAvvio}. Prevista inizio {durataPrevistaQuandoParteInizia}. Delta avvio {deltaAvvio}. Delta inizio {deltaInizio}.";
        }

        public string OrdineOriginale { get; }
        public string OrdineEffettivo { get; }
        public string NumeroParte { get; }
        public string NomeParte { get; }
        public string TipoParte { get; }
        public string DurataPrevistaAllAvvio { get; }
        public string DurataPrevistaQuandoParteInizia { get; }
        public string DurataReale { get; }
        public string DeltaAvvio { get; }
        public string DeltaInizio { get; }
        public string FuoriTempo { get; }
        public string TempoSottoZero { get; }
        public string InizioSottoZero { get; }
        public string NumeroModifiche { get; }
        public string Saltata { get; }
        public string Rimossa { get; }
        public string Aggiunta { get; }
        public string OraInizio { get; }
        public string OraFine { get; }
        public string OrdiniDisplay { get; }
        public string PrevistaDisplay { get; }
        public string DeltaDisplay { get; }
        public string StatoParte { get; }
        public string TooltipText { get; }

        private static string CreaStatoParte(string numeroModifiche, string saltata, string rimossa, string aggiunta)
        {
            List<string> stati = new List<string>();

            if (IsFlagTrue(saltata))
            {
                stati.Add("Saltata");
            }

            if (IsFlagTrue(rimossa))
            {
                stati.Add("Rimossa");
            }

            if (IsFlagTrue(aggiunta))
            {
                stati.Add("Aggiunta");
            }

            if (!string.IsNullOrWhiteSpace(numeroModifiche) && numeroModifiche != "0")
            {
                stati.Add($"Mod. {numeroModifiche}");
            }

            return stati.Count == 0 ? "Regolare" : string.Join(" | ", stati);
        }

        private static bool IsFlagTrue(string value)
        {
            return string.Equals(value, "Si", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Sì", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class PausaDettaglioItemViewModel
    {
        public PausaDettaglioItemViewModel(string motivo, string inizio, string fine, string durata)
        {
            Motivo = motivo;
            Inizio = inizio;
            Fine = fine;
            Durata = durata;
            Intervallo = $"{inizio} -> {fine}";
        }

        public string Motivo { get; }
        public string Inizio { get; }
        public string Fine { get; }
        public string Durata { get; }
        public string Intervallo { get; }
    }

    public sealed class CambioParteDettaglioItemViewModel
    {
        public CambioParteDettaglioItemViewModel(string ora, string tipo, string partePrecedente, string parteSuccessiva, string indici)
        {
            Ora = ora;
            Tipo = tipo;
            PartePrecedente = partePrecedente;
            ParteSuccessiva = parteSuccessiva;
            Indici = indici;
            Passaggio = $"{partePrecedente} -> {parteSuccessiva}";
        }

        public string Ora { get; }
        public string Tipo { get; }
        public string PartePrecedente { get; }
        public string ParteSuccessiva { get; }
        public string Indici { get; }
        public string Passaggio { get; }
    }

    public sealed class ModificaParteDettaglioItemViewModel
    {
        public ModificaParteDettaglioItemViewModel(string ora, string parte, string nomePrecedente, string nomeNuovo, string durataPrecedente, string durataNuova, string ordine)
        {
            Ora = ora;
            Parte = parte;
            NomePrecedente = nomePrecedente;
            NomeNuovo = nomeNuovo;
            DurataPrecedente = durataPrecedente;
            DurataNuova = durataNuova;
            Ordine = ordine;
            NomeCambio = $"{nomePrecedente} -> {nomeNuovo}";
            DurataCambio = $"{durataPrecedente} -> {durataNuova}";
        }

        public string Ora { get; }
        public string Parte { get; }
        public string NomePrecedente { get; }
        public string NomeNuovo { get; }
        public string DurataPrecedente { get; }
        public string DurataNuova { get; }
        public string Ordine { get; }
        public string NomeCambio { get; }
        public string DurataCambio { get; }
    }
}
