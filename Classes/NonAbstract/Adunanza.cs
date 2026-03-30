using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CueTime.Classes.Statistics;
using CueTime.Classes.Utilities;
using CueTime.Classes.Utilities.Impostazioni;

namespace CueTime.Classes.NonAbstract
{
    public class Adunanza : INotifyPropertyChanged
    {
        private static readonly MeetingPartsSnapshotStore SharedSnapshotStore = new MeetingPartsSnapshotStore();
        private readonly ImpostazioniAdunanze _settings;
        private readonly MeetingPartsSnapshotStore _snapshotStore;
        private readonly Finesettimanale _finesettimana = new Finesettimanale();
        private readonly Infrasettimanale _infrasettimanale = new Infrasettimanale();
        private readonly SorveglianteInfrasettimanale _sorveglianteInfrasettimanale = new SorveglianteInfrasettimanale();
        private readonly SorveglianteFinesettimanale _sorveglianteFinesettimanale = new SorveglianteFinesettimanale();
        private ObservableCollection<Parte> _parti = new ObservableCollection<Parte>();
        private int _currentParteIndex;
        private Parte? _current;
        private TimeSpan _tempoResiduo;

        public bool LastLoadIsCurrentWeek { get; private set; } = true;

        public TipoAdunanzaStatistiche TipoAdunanzaCorrente { get; private set; } = TipoAdunanzaStatistiche.Sconosciuta;

        public DateTime DataRiferimentoCorrente { get; private set; } = DateTime.Today;

        public Adunanza() : this(new ImpostazioniAdunanze())
        {
        }

        public Adunanza(ImpostazioniAdunanze settings)
            : this(settings, null)
        {
        }

        internal Adunanza(ImpostazioniAdunanze settings, MeetingPartsSnapshotStore? snapshotStore)
        {
            _settings = settings;
            _snapshotStore = snapshotStore ?? SharedSnapshotStore;
        }

        public ObservableCollection<Parte> Parti
        {
            get => _parti;
            set
            {
                if (!ReferenceEquals(_parti, value))
                {
                    _parti = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public Parte? Current
        {
            get => _current;
            set
            {
                if (ReferenceEquals(_current, value))
                {
                    return;
                }

                if (_current != null)
                {
                    _current.IsCurrent = false;
                }

                _current = value;

                if (_current != null)
                {
                    _current.IsCurrent = true;
                }

                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public TimeSpan TempoResiduo
        {
            get => _tempoResiduo;
            set
            {
                if (_tempoResiduo != value)
                {
                    _tempoResiduo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TempoResiduoString));
                }
            }
        }

        public TimeSpan TempoTotaleRiferimento { get; set; }

        public TimeSpan TempoConsumatoPartiRimosse { get; set; }

        [JsonIgnore]
        public string TempoResiduoString
        {
            get
            {
                TimeSpan ts = TempoResiduo;
                string sign = ts > TimeSpan.Zero ? "-" : ts < TimeSpan.Zero ? "+" : string.Empty;
                ts = ts.Duration();

                int totalMinutes = (int)ts.TotalMinutes;
                int seconds = ts.Seconds;

                return $"{sign}{totalMinutes}:{seconds:00}";
            }
        }

        public async Task SelectedAdunanza(bool preferCacheOnly = false)
        {
            Stopwatch startupTimer = Stopwatch.StartNew();
            DayOfWeek today = DateTime.Now.DayOfWeek;
            DateTime oggi = DateTime.Today;
            DateTime[] dateVisita = _settings.DateVisitaSorvegliante ?? Array.Empty<DateTime>();
            DateTime dataRiferimento = ResolveReferenceDate(oggi, dateVisita);
            bool isSettimanaVisitaSorvegliante = dateVisita
                .Where(ImpostazioniAdunanze.IsDataVisitaValida)
                .Take(2)
                .Any(data => IsInVisitWeek(oggi, data.Date));
            MeetingSnapshotKind snapshotKind = IsWeekendMeeting(today) ? MeetingSnapshotKind.Weekend : MeetingSnapshotKind.Midweek;
            string loadSource = preferCacheOnly ? "cache locale" : "web";

            ObservableCollection<Parte> partiCaricate;

            if (preferCacheOnly
                && _snapshotStore.TryLoadSnapshot(
                    snapshotKind,
                    isSettimanaVisitaSorvegliante,
                    dataRiferimento,
                    out ObservableCollection<Parte> snapshotParts,
                    out bool isCurrentWeekSnapshot))
            {
                partiCaricate = snapshotParts;
                loadSource = isCurrentWeekSnapshot ? "snapshot locale corrente" : "snapshot locale precedente";
                LastLoadIsCurrentWeek = isCurrentWeekSnapshot;
            }
            else
            {
                partiCaricate = await CaricaPartiAsync(today, dataRiferimento, isSettimanaVisitaSorvegliante, preferCacheOnly);
                LastLoadIsCurrentWeek = WebPartsLoader.LastCacheLoadIsCurrentWeek;
            }

            TempoResiduo = TimeSpan.Zero;
            TempoTotaleRiferimento = TimeSpan.Zero;
            TempoConsumatoPartiRimosse = TimeSpan.Zero;
            DataRiferimentoCorrente = dataRiferimento;
            Current = null;
            ReplaceParti(partiCaricate);
            NormalizzaSchemaSorveglianteSeNecessario(today, dataRiferimento, isSettimanaVisitaSorvegliante);
            TipoAdunanzaCorrente = ResolveMeetingType(today, isSettimanaVisitaSorvegliante);

            if (Parti.Count > 0)
            {
                _snapshotStore.SaveSnapshot(snapshotKind, isSettimanaVisitaSorvegliante, dataRiferimento, Parti);
                InizializzaTempoRiferimentoDaPartiCorrenti();
                _currentParteIndex = 0;
                Current = Parti[_currentParteIndex];
            }
            else
            {
                _currentParteIndex = 0;
                Current = null;
            }

            AppLogger.LogInfo($"Adunanza caricata da {loadSource} in {startupTimer.ElapsedMilliseconds} ms con {Parti.Count} parti.");
        }

        private void NormalizzaSchemaSorveglianteSeNecessario(DayOfWeek today, DateTime dataRiferimento, bool isSettimanaVisitaSorvegliante)
        {
            if (!isSettimanaVisitaSorvegliante)
            {
                return;
            }

            if (IsWeekendMeeting(today))
            {
                _sorveglianteFinesettimanale.DataRiferimento = dataRiferimento;
                _sorveglianteFinesettimanale.Parti = Parti;
                _sorveglianteFinesettimanale.ModificaSchemaParti();
                return;
            }

            _sorveglianteInfrasettimanale.Parti = Parti;
            _sorveglianteInfrasettimanale.ModificaSchemaParti();
        }

        private async Task<ObservableCollection<Parte>> CaricaPartiAsync(
            DayOfWeek today,
            DateTime dataRiferimento,
            bool isSettimanaVisitaSorvegliante,
            bool preferCacheOnly)
        {
            if (preferCacheOnly)
            {
                if (today == DayOfWeek.Sunday || today == DayOfWeek.Saturday)
                {
                    if (isSettimanaVisitaSorvegliante)
                    {
                        _sorveglianteFinesettimanale.DataRiferimento = dataRiferimento;
                        _sorveglianteFinesettimanale.CaricaSchemaDaCache();
                        return _sorveglianteFinesettimanale.Parti;
                    }

                    _finesettimana.DataRiferimento = dataRiferimento;
                    _finesettimana.LoadFromCache();
                    return _finesettimana.Parti;
                }

                if (isSettimanaVisitaSorvegliante)
                {
                    _sorveglianteInfrasettimanale.CaricaSchemaDaCache();
                    return _sorveglianteInfrasettimanale.Parti;
                }

                _infrasettimanale.LoadFromCache();
                return _infrasettimanale.Parti;
            }

            if (today == DayOfWeek.Sunday || today == DayOfWeek.Saturday)
            {
                if (isSettimanaVisitaSorvegliante)
                {
                    _sorveglianteFinesettimanale.DataRiferimento = dataRiferimento;
                    await _sorveglianteFinesettimanale.CaricaSchema();
                    return _sorveglianteFinesettimanale.Parti;
                }

                _finesettimana.DataRiferimento = dataRiferimento;
                await _finesettimana.LoadAsync();
                return _finesettimana.Parti;
            }

            if (isSettimanaVisitaSorvegliante)
            {
                await _sorveglianteInfrasettimanale.CaricaSchema();
                return _sorveglianteInfrasettimanale.Parti;
            }

            await _infrasettimanale.LoadAsync();
            return _infrasettimanale.Parti;
        }

        private static DateTime ResolveReferenceDate(DateTime today, DateTime[] visitDates)
        {
            foreach (DateTime visitDate in (visitDates ?? Array.Empty<DateTime>())
                .Where(ImpostazioniAdunanze.IsDataVisitaValida)
                .Select(data => data.Date))
            {
                if (IsInVisitWeek(today, visitDate))
                {
                    return visitDate;
                }
            }

            return today.Date;
        }

        private static bool IsInVisitWeek(DateTime today, DateTime visitStartDate)
        {
            DateTime start = visitStartDate.Date;
            DateTime end = start.AddDays(6);
            return today >= start && today <= end;
        }

        private static bool IsWeekendMeeting(DayOfWeek today)
        {
            return today == DayOfWeek.Saturday || today == DayOfWeek.Sunday;
        }

        private TipoAdunanzaStatistiche ResolveMeetingType(DayOfWeek today, bool isSettimanaVisitaSorvegliante)
        {
            if (Parti.Any(parte => parte.NomeParte.Contains("commemorazione", StringComparison.OrdinalIgnoreCase)))
            {
                return TipoAdunanzaStatistiche.Commemorazione;
            }

            if (isSettimanaVisitaSorvegliante)
            {
                return TipoAdunanzaStatistiche.Sorvegliante;
            }

            return IsWeekendMeeting(today)
                ? TipoAdunanzaStatistiche.Finesettimanale
                : TipoAdunanzaStatistiche.Infrasettimanale;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Avanti()
        {
            if (Parti.Count == 0)
            {
                Current = null;
                _currentParteIndex = 0;
                return;
            }

            int idx = Current != null ? Parti.IndexOf(Current) : -1;
            if (idx < 0)
            {
                _currentParteIndex = 0;
                Current = Parti[0];
                return;
            }

            if (idx >= Parti.Count - 1)
            {
                return;
            }

            _currentParteIndex = idx + 1;
            Current = Parti[_currentParteIndex];
        }

        public void Indietro()
        {
            if (Parti.Count == 0)
            {
                Current = null;
                _currentParteIndex = 0;
                return;
            }

            int idx = Current != null ? Parti.IndexOf(Current) : -1;
            if (idx < 0)
            {
                _currentParteIndex = 0;
                Current = Parti[0];
                return;
            }

            if (idx <= 0)
            {
                return;
            }

            _currentParteIndex = idx - 1;
            Current = Parti[_currentParteIndex];
        }

        public void InizializzaTempoRiferimentoDaPartiCorrenti()
        {
            TempoTotaleRiferimento = CalcolaTempoTotaleParti();
            TempoConsumatoPartiRimosse = TimeSpan.Zero;
        }

        public void NormalizzaTracciamentoResiduo()
        {
            if (TempoTotaleRiferimento <= TimeSpan.Zero && Parti.Count > 0)
            {
                TempoTotaleRiferimento = CalcolaTempoTotaleParti();
            }

            if (TempoConsumatoPartiRimosse < TimeSpan.Zero)
            {
                TempoConsumatoPartiRimosse = TimeSpan.Zero;
            }
        }

        public TimeSpan CalcolaTempoTotaleParti()
        {
            return Parti.Aggregate(TimeSpan.Zero, (totale, parte) => totale + parte.TempoParte);
        }

        private void ReplaceParti(ObservableCollection<Parte>? nuoveParti)
        {
            Parti.Clear();

            if (nuoveParti == null)
            {
                return;
            }

            foreach (Parte parte in nuoveParti)
            {
                Parti.Add(parte);
            }
        }
    }
}

