#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

// Assicurati che questi namespace esistano nel tuo progetto
using InTempo.Classes.NonAbstract;

namespace InTempo.Classes.Utilities
{
    public static class WebPartsLoader
    {
        private const string WeeklyRootUrl = @"https://wol.jw.org/it/wol/meetings/r6/lp-i";
        private const string BaseUrl = "https://wol.jw.org";
        private const int SONG_MIN = 5;

        // Propieta' Completed
        public static bool IsLoading { get; private set; } = true;

        // Timeouts
        private const int WeekPageTimeoutMs = 12000;
        private const int DetailPageTimeoutMs = 15000;
        private const int NetworkMaxAttempts = 2;
        private const int NetworkRetryDelayMs = 700;

        // ==========================================================
        // Cache: LocalAppData (ROBUSTO)
        // ==========================================================

        private static readonly string CacheDir = BuildCacheDir();

        private static string BuildCacheDir()
        {
            // In alcuni ambienti può tornare null/empty: evitiamo crash nello static initializer
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(basePath))
                basePath = Path.GetTempPath(); // fallback sicuro (mai null)

            return Path.Combine(basePath, "InTime", "cache");
        }

        // Marker pulizia settimanale (DEVE stare DOPO CacheDir)
        private static readonly string CachePurgeMarkerPath =
            Path.Combine(CacheDir, "_last_weekly_purge.txt");

        // Lock per evitare doppie pulizie in multi-thread
        private static readonly object CachePurgeLock = new object();

        // TTL cache settimanale
        private const int CacheTtlDays = 8;

        // ==========================================================
        // ✅ CONFIG (modificali qui quando vuoi)
        // ==========================================================

        private static readonly string[] MWB_SECTION_TREASURES = { "TESORI DELLA PAROLA DI DIO" };
        private static readonly string[] MWB_SECTION_MINISTRY = { "EFFICACI NEL MINISTERO" };
        private static readonly string[] MWB_SECTION_LIFE = { "VITA CRISTIANA" };

        // Solo questi titoli sono validi dentro “Efficaci nel ministero”
        private static readonly string[] MWB_MINISTRY_ALLOWED_PARTS =
        {
            "Iniziare una conversazione",
            "Coltivare l’interesse",
            "Fare discepoli",
            "Spiegare quello in cui si crede",
            "Discorso"
        };

        private const string TITLE_INTRO_COMMENTS = "Commenti introduttivi";
        private const string TITLE_BIBLE_READING = "Lettura biblica";
        private const string TITLE_GEMS = "Gemme spirituali";
        private const string TITLE_CONG_BIBLE_STUDY = "Studio biblico di congregazione";
        private const string TITLE_FINAL_COMMENTS = "Commenti conclusivi";
        private const string TITLE_COUNSEL = "Consigli";

        // ==========================================================
        // ✅ TIPI (quello che finisce in Parte.Tipo)
        // ==========================================================
        private const string TYPE_CANTICO = "Cantico";
        private const string TYPE_DISCORSO = "Discorso";
        private const string TYPE_LETTURA_BIBLICA = "Lettura biblica";

        // Extra (coerenti, ma opzionali)
        private const string TYPE_COMMENTI = "Commenti";
        private const string TYPE_CONSIGLI = "Consigli";
        private const string TYPE_GEMME = "Gemme spirituali";
        private const string TYPE_STUDIO = "Studio";
        private const string TYPE_MINISTERO = "Ministero";
        private const string TYPE_TESORI = "Tesori";
        private const string TYPE_VITA_CRISTIANA = "Vita cristiana";
        private const string TYPE_PARTE = "Parte";

        // Weekend stock
        private const int WEEKEND_SONG_MIN = 5;
        private const int WEEKEND_TALK_MIN = 30;
        private const int WEEKEND_WT_MIN = 60;

        // Palette custom sezioni principali
        private static readonly System.Windows.Media.Brush ColorSezione1 =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x32, 0xC4, 0xE3)); // #32c4e3
        private static readonly System.Windows.Media.Brush ColorSezione2 =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE3, 0xA2, 0x1D)); // #e3a21d
        private static readonly System.Windows.Media.Brush ColorSezione3 =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE3, 0x4A, 0x2B)); // #e34a2b

        // Palette neutra per parti secondarie
        private static readonly System.Windows.Media.Brush ColorCantico =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x76, 0x76, 0x76)); // #767676
        private static readonly System.Windows.Media.Brush ColorCommenti =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x9A, 0x9A, 0x9A)); // #9a9a9a
        private static readonly System.Windows.Media.Brush ColorConsigli =
            new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x58, 0x58, 0x58)); // #585858

        // ==========================================================
        // Regex minuti
        // ==========================================================

        private static readonly Regex MinutesRegex = new Regex(
            @"(?<!\d)(\d{1,3})\s*(?:min\.?|minuti|minute)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MinutesTokenCleanup = new Regex(
            @"\s*\(?\s*\d{1,3}\s*(?:min\.?|minuti|minute)\s*\)?\s*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private enum MeetingKind { Midweek, Weekend }

        static WebPartsLoader()
        {
            try { Directory.CreateDirectory(CacheDir); } catch { }

            // ✅ Pulizia automatica: 1 volta a settimana (alla prima esecuzione della settimana)
            EnsureWeeklyCachePurge();
        }

        public static string GetCacheFolderPath() => CacheDir;

        public static void ClearCache()
        {
            try
            {
                if (Directory.Exists(CacheDir))
                    Directory.Delete(CacheDir, true);
            }
            catch { }

            try { Directory.CreateDirectory(CacheDir); } catch { }
        }

        // ==========================================================
        // API PUBBLICA
        // ==========================================================

        // ✅ WEEKEND: stock + (opzionale) recupero cantici 2 e 3 dallo Studio WT
        public static async Task<ObservableCollection<Parte>> CaricaFineSettimanaAsync(
            bool preferStudyForWeekend = true,
            bool bypassCache = false)
        {
            // Resetto a true all'inizio dell'operazione
            IsLoading = true;

            var stock = BuildWeekendStock();

            // Se non vuoi usare lo studio WT (o vuoi evitare web), torno stock puro
            if (!preferStudyForWeekend)
            {
                // ✅ FIX: Imposto IsLoading a false anche se esco subito
                IsLoading = false;
                return stock;
            }

            try
            {
                // Prendo SOLO cantico 2 e 3 dallo Studio Torre di Guardia
                // (Questo metodo gestisce internamente la cache: se trova il file, lo usa)
                var (song2, song3) = await ExecuteWithRetryAsync(
                    attempt => CaricaCanticiFineSettimana_2_3_Async(bypassCache || attempt > 1))
                    .ConfigureAwait(false);

                // ✅ FIX: Tipo corretto per i cantici (non "Fine settimana")
                const string tipo = TYPE_CANTICO;

                // Indici: 0 Cantico iniziale, 2 intermezzo, 4 finale
                if (song2.HasValue)
                    stock[2] = new Parte($"Cantico {song2.Value}",
                        TimeSpan.FromMinutes(WEEKEND_SONG_MIN),
                        tipo,
                        ColorCantico,
                        TimeSpan.FromMinutes(WEEKEND_SONG_MIN),
                        null);

                if (song3.HasValue)
                    stock[4] = new Parte($"Cantico {song3.Value}",
                        TimeSpan.FromMinutes(WEEKEND_SONG_MIN),
                        tipo,
                        ColorCantico,
                        TimeSpan.FromMinutes(WEEKEND_SONG_MIN),
                        null);
            }
            catch
            {
                // Fallback: se qualcosa va storto, rimane stock
            }
            finally
            {
                // ✅ FIX: Assicuro che IsLoading diventi false sia in caso di successo (Web/Cache) 
                // sia in caso di errore (Fallback su Stock)
                IsLoading = false;
            }

            return stock;
        }

        public static async Task<ObservableCollection<Parte>> CaricaInfrasettimanaleAsync(
            bool bypassCache = false)
        {
            // Resetto a true all'inizio
            IsLoading = true;

            try
            {
                // GetMeetingArticleAsync controlla la cache. Se il file esiste, lo carica e ritorna subito.
                HtmlNode article = await ExecuteWithRetryAsync(
                    attempt => GetMeetingArticleAsync(MeetingKind.Midweek, bypassCache || attempt > 1, preferStudyForWeekend: true))
                    .ConfigureAwait(false);

                // ✅ FIX: Se siamo qui, abbiamo l'articolo (da web o da cache).
                // Possiamo dire che il caricamento "pesante" è finito.
                IsLoading = false;

                return ParseMidweekFromMwbArticle(article);
            }
            catch
            {
                // In caso di errore critico, assicuriamoci di sbloccare lo stato di caricamento
                IsLoading = false;
                throw; // Rilanciamo l'errore per farlo gestire alla UI se necessario
            }
        }

        // ==========================================================
        // WEEKEND STOCK
        // ==========================================================

        private static ObservableCollection<Parte> BuildWeekendStock()
        {
            var list = new List<Parte>
            {
                new Parte("Cantico (iniziale)",       TimeSpan.FromMinutes(SONG_MIN),         TYPE_CANTICO,  ColorCantico,   TimeSpan.FromMinutes(SONG_MIN),         1),
                new Parte("Discorso pubblico",        TimeSpan.FromMinutes(WEEKEND_TALK_MIN), TYPE_DISCORSO, ColorSezione2,TimeSpan.FromMinutes(WEEKEND_TALK_MIN), 2),
                new Parte("Cantico (intermezzo)",     TimeSpan.FromMinutes(SONG_MIN),         TYPE_CANTICO,  ColorCantico,   TimeSpan.FromMinutes(SONG_MIN),         3),
                new Parte("Studio Torre di Guardia",  TimeSpan.FromMinutes(WEEKEND_WT_MIN),   TYPE_STUDIO,   ColorSezione3,      TimeSpan.FromMinutes(WEEKEND_WT_MIN),   4),
                new Parte("Cantico (finale)",         TimeSpan.FromMinutes(SONG_MIN),         TYPE_CANTICO,  ColorCantico,   TimeSpan.FromMinutes(SONG_MIN),         5)
            };

            return new ObservableCollection<Parte>(list);
        }

        // ==========================================================
        // CORE: carica pagina dettaglio e ritorna <article>
        // ==========================================================

        private static async Task<HtmlNode> GetMeetingArticleAsync(MeetingKind kind, bool bypassCache, bool preferStudyForWeekend)
        {
            var now = DateTime.Now;
            var (year, week) = GetIsoWeek(now);

            string wantedKey = (kind == MeetingKind.Weekend) ? "weekend" : "midweek";
            string detailCache = Path.Combine(CacheDir, $"{wantedKey}-{year}-{week:D2}.html");
            string linksPath = Path.Combine(CacheDir, $"links-{year}-{week:D2}.txt");

            // 1) Cache dettaglio "fresca"
            if (!bypassCache && File.Exists(detailCache))
            {
                var age = now - File.GetLastWriteTime(detailCache);
                if (age.TotalDays <= CacheTtlDays)
                {
                    try
                    {
                        string html = File.ReadAllText(detailCache);
                        var doc = LoadHtml(html);
                        var art = FindArticleNode(doc);
                        if (art != null) return art;
                    }
                    catch { }
                }
            }

            // 2) Link salvato
            string? cachedLink = (!bypassCache) ? TryReadLinkFromFile(linksPath, wantedKey) : null;
            if (!string.IsNullOrWhiteSpace(cachedLink))
            {
                try
                {
                    string absUrl = BuildAbsoluteUrl(cachedLink);
                    string html = await FastGetAsync(absUrl, DetailPageTimeoutMs).ConfigureAwait(false);
                    SafeWriteAllText(detailCache, html);

                    var doc = LoadHtml(html);
                    var art = FindArticleNode(doc);
                    if (art != null) return art;
                }
                catch
                {
                    // Il link in cache può essere scaduto o temporaneamente irraggiungibile:
                    // proseguiamo con il flusso standard da pagina settimanale.
                }
            }

            // 3) Pagina settimanale (direct /year/week → fallback root)
            HtmlDocument weekDoc = await LoadWeekPageDirectFirstAsync(year, week).ConfigureAwait(false);
            if (!IsValidDoc(weekDoc))
                throw new InvalidOperationException("Pagina Adunanze non valida.");

            // 4) Link alle sezioni
            string midHref = FindSectionFirstLink(weekDoc, "Vita e ministero");
            string wkHref = FindWeekendLink(weekDoc, preferStudyForWeekend);

            TryWriteLinks(linksPath, midHref, wkHref);

            string href = (kind == MeetingKind.Weekend) ? wkHref : midHref;
            if (string.IsNullOrWhiteSpace(href))
                throw new InvalidOperationException("Link non trovato per 'Vita e ministero'.");

            // 5) Scarico dettaglio
            string detailUrl = BuildAbsoluteUrl(href);
            string detailHtml = await FastGetAsync(detailUrl, DetailPageTimeoutMs).ConfigureAwait(false);
            SafeWriteAllText(detailCache, detailHtml);

            var detailDoc = LoadHtml(detailHtml);
            var article = FindArticleNode(detailDoc);
            if (article == null)
                throw new InvalidOperationException("Nodo <article> non trovato nella pagina di dettaglio.");

            return article;
        }

        // ==========================================================
        // ✅ PARSING MWB (midweek)
        // ==========================================================

        private static ObservableCollection<Parte> ParseMidweekFromMwbArticle(HtmlNode article)
        {
            var result = new List<Parte>();
            if (article == null) return new ObservableCollection<Parte>();

            string currentSection = "Apertura";
            bool counselMode = false;

            int partNo = 0;           // numerazione progressiva
            int? lastNumbered = null; // ultimo numero assegnato (serve per "Consigli")

            var nodes = article.SelectNodes(".//*[self::h2 or self::h3 or self::p]");
            if (nodes == null) return new ObservableCollection<Parte>();

            foreach (var n in nodes)
            {
                string raw = HtmlEntity.DeEntitize(n.InnerText ?? string.Empty);
                raw = NormalizeSpaces(raw);
                if (string.IsNullOrWhiteSpace(raw)) continue;

                // Cambio sezione
                if (n.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
                {
                    if (MatchesAny(raw, MWB_SECTION_TREASURES)) { currentSection = "Tesori della Parola di Dio"; continue; }
                    if (MatchesAny(raw, MWB_SECTION_MINISTRY)) { currentSection = "Efficaci nel ministero"; continue; }
                    if (MatchesAny(raw, MWB_SECTION_LIFE)) { currentSection = "Vita cristiana"; continue; }
                    continue;
                }

                if (!n.Name.Equals("h3", StringComparison.OrdinalIgnoreCase))
                    continue;

                // --- Apertura: Cantico + Commenti introduttivi nello stesso h3  => NumeroParte = null
                if (raw.IndexOf("Commenti introduttivi", StringComparison.OrdinalIgnoreCase) >= 0
                    && raw.IndexOf("Cantico", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string? song = ExtractCanticoTitle(raw);
                    if (!string.IsNullOrWhiteSpace(song))
                        result.Add(new Parte(song,
                            TimeSpan.FromMinutes(SONG_MIN),
                            TYPE_CANTICO,
                            ColorCantico,
                            TimeSpan.FromMinutes(SONG_MIN),
                            null));

                    int mIntro = FindMinutesForHeading(n);
                    result.Add(new Parte(TITLE_INTRO_COMMENTS,
                        TimeSpan.FromMinutes(mIntro),
                        TYPE_COMMENTI,
                        ColorCommenti,
                        TimeSpan.FromMinutes(mIntro),
                        null));

                    continue;
                }

                // --- Chiusura: Commenti conclusivi + Cantico e preghiera nello stesso h3 => NumeroParte = null
                if (raw.IndexOf("Commenti conclusivi", StringComparison.OrdinalIgnoreCase) >= 0
                    && raw.IndexOf("Cantico", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int mEnd = FindMinutesForHeading(n);
                    result.Add(new Parte(TITLE_FINAL_COMMENTS,
                        TimeSpan.FromMinutes(mEnd),
                        TYPE_COMMENTI,
                        ColorCommenti,
                        TimeSpan.FromMinutes(mEnd),
                        null));

                    string? song = ExtractCanticoTitle(raw);
                    if (!string.IsNullOrWhiteSpace(song))
                        result.Add(new Parte(song,
                            TimeSpan.FromMinutes(SONG_MIN),
                            TYPE_CANTICO,
                            ColorCantico,
                            TimeSpan.FromMinutes(SONG_MIN),
                            null));

                    continue;
                }

                if (raw.StartsWith("Cantico", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("Canto", StringComparison.OrdinalIgnoreCase))
                {
                    string songTitle = ExtractCanticoTitle(raw);

                    result.Add(new Parte(songTitle,
                        TimeSpan.FromMinutes(SONG_MIN),
                        TYPE_CANTICO,
                        ColorCantico,
                        TimeSpan.FromMinutes(SONG_MIN),
                        null));
                    continue;
                }

                string title = RemoveLeadingNumber(raw);

                // Gemme spirituali => NUMERATA
                if (title.Equals(TITLE_GEMS, StringComparison.OrdinalIgnoreCase))
                {
                    int min = FindMinutesForHeading(n);

                    partNo++;
                    lastNumbered = partNo;

                    result.Add(new Parte(TITLE_GEMS,
                        TimeSpan.FromMinutes(min),
                        TYPE_GEMME,
                        ColorSezione1,
                        TimeSpan.FromMinutes(min),
                        partNo));

                    continue;
                }

                // Lettura biblica => NUMERATA + subito "Consigli" con stesso numero
                if (title.StartsWith(TITLE_BIBLE_READING, StringComparison.OrdinalIgnoreCase))
                {
                    int min = FindMinutesForHeading(n);

                    partNo++;
                    lastNumbered = partNo;

                    result.Add(new Parte(TITLE_BIBLE_READING,
                        TimeSpan.FromMinutes(min),
                        TYPE_LETTURA_BIBLICA,
                        ColorSezione1,
                        TimeSpan.FromMinutes(min),
                        partNo));

                    // da qui in poi metto "Consigli" dopo ogni parte studenti
                    counselMode = true;

                    // Consigli => stesso numero (NON incrementa)
                    result.Add(new Parte(TITLE_COUNSEL,
                        TimeSpan.FromMinutes(1),
                        TYPE_CONSIGLI,
                        ColorConsigli,
                        TimeSpan.FromMinutes(1),
                        lastNumbered));

                    continue;
                }

                // Studio biblico di congregazione => NUMERATA
                if (title.Equals(TITLE_CONG_BIBLE_STUDY, StringComparison.OrdinalIgnoreCase))
                {
                    int min = FindMinutesForHeading(n);
                    if (min <= 0) min = 30;

                    partNo++;
                    lastNumbered = partNo;

                    result.Add(new Parte(TITLE_CONG_BIBLE_STUDY,
                        TimeSpan.FromMinutes(min),
                        TYPE_STUDIO,
                        ColorSezione3,
                        TimeSpan.FromMinutes(min),
                        partNo));

                    continue;
                }

                // Efficaci nel ministero: solo whitelist => NUMERATA + Consigli col numero uguale
                if (currentSection.Equals("Efficaci nel ministero", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsAllowedMinistryPart(title))
                    {
                        int min = FindMinutesForHeading(n);

                        partNo++;
                        lastNumbered = partNo;

                        // Tipo: se è "Discorso" -> Discorso, altrimenti Ministero
                        string tipo = ResolveTipoFromTitleAndSection(title, currentSection);

                        result.Add(new Parte(title,
                            TimeSpan.FromMinutes(min),
                            tipo,
                            ColorSezione2,
                            TimeSpan.FromMinutes(min),
                            partNo));

                        if (counselMode)
                        {
                            result.Add(new Parte(TITLE_COUNSEL,
                                TimeSpan.FromMinutes(1),
                                TYPE_CONSIGLI,
                                ColorConsigli,
                                TimeSpan.FromMinutes(1),
                                lastNumbered)); // stesso numero
                        }
                    }
                    continue;
                }

                // Vita cristiana => NUMERATA
                if (currentSection.Equals("Vita cristiana", StringComparison.OrdinalIgnoreCase))
                {
                    int min = FindMinutesForHeading(n);

                    partNo++;
                    lastNumbered = partNo;

                    string tipo = ResolveTipoFromTitleAndSection(title, currentSection);

                    result.Add(new Parte(title,
                        TimeSpan.FromMinutes(min),
                        tipo,
                        ColorSezione3,
                        TimeSpan.FromMinutes(min),
                        partNo));

                    continue;
                }

                // Tesori (tutte le altre parti) => NUMERATA
                if (currentSection.Equals("Tesori della Parola di Dio", StringComparison.OrdinalIgnoreCase))
                {
                    int min = FindMinutesForHeading(n);

                    partNo++;
                    lastNumbered = partNo;

                    string tipo = ResolveTipoFromTitleAndSection(title, currentSection);

                    result.Add(new Parte(title,
                        TimeSpan.FromMinutes(min),
                        tipo,
                        ColorSezione1,
                        TimeSpan.FromMinutes(min),
                        partNo));

                    continue;
                }
            }

            return new ObservableCollection<Parte>(result);
        }

        private static bool IsAllowedMinistryPart(string title)
        {
            foreach (var t in MWB_MINISTRY_ALLOWED_PARTS)
                if (title.Equals(t, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        // ==========================================================
        // ✅ Risoluzione Tipo (Parte.Tipo) -> NO "Fine settimana"/"Apertura"/sezioni
        // ==========================================================
        private static string ResolveTipoFromTitleAndSection(string title, string currentSection)
        {
            if (string.IsNullOrWhiteSpace(title))
                return TYPE_PARTE;

            var t = title.Trim();

            // Cantici
            if (t.StartsWith("Cantico", StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("Canto", StringComparison.OrdinalIgnoreCase))
                return TYPE_CANTICO;

            // Lettura biblica
            if (t.StartsWith(TITLE_BIBLE_READING, StringComparison.OrdinalIgnoreCase))
                return TYPE_LETTURA_BIBLICA;

            // Discorso
            if (t.IndexOf("Discorso", StringComparison.OrdinalIgnoreCase) >= 0)
                return TYPE_DISCORSO;

            // Extra coerenti
            if (t.Equals(TITLE_INTRO_COMMENTS, StringComparison.OrdinalIgnoreCase) ||
                t.Equals(TITLE_FINAL_COMMENTS, StringComparison.OrdinalIgnoreCase))
                return TYPE_COMMENTI;

            if (t.Equals(TITLE_COUNSEL, StringComparison.OrdinalIgnoreCase))
                return TYPE_CONSIGLI;

            if (t.Equals(TITLE_GEMS, StringComparison.OrdinalIgnoreCase))
                return TYPE_GEMME;

            if (t.Equals(TITLE_CONG_BIBLE_STUDY, StringComparison.OrdinalIgnoreCase) ||
                t.StartsWith("Studio", StringComparison.OrdinalIgnoreCase))
                return TYPE_STUDIO;

            // Fallback per sezione (ma NON usiamo il nome sezione come tipo)
            if (currentSection.Equals("Efficaci nel ministero", StringComparison.OrdinalIgnoreCase))
                return TYPE_MINISTERO;

            if (currentSection.Equals("Tesori della Parola di Dio", StringComparison.OrdinalIgnoreCase))
                return TYPE_TESORI;

            if (currentSection.Equals("Vita cristiana", StringComparison.OrdinalIgnoreCase))
                return TYPE_VITA_CRISTIANA;

            return TYPE_PARTE;
        }

        private static int FindMinutesForHeading(HtmlNode heading)
        {
            if (heading == null) return 0;

            // 1) minuti nel titolo
            string raw = HtmlEntity.DeEntitize(heading.InnerText ?? string.Empty);
            raw = NormalizeSpaces(raw);
            int m = ExtractMinutes(raw);
            if (m > 0) return m;

            // 2) minuti nel primo <p> successivo (tipico MWB)
            var p = heading.SelectSingleNode("following::p[1]");
            if (p != null)
            {
                string pr = HtmlEntity.DeEntitize(p.InnerText ?? string.Empty);
                pr = NormalizeSpaces(pr);
                m = ExtractMinutes(pr);
                if (m > 0) return m;
            }

            return 0;
        }

        private static string ExtractReferenceFromNextMinutesParagraph(HtmlNode heading)
        {
            var p = heading?.SelectSingleNode("following::p[1]");
            if (p == null) return string.Empty;

            string pr = HtmlEntity.DeEntitize(p.InnerText ?? string.Empty);
            pr = NormalizeSpaces(pr);

            pr = RemoveMinutesToken(pr);

            int idx = pr.IndexOf("(", StringComparison.Ordinal);
            if (idx > 0) pr = pr.Substring(0, idx).Trim();

            return pr.Trim().Trim('–', '-', '—', ':', ';', '.', ',', ' ');
        }

        private static string ExtractCanticoTitle(string text)
        {
            var m = Regex.Match(text ?? "", @"\b(?:Cantico|Canto)\s+(\d+)\b", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int num))
                return $"Cantico {num}";

            return (text ?? "").Trim();
        }

        private static string RemoveLeadingNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var m = Regex.Match(s, @"^\s*\d+\.\s*(.+)$");
            return m.Success ? m.Groups[1].Value.Trim() : s.Trim();
        }

        private static bool MatchesAny(string text, IEnumerable<string> tokens)
        {
            foreach (var t in tokens)
                if (text.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private static int ExtractMinutes(string text)
        {
            var m = MinutesRegex.Match(text ?? "");
            return (m.Success && int.TryParse(m.Groups[1].Value, out int min)) ? min : 0;
        }

        private static string RemoveMinutesToken(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            string s = MinutesTokenCleanup.Replace(text, " ");
            s = NormalizeSpaces(s);
            return s.Trim('–', '-', '—', ':', ';', '.', ',', ' ').Trim();
        }

        private static string NormalizeSpaces(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            s = s.Replace("\u00A0", " ");
            s = Regex.Replace(s, @"\s+", " ");
            return s.Trim();
        }

        // ==========================================================
        // helper cache/file
        // ==========================================================

        private static bool SafeWriteAllText(string path, string content)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(path, content ?? string.Empty);
                return true;
            }
            catch { return false; }
        }

        private static bool SafeWriteAllLines(string path, string[] lines)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllLines(path, lines ?? Array.Empty<string>());
                return true;
            }
            catch { return false; }
        }

        private static string? TryReadLinkFromFile(string path, string key)
        {
            try
            {
                if (!File.Exists(path)) return null;

                foreach (var line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string k = line.Substring(0, eq).Trim().ToLowerInvariant();
                    string v = line.Substring(eq + 1).Trim();

                    if (k == key && !string.IsNullOrWhiteSpace(v))
                        return v;
                }
            }
            catch { }

            return null;
        }

        private static void TryWriteLinks(string path, string midHref, string wkHref)
        {
            SafeWriteAllLines(path, new[]
            {
                "midweek=" + (midHref ?? string.Empty),
                "weekend=" + (wkHref  ?? string.Empty)
            });
        }

        // ==========================================================
        // HTML helpers
        // ==========================================================

        private static HtmlDocument LoadHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html ?? string.Empty);
            return doc;
        }

        private static HtmlNode? FindArticleNode(HtmlDocument? doc)
        {
            return doc?.DocumentNode?.SelectSingleNode("//article")
                ?? doc?.DocumentNode?.SelectSingleNode("//div[@id='content']")
                ?? doc?.DocumentNode?.SelectSingleNode("//body");
        }

        private static bool IsValidDoc(HtmlDocument? doc)
            => doc?.DocumentNode != null && !string.IsNullOrWhiteSpace(doc.DocumentNode.InnerHtml);

        private static string FindSectionFirstLink(HtmlDocument doc, string sectionHeadingContains)
        {
            var h2Nodes = doc.DocumentNode.SelectNodes("//div[@id='materialNav']//h2");
            if (h2Nodes == null || h2Nodes.Count == 0) return string.Empty;

            foreach (var h2 in h2Nodes)
            {
                string txt = (h2.InnerText ?? string.Empty).Trim();
                if (txt.IndexOf(sectionHeadingContains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var ul = h2.SelectSingleNode("./following-sibling::ul[1]");
                    var a = ul?.SelectSingleNode(".//a[1]");
                    if (a != null) return a.GetAttributeValue("href", string.Empty);
                }
            }
            return string.Empty;
        }

        private static string FindWeekendLink(HtmlDocument doc, bool preferStudy)
        {
            var container = doc.DocumentNode.SelectSingleNode("//div[@id='materialNav']");
            if (container == null) return string.Empty;

            string? study = null;
            string? talk = null;

            var links = container.SelectNodes(".//a");
            if (links != null)
            {
                foreach (var a in links)
                {
                    string at = (a.InnerText ?? "").Trim();

                    if (study == null &&
                        (at.IndexOf("Studio", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         at.IndexOf("Torre di Guardia", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        study = a.GetAttributeValue("href", string.Empty);
                        continue;
                    }

                    if (talk == null && at.IndexOf("Adunanza pubblica", StringComparison.OrdinalIgnoreCase) >= 0)
                        talk = a.GetAttributeValue("href", string.Empty);

                    if (study != null && talk != null) break;
                }
            }

            if (preferStudy)
                return !string.IsNullOrWhiteSpace(study) ? study : (talk ?? string.Empty);
            else
                return !string.IsNullOrWhiteSpace(talk) ? talk : (study ?? string.Empty);
        }

        private static (int year, int week) GetIsoWeek(DateTime dt)
        {
            var cal = CultureInfo.InvariantCulture.Calendar;
            int week = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = dt.Year;

            if (dt.Month == 1 && week >= 52) year -= 1;
            if (dt.Month == 12 && week == 1) year += 1;

            return (year, week);
        }

        private static string BuildWeekUrl(int year, int week) => $"{WeeklyRootUrl}/{year}/{week:D2}";

        private static async Task<HtmlDocument> LoadWeekPageDirectFirstAsync(int year, int week)
        {
            try
            {
                string weekHtml = await FastGetAsync(BuildWeekUrl(year, week), WeekPageTimeoutMs).ConfigureAwait(false);
                var wdoc = LoadHtml(weekHtml);
                if (wdoc.DocumentNode.SelectSingleNode("//div[@id='materialNav']") != null)
                    return wdoc;
            }
            catch { }

            string rootHtml = await FastGetAsync(WeeklyRootUrl, WeekPageTimeoutMs).ConfigureAwait(false);
            return LoadHtml(rootHtml);
        }

        private static async Task<T> ExecuteWithRetryAsync<T>(Func<int, Task<T>> operation)
        {
            Exception? last = null;

            for (int attempt = 1; attempt <= NetworkMaxAttempts; attempt++)
            {
                try
                {
                    return await operation(attempt).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    last = ex;

                    if (attempt >= NetworkMaxAttempts)
                        break;

                    await Task.Delay(NetworkRetryDelayMs).ConfigureAwait(false);
                }
            }

            throw last ?? new InvalidOperationException("Operazione di rete non riuscita.");
        }

        private static string BuildAbsoluteUrl(string href)
        {
            if (string.IsNullOrWhiteSpace(href)) return string.Empty;
            if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return href;
            return BaseUrl + href;
        }

        // ==========================================================
        // ✅ HTTP (Aggressive Staggered Hedging - SILENZIOSO)
        // ==========================================================

        // 1. Windows Chrome (Veloce, standard)
        private static readonly HttpClient ClientWinChrome = CreateHttpClient(useProxy: false,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

        // 2. Mac Safari (Spesso instradato su CDN diverse)
        private static readonly HttpClient ClientMacSafari = CreateHttpClient(useProxy: false,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15");

        // 3. iPhone Mobile (Priorità mobile sui server moderni)
        private static readonly HttpClient ClientMobile = CreateHttpClient(useProxy: false,
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Mobile/15E148 Safari/604.1");

        // 4. Windows Firefox (Motore diverso, utile se ci sono blocchi specifici)
        private static readonly HttpClient ClientWinFirefox = CreateHttpClient(useProxy: false,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0");

        // 5. Proxy di sistema (Ultima spiaggia assoluta)
        private static readonly HttpClient ClientProxy = CreateHttpClient(useProxy: true,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

        private static HttpClient CreateHttpClient(bool useProxy, string userAgent)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseProxy = useProxy,
                Proxy = null, // Usa quello di sistema se useProxy è true
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };

            var client = new HttpClient(handler, disposeHandler: true);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "it-IT,it;q=0.9,en-US;q=0.8,en;q=0.7");

            client.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "max-age=0");

            return client;
        }

        private static async Task<string> FastGetAsync(string url, int timeoutMs)
        {
            using var raceCts = new CancellationTokenSource(timeoutMs);
            Exception? lastException = null;

            async Task<(bool Success, string Content, Exception? Error)> RunFetchStrategy(HttpClient client, int delayStartMs)
            {
                try
                {
                    // Ritardo controllato per scaglionare i tentativi
                    if (delayStartMs > 0)
                    {
                        await Task.Delay(delayStartMs, raceCts.Token).ConfigureAwait(false);
                    }

                    using var req = new HttpRequestMessage(HttpMethod.Get, url);
                    using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, raceCts.Token).ConfigureAwait(false);
                    resp.EnsureSuccessStatusCode();

                    string html = await resp.Content.ReadAsStringAsync(raceCts.Token).ConfigureAwait(false);

                    // Se durante il caricamento un'altra strategia ha già vinto e cancellato, blocchiamo qui per evitare "doppi successi"
                    if (raceCts.IsCancellationRequested)
                    {
                        return (false, string.Empty, new OperationCanceledException());
                    }

                    return (true, html, null);
                }
                catch (Exception ex)
                {
                    return (false, string.Empty, ex);
                }
            }

            var pendingTasks = new List<Task<(bool Success, string Content, Exception? Error)>>
            {
                RunFetchStrategy(ClientWinChrome,   delayStartMs: 0),
                RunFetchStrategy(ClientMacSafari,   delayStartMs: 350),
                RunFetchStrategy(ClientMobile,      delayStartMs: 700),
                RunFetchStrategy(ClientWinFirefox,  delayStartMs: 1200),
                RunFetchStrategy(ClientProxy,       delayStartMs: 2000)
            };

            while (pendingTasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(pendingTasks).ConfigureAwait(false);
                pendingTasks.Remove(completedTask);

                var result = await completedTask.ConfigureAwait(false);

                if (result.Success)
                {
                    // Trovato il primissimo vincitore! Cancelliamo tutti gli altri ancora in volo.
                    if (!raceCts.IsCancellationRequested)
                    {
                        raceCts.Cancel();
                    }

                    return result.Content;
                }
                else
                {
                    // Salviamo l'ultimo errore in caso falliscano tutti. Il while continua.
                    lastException = result.Error;
                }
            }

            throw new HttpRequestException($"Impossibile scaricare '{url}' dopo aver provato 5 strategie concorrenti.", lastException);
        }

        private static (int? Song2, int? Song3) ExtractWeekendSongs_2_3_FromWtStudyHtml(string wtStudyHtml)
        {
            if (string.IsNullOrWhiteSpace(wtStudyHtml))
                return (null, null);

            var doc = new HtmlDocument();
            doc.LoadHtml(wtStudyHtml);

            // Limitiamoci al contenuto dell'articolo per evitare "rumore" (nav, footer, ecc.)
            var article = doc.DocumentNode.SelectSingleNode("//article[@id='article']") ?? doc.DocumentNode;

            // Nei WT study, i cantici stanno tipicamente in <p class="pubRefs"> ... <strong>CANTICO 17</strong> ...
            var strongNodes = article.SelectNodes(
                ".//p[contains(concat(' ', normalize-space(@class), ' '), ' pubRefs ')]//strong"
            );

            var found = new List<int>(capacity: 2);

            if (strongNodes != null)
            {
                foreach (var s in strongNodes)
                {
                    var text = HtmlEntity.DeEntitize(s.InnerText ?? "").Trim();

                    // Cerca "CANTICO 17" (case-insensitive)
                    var m = Regex.Match(text, @"\bCANTICO\s*(\d+)\b", RegexOptions.IgnoreCase);
                    if (!m.Success) continue;

                    if (int.TryParse(m.Groups[1].Value, out var num))
                    {
                        // Evita duplicati consecutivi
                        if (found.Count == 0 || found[^1] != num)
                            found.Add(num);

                        if (found.Count >= 2)
                            break;
                    }
                }
            }

            // Nel WT Study: il primo cantico trovato = cantico 2 (intermezzo), il secondo = cantico 3 (finale)
            int? song2 = found.Count >= 1 ? found[0] : null;
            int? song3 = found.Count >= 2 ? found[1] : null;

            return (song2, song3);
        }

        private static async Task<(int? Song2, int? Song3)> CaricaCanticiFineSettimana_2_3_Async(bool bypassCache)
        {
            var now = DateTime.Now;
            var (year, week) = GetIsoWeek(now);

            // Cache dedicata SOLO per lo Studio WT (weekend)
            string cachePath = Path.Combine(CacheDir, $"weekend-wtstudy-{year}-{week:D2}.html");

            // Cache "fresca"
            if (!bypassCache && File.Exists(cachePath))
            {
                var age = now - File.GetLastWriteTime(cachePath);
                if (age.TotalDays <= CacheTtlDays)
                {
                    try
                    {
                        string html = File.ReadAllText(cachePath);
                        return ExtractWeekendSongs_2_3_FromWtStudyHtml(html);
                    }
                    catch { }
                }
            }

            // Prendo la pagina settimanale e ricavo il link dello Studio Torre di Guardia
            HtmlDocument weekDoc = await LoadWeekPageDirectFirstAsync(year, week).ConfigureAwait(false);
            if (!IsValidDoc(weekDoc))
                return (null, null);

            // Forzo preferStudy=true perché i cantici 2 e 3 stanno nello Studio WT
            string wtHref = FindWeekendLink(weekDoc, preferStudy: true);
            if (string.IsNullOrWhiteSpace(wtHref))
                return (null, null);

            string wtUrl = BuildAbsoluteUrl(wtHref);
            string wtHtml = await FastGetAsync(wtUrl, DetailPageTimeoutMs).ConfigureAwait(false);

            SafeWriteAllText(cachePath, wtHtml);

            return ExtractWeekendSongs_2_3_FromWtStudyHtml(wtHtml);
        }

        // ==========================================================
        // ✅ Pulizia cache: 1 volta a settimana
        // ==========================================================
        private static void EnsureWeeklyCachePurge()
        {
            try
            {
                lock (CachePurgeLock)
                {
                    Directory.CreateDirectory(CacheDir);

                    var (year, week) = GetIsoWeek(DateTime.Now);
                    string token = $"{year}-{week:D2}"; // es: 2026-07

                    string last = "";
                    if (File.Exists(CachePurgeMarkerPath))
                    {
                        try { last = (File.ReadAllText(CachePurgeMarkerPath) ?? "").Trim(); }
                        catch { last = ""; }
                    }

                    // Se è cambiata la settimana, svuota TUTTA la cache
                    if (!string.Equals(last, token, StringComparison.Ordinal))
                    {
                        try
                        {
                            if (Directory.Exists(CacheDir))
                                Directory.Delete(CacheDir, true);
                        }
                        catch { /* ignora */ }

                        try { Directory.CreateDirectory(CacheDir); } catch { /* ignora */ }

                        try { File.WriteAllText(CachePurgeMarkerPath, token); } catch { /* ignora */ }
                    }
                }
            }
            catch
            {
                // Non bloccare mai l'app per la cache
            }
        }
    }
}
