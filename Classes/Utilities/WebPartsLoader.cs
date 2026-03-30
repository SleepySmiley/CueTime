#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CueTime.Classes.NonAbstract;

namespace CueTime.Classes.Utilities
{
    public static class WebPartsLoader
    {
        private const int WeekPageTimeoutMs = 12000;
        private const int DetailPageTimeoutMs = 15000;
        private const int MemorialPageTimeoutMs = 750;

        private static readonly WebPartsCache Cache = new WebPartsCache();
        private static readonly WebFetcher Fetcher = new WebFetcher();
        private static readonly HtmlParteParser Parser = new HtmlParteParser();
        private static readonly ParteFactory Factory = new ParteFactory();

        public static bool IsLoading { get; private set; }

        public static bool LastCacheLoadIsCurrentWeek { get; private set; } = true;

        static WebPartsLoader()
        {
            Cache.EnsureWeeklyCachePurge();
        }

        public static string GetCacheFolderPath() => Cache.CacheDirectory;

        public static void ClearCache() => Cache.Clear();

        public static bool HasUsableCachedDataForToday(bool isWeekendMeeting)
        {
            if (TryLoadCachedMemorialSchema(DateTime.Today, out _))
            {
                return true;
            }

            if (Cache.TryReadFreshSnapshot(
                isWeekendMeeting ? WebMeetingKind.Weekend : WebMeetingKind.Midweek,
                DateTime.Now,
                out _))
            {
                return true;
            }

            if (isWeekendMeeting)
            {
                return true;
            }

            return TryGetCachedMeetingArticleHtml(WebMeetingKind.Midweek, out _)
                || Cache.TryReadLatestSnapshot(WebMeetingKind.Midweek, out _)
                || Cache.TryLoadLatestArticleHtml(WebMeetingKind.Midweek, out _);
        }

        public static ObservableCollection<Parte> CaricaFineSettimanaDaCache(bool preferStudyForWeekend = true)
        {
            return CaricaFineSettimanaDaCache(DateTime.Now, preferStudyForWeekend);
        }

        public static ObservableCollection<Parte> CaricaFineSettimanaDaCache(DateTime referenceDate, bool preferStudyForWeekend = true)
        {
            IsLoading = false;
            DateTime normalizedReferenceDate = referenceDate.Date;

            if (TryLoadCachedMemorialSchema(normalizedReferenceDate, out ObservableCollection<Parte>? memorial))
            {
                LastCacheLoadIsCurrentWeek = true;
                PersistCurrentWeekSnapshot(WebMeetingKind.Weekend, normalizedReferenceDate, memorial!);
                return memorial!;
            }

            if (TryGetCachedSnapshot(WebMeetingKind.Weekend, normalizedReferenceDate, out ObservableCollection<Parte> cachedSnapshot))
            {
                LastCacheLoadIsCurrentWeek = true;
                AppLogger.LogInfo("Parti del fine settimana caricate dalla snapshot cache corrente.");
                return cachedSnapshot;
            }

            ObservableCollection<Parte> stock = Factory.BuildWeekendStock();
            if (!preferStudyForWeekend)
            {
                LastCacheLoadIsCurrentWeek = false;
                PersistLatestSnapshot(WebMeetingKind.Weekend, stock);
                return stock;
            }

            if (TryGetCachedWeekendStudyHtml(normalizedReferenceDate, out string cachedStudyHtml))
            {
                WeekendSongSelection songs = Parser.ExtractWeekendSongsFromWtStudyHtml(cachedStudyHtml);
                Factory.ApplyWeekendSongs(stock, songs);
                LastCacheLoadIsCurrentWeek = true;
                PersistCurrentWeekSnapshot(WebMeetingKind.Weekend, normalizedReferenceDate, stock);
                AppLogger.LogInfo("Parti del fine settimana caricate dalla cache locale.");
                return stock;
            }

            if (TryGetLatestSnapshot(WebMeetingKind.Weekend, out ObservableCollection<Parte> latestSnapshot))
            {
                LastCacheLoadIsCurrentWeek = false;
                AppLogger.LogInfo("Parti del fine settimana caricate dall'ultima snapshot disponibile.");
                return latestSnapshot;
            }

            LastCacheLoadIsCurrentWeek = false;
            PersistLatestSnapshot(WebMeetingKind.Weekend, stock);
            AppLogger.LogInfo("Nessuna cache specifica trovata per il fine settimana. Uso lo schema stock immediato.");
            return stock;
        }

        public static ObservableCollection<Parte> CaricaInfrasettimanaleDaCache()
        {
            IsLoading = false;

            if (TryLoadCachedMemorialSchema(DateTime.Today, out ObservableCollection<Parte>? memorial))
            {
                LastCacheLoadIsCurrentWeek = true;
                PersistCurrentWeekSnapshot(WebMeetingKind.Midweek, DateTime.Today, memorial!);
                return memorial!;
            }

            if (TryGetCachedSnapshot(WebMeetingKind.Midweek, DateTime.Today, out ObservableCollection<Parte> cachedSnapshot))
            {
                LastCacheLoadIsCurrentWeek = true;
                AppLogger.LogInfo("Parti dell'infrasettimanale caricate dalla snapshot cache corrente.");
                return cachedSnapshot;
            }

            if (TryGetCachedMeetingArticleHtml(WebMeetingKind.Midweek, out string cachedDetailHtml))
            {
                try
                {
                    ObservableCollection<Parte> parti = ParseMidweekOrThrow(cachedDetailHtml, "articolo infrasettimanale nella cache corrente");
                    LastCacheLoadIsCurrentWeek = true;
                    PersistCurrentWeekSnapshot(WebMeetingKind.Midweek, DateTime.Today, parti);
                    AppLogger.LogInfo("Parti dell'infrasettimanale caricate dalla cache corrente.");
                    return parti;
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning("La cache corrente dell'infrasettimanale non ha prodotto parti utilizzabili.", ex);
                }
            }

            if (TryGetLatestSnapshot(WebMeetingKind.Midweek, out ObservableCollection<Parte> latestSnapshot))
            {
                LastCacheLoadIsCurrentWeek = false;
                AppLogger.LogInfo("Parti dell'infrasettimanale caricate dall'ultima snapshot disponibile.");
                return latestSnapshot;
            }

            if (Cache.TryLoadLatestArticleHtml(WebMeetingKind.Midweek, out string latestCachedHtml))
            {
                try
                {
                    ObservableCollection<Parte> parti = ParseMidweekOrThrow(latestCachedHtml, "ultima cache dell'infrasettimanale");
                    LastCacheLoadIsCurrentWeek = false;
                    PersistLatestSnapshot(WebMeetingKind.Midweek, parti);
                    AppLogger.LogInfo("Parti dell'infrasettimanale caricate dall'ultima cache disponibile.");
                    return parti;
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning("Anche l'ultima cache dell'infrasettimanale non ha prodotto parti utilizzabili.", ex);
                }
            }

            ObservableCollection<Parte> stock = Factory.BuildMidweekStock();
            LastCacheLoadIsCurrentWeek = false;
            PersistLatestSnapshot(WebMeetingKind.Midweek, stock);
            AppLogger.LogWarning("Nessuna cache utilizzabile trovata per l'infrasettimanale. Uso uno schema locale immediato.");
            return stock;
        }

        public static async Task<ObservableCollection<Parte>> CaricaFineSettimanaAsync(
            bool preferStudyForWeekend = true,
            bool bypassCache = false)
        {
            return await CaricaFineSettimanaAsync(DateTime.Now, preferStudyForWeekend, bypassCache).ConfigureAwait(false);
        }

        public static async Task<ObservableCollection<Parte>> CaricaFineSettimanaAsync(
            DateTime referenceDate,
            bool preferStudyForWeekend = true,
            bool bypassCache = false)
        {
            IsLoading = true;
            DateTime normalizedReferenceDate = referenceDate.Date;

            ObservableCollection<Parte> stock = Factory.BuildWeekendStock();

            try
            {
                ObservableCollection<Parte>? memorial = await TryLoadMemorialSchemaAsync(normalizedReferenceDate, bypassCache).ConfigureAwait(false);
                if (memorial != null)
                {
                    LastCacheLoadIsCurrentWeek = true;
                    PersistCurrentWeekSnapshot(WebMeetingKind.Weekend, normalizedReferenceDate, memorial);
                    return memorial;
                }

                if (!preferStudyForWeekend)
                {
                    LastCacheLoadIsCurrentWeek = false;
                    PersistLatestSnapshot(WebMeetingKind.Weekend, stock);
                    return stock;
                }

                WeekendSongSelection songs = await GetWeekendSongsAsync(normalizedReferenceDate, bypassCache).ConfigureAwait(false);
                Factory.ApplyWeekendSongs(stock, songs);
                LastCacheLoadIsCurrentWeek = true;
                PersistCurrentWeekSnapshot(WebMeetingKind.Weekend, normalizedReferenceDate, stock);
                return stock;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("Errore durante il caricamento dei cantici del fine settimana. Verra usato lo schema stock.", ex);
                LastCacheLoadIsCurrentWeek = false;
                PersistLatestSnapshot(WebMeetingKind.Weekend, stock);
                return stock;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public static async Task<ObservableCollection<Parte>> CaricaInfrasettimanaleAsync(bool bypassCache = false)
        {
            IsLoading = true;

            try
            {
                ObservableCollection<Parte>? memorial = await TryLoadMemorialSchemaAsync(DateTime.Today, bypassCache).ConfigureAwait(false);
                if (memorial != null)
                {
                    LastCacheLoadIsCurrentWeek = true;
                    PersistCurrentWeekSnapshot(WebMeetingKind.Midweek, DateTime.Today, memorial);
                    return memorial;
                }

                string detailHtml = await GetMeetingArticleHtmlAsync(WebMeetingKind.Midweek, bypassCache, preferStudyForWeekend: true).ConfigureAwait(false);
                ObservableCollection<Parte> parti = ParseMidweekOrThrow(detailHtml, "articolo infrasettimanale corrente");

                LastCacheLoadIsCurrentWeek = true;
                PersistCurrentWeekSnapshot(WebMeetingKind.Midweek, DateTime.Today, parti);
                return parti;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("Errore durante il caricamento web dell'infrasettimanale corrente. Provo le alternative locali.", ex);

                if (Cache.TryLoadLatestArticleHtml(WebMeetingKind.Midweek, out string cachedHtml))
                {
                    try
                    {
                        ObservableCollection<Parte> parti = ParseMidweekOrThrow(cachedHtml, "ultima cache disponibile dell'infrasettimanale");
                        LastCacheLoadIsCurrentWeek = false;
                        PersistLatestSnapshot(WebMeetingKind.Midweek, parti);
                        return parti;
                    }
                    catch (Exception cacheEx)
                    {
                        AppLogger.LogWarning("Anche l'ultima cache disponibile dell'infrasettimanale non ha prodotto parti utilizzabili.", cacheEx);
                    }
                }

                if (TryGetLatestSnapshot(WebMeetingKind.Midweek, out ObservableCollection<Parte> latestSnapshot))
                {
                    LastCacheLoadIsCurrentWeek = false;
                    AppLogger.LogInfo("Parti dell'infrasettimanale caricate dall'ultima snapshot disponibile.");
                    return latestSnapshot;
                }

                ObservableCollection<Parte> stock = Factory.BuildMidweekStock();
                LastCacheLoadIsCurrentWeek = false;
                PersistLatestSnapshot(WebMeetingKind.Midweek, stock);
                AppLogger.LogWarning("Nessuna sorgente infrasettimanale utilizzabile trovata. Uso uno schema locale immediato.");
                return stock;
            }
            finally
            {
                IsLoading = false;
            }
        }

        internal static bool IsAllowedCachedUrl(string? url)
        {
            return WebFetcher.IsAllowedCachedUrl(url);
        }

        private static async Task<ObservableCollection<Parte>?> TryLoadMemorialSchemaAsync(DateTime referenceDate, bool bypassCache)
        {
            if (!bypassCache && TryLoadCachedMemorialSchema(referenceDate, out ObservableCollection<Parte>? cachedMemorial))
            {
                return cachedMemorial;
            }

            try
            {
                if (await IsOfficialMemorialDateAsync(referenceDate, bypassCache).ConfigureAwait(false))
                {
                    return Factory.BuildMemorialSchema();
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("Impossibile verificare la data ufficiale della commemorazione. Proseguo con il flusso normale.", ex);
            }

            return null;
        }

        private static bool TryLoadCachedMemorialSchema(DateTime referenceDate, out ObservableCollection<Parte>? memorial)
        {
            memorial = null;
            DateTime targetDate = referenceDate.Date;

            if (!TryGetCachedOfficialMemorialDate(targetDate.Year, out DateTime cachedOfficialDate))
            {
                return false;
            }

            if (cachedOfficialDate.Date != targetDate)
            {
                return false;
            }

            memorial = Factory.BuildMemorialSchema();
            return true;
        }

        private static bool TryGetCachedOfficialMemorialDate(int year, out DateTime cachedDate)
        {
            cachedDate = DateTime.MinValue;
            string cachePath = Cache.GetMemorialDateCachePath(year);

            return Cache.TryReadText(cachePath, out string cached)
                && DateTime.TryParseExact(
                    cached.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out cachedDate);
        }

        private static async Task<bool> IsOfficialMemorialDateAsync(DateTime referenceDate, bool bypassCache)
        {
            DateTime targetDate = referenceDate.Date;
            DateTime? officialDate = await GetOfficialMemorialDateAsync(targetDate.Year, bypassCache).ConfigureAwait(false);
            return officialDate.HasValue && officialDate.Value.Date == targetDate;
        }

        private static async Task<DateTime?> GetOfficialMemorialDateAsync(int year, bool bypassCache)
        {
            string cachePath = Cache.GetMemorialDateCachePath(year);

            if (!bypassCache
                && Cache.TryReadText(cachePath, out string cached)
                && DateTime.TryParseExact(
                    cached.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime cachedDate))
            {
                return cachedDate.Date;
            }

            string html = await Fetcher.LoadMemorialPageAsync(MemorialPageTimeoutMs).ConfigureAwait(false);
            DateTime? officialDate = Parser.TryExtractOfficialMemorialDate(html, year);

            if (officialDate.HasValue)
            {
                Cache.TryWriteText(cachePath, officialDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }

            return officialDate;
        }

        private static async Task<WeekendSongSelection> GetWeekendSongsAsync(DateTime referenceDate, bool bypassCache)
        {
            if (!bypassCache && TryGetCachedWeekendStudyHtml(referenceDate, out string cachedHtml))
            {
                return Parser.ExtractWeekendSongsFromWtStudyHtml(cachedHtml);
            }

            DateTime targetDate = referenceDate.Date;
            (int year, int week) = WebPartsCache.GetIsoWeek(targetDate);
            string cachePath = Cache.GetWeekendStudyCachePath(year, week);
            string weekHtml = await Fetcher.LoadWeekPageAsync(year, week, WeekPageTimeoutMs).ConfigureAwait(false);
            MeetingLinks links = Parser.ExtractMeetingLinks(weekHtml);
            string studyHref = links.WeekendStudyHref;
            if (string.IsNullOrWhiteSpace(studyHref))
            {
                return new WeekendSongSelection(null, null);
            }

            if (!Fetcher.TryBuildAllowedUrl(studyHref, out string studyUrl))
            {
                AppLogger.LogWarning($"Il link dello studio settimanale '{studyHref}' non è nella allowlist autorizzata.");
                return new WeekendSongSelection(null, null);
            }

            string wtHtml = await Fetcher.GetStringAsync(studyUrl, DetailPageTimeoutMs).ConfigureAwait(false);
            Cache.TryWriteText(cachePath, wtHtml);
            return Parser.ExtractWeekendSongsFromWtStudyHtml(wtHtml);
        }

        private static async Task<string> GetMeetingArticleHtmlAsync(WebMeetingKind kind, bool bypassCache, bool preferStudyForWeekend)
        {
            if (!bypassCache && TryGetCachedMeetingArticleHtml(kind, out string cachedDetailHtml))
            {
                return cachedDetailHtml;
            }

            DateTime now = DateTime.Now;
            (int year, int week) = WebPartsCache.GetIsoWeek(now);
            string detailCache = Cache.GetDetailCachePath(kind, year, week);
            string linksPath = Cache.GetLinksPath(year, week);
            string wantedKey = kind == WebMeetingKind.Weekend ? "weekend" : "midweek";
            string? cachedLink = !bypassCache ? Cache.TryReadLink(linksPath, wantedKey) : null;
            if (!string.IsNullOrWhiteSpace(cachedLink))
            {
                if (Fetcher.TryBuildAllowedCachedUrl(cachedLink, out string cachedUrl))
                {
                    try
                    {
                        string cachedHtml = await Fetcher.GetStringAsync(cachedUrl, DetailPageTimeoutMs).ConfigureAwait(false);
                        Cache.TryWriteText(detailCache, cachedHtml);

                        if (Parser.TryGetArticleNode(cachedHtml, out _))
                        {
                            return cachedHtml;
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogWarning($"Il link cache '{cachedLink}' non e piu valido o raggiungibile.", ex);
                    }
                }
                else
                {
                    AppLogger.LogWarning($"Il link cache '{cachedLink}' non e nella allowlist host autorizzata e verra ignorato.");
                }
            }

            string weekHtml = await Fetcher.LoadWeekPageAsync(year, week, WeekPageTimeoutMs).ConfigureAwait(false);
            MeetingLinks links = Parser.ExtractMeetingLinks(weekHtml);
            string midHref = links.MidweekHref;
            string weekendHref = preferStudyForWeekend
                ? (!string.IsNullOrWhiteSpace(links.WeekendStudyHref) ? links.WeekendStudyHref : links.WeekendTalkHref)
                : (!string.IsNullOrWhiteSpace(links.WeekendTalkHref) ? links.WeekendTalkHref : links.WeekendStudyHref);

            Cache.TryWriteLinks(linksPath, midHref, weekendHref);

            string href = kind == WebMeetingKind.Weekend ? weekendHref : midHref;
            if (string.IsNullOrWhiteSpace(href))
            {
                throw new InvalidOperationException("Link non trovato per l'adunanza richiesta.");
            }

            if (!Fetcher.TryBuildAllowedUrl(href, out string detailUrl))
            {
                throw new InvalidOperationException("Il link estratto dall'articolo non è nella allowlist autorizzata.");
            }

            string detailHtml = await Fetcher.GetStringAsync(detailUrl, DetailPageTimeoutMs).ConfigureAwait(false);
            Cache.TryWriteText(detailCache, detailHtml);

            if (!Parser.TryGetArticleNode(detailHtml, out _))
            {
                throw new InvalidOperationException("Nodo <article> non trovato nella pagina di dettaglio.");
            }

            return detailHtml;
        }

        private static bool TryGetCachedMeetingArticleHtml(WebMeetingKind kind, out string detailHtml)
        {
            DateTime now = DateTime.Now;
            (int year, int week) = WebPartsCache.GetIsoWeek(now);
            string detailCache = Cache.GetDetailCachePath(kind, year, week);

            return Cache.TryReadFreshText(detailCache, now, out detailHtml)
                && Parser.TryGetArticleNode(detailHtml, out _);
        }

        private static bool TryGetCachedWeekendStudyHtml(DateTime referenceDate, out string cachedHtml)
        {
            DateTime targetDate = referenceDate.Date;
            (int year, int week) = WebPartsCache.GetIsoWeek(targetDate);
            string cachePath = Cache.GetWeekendStudyCachePath(year, week);
            return Cache.TryReadFreshText(cachePath, targetDate, out cachedHtml);
        }

        private static ObservableCollection<Parte> ParseMidweekOrThrow(string html, string source)
        {
            ObservableCollection<Parte> parti = Factory.CreateParti(Parser.ParseMidweekFromHtml(html));
            if (parti.Count == 0)
            {
                throw new InvalidOperationException($"Nessuna parte estratta da {source}.");
            }

            return parti;
        }

        private static bool TryGetCachedSnapshot(WebMeetingKind kind, DateTime referenceDate, out ObservableCollection<Parte> parti)
        {
            if (Cache.TryReadFreshSnapshot(kind, referenceDate.Date, out var snapshot))
            {
                parti = Factory.CreatePartiFromSnapshot(snapshot);
                return parti.Count > 0;
            }

            parti = new ObservableCollection<Parte>();
            return false;
        }

        private static bool TryGetLatestSnapshot(WebMeetingKind kind, out ObservableCollection<Parte> parti)
        {
            if (Cache.TryReadLatestSnapshot(kind, out var snapshot))
            {
                parti = Factory.CreatePartiFromSnapshot(snapshot);
                return parti.Count > 0;
            }

            parti = new ObservableCollection<Parte>();
            return false;
        }

        private static void PersistCurrentWeekSnapshot(WebMeetingKind kind, DateTime referenceDate, ObservableCollection<Parte> parti)
        {
            var snapshot = Factory.CreateSnapshot(parti);
            if (snapshot.Count == 0)
            {
                return;
            }

            (int year, int week) = WebPartsCache.GetIsoWeek(referenceDate.Date);
            Cache.TryWriteSnapshot(kind, year, week, snapshot);
        }

        private static void PersistLatestSnapshot(WebMeetingKind kind, ObservableCollection<Parte> parti)
        {
            var snapshot = Factory.CreateSnapshot(parti);
            if (snapshot.Count == 0)
            {
                return;
            }

            Cache.TryWriteLatestSnapshot(kind, snapshot);
        }
    }
}

