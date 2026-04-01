#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace InTempo.Classes.Utilities
{
    internal sealed class WebPartsCache
    {
        private const int CacheTtlDays = 8;
        private readonly object _cachePurgeLock = new object();

        public WebPartsCache()
        {
            CacheDirectory = BuildCacheDir();
            LegacyCacheDirectory = BuildLegacyCacheDir();
            CachePurgeMarkerPath = Path.Combine(CacheDirectory, "_last_weekly_purge.txt");

            try
            {
                Directory.CreateDirectory(CacheDirectory);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile creare la cartella cache '{CacheDirectory}'.", ex);
            }

            TryMigrateLegacyCacheDirectory();
        }

        public string CacheDirectory { get; }

        private string LegacyCacheDirectory { get; }

        private string CachePurgeMarkerPath { get; }

        public string GetDetailCachePath(WebMeetingKind kind, int year, int week)
        {
            string wantedKey = kind == WebMeetingKind.Weekend ? "weekend" : "midweek";
            return Path.Combine(CacheDirectory, $"{wantedKey}-{year}-{week:D2}.html");
        }

        public string GetLinksPath(int year, int week)
        {
            return Path.Combine(CacheDirectory, $"links-{year}-{week:D2}.txt");
        }

        public string GetWeekendStudyCachePath(int year, int week)
        {
            return Path.Combine(CacheDirectory, $"weekend-wtstudy-{year}-{week:D2}.html");
        }

        public string GetMemorialDateCachePath(int year)
        {
            return Path.Combine(CacheDirectory, $"memorial-date-{year}.txt");
        }

        public string GetSnapshotCachePath(WebMeetingKind kind, int year, int week)
        {
            string prefix = kind == WebMeetingKind.Weekend ? "weekend-parts" : "midweek-parts";
            return Path.Combine(CacheDirectory, $"{prefix}-{year}-{week:D2}.json");
        }

        public string GetLatestSnapshotCachePath(WebMeetingKind kind)
        {
            string prefix = kind == WebMeetingKind.Weekend ? "weekend" : "midweek";
            return Path.Combine(CacheDirectory, $"{prefix}-latest.json");
        }

        public void Clear()
        {
            try
            {
                if (Directory.Exists(CacheDirectory))
                {
                    Directory.Delete(CacheDirectory, true);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile eliminare la cartella cache '{CacheDirectory}'.", ex);
            }

            try
            {
                Directory.CreateDirectory(CacheDirectory);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile ricreare la cartella cache '{CacheDirectory}'.", ex);
            }
        }

        public bool TryReadFreshText(string path, DateTime now, out string text)
        {
            text = string.Empty;

            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                TimeSpan age = now - File.GetLastWriteTime(path);
                if (age.TotalDays > CacheTtlDays)
                {
                    return false;
                }

                text = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere la cache fresca '{path}'.", ex);
                text = string.Empty;
                return false;
            }
        }

        public bool TryReadText(string path, out string text)
        {
            text = string.Empty;

            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                text = File.ReadAllText(path);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere il file cache '{path}'.", ex);
                text = string.Empty;
                return false;
            }
        }

        public bool TryWriteText(string path, string content)
        {
            try
            {
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, content ?? string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile scrivere il file cache '{path}'.", ex);
                return false;
            }
        }

        public bool TryWriteLinks(string path, string midHref, string wkHref)
        {
            try
            {
                string? dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllLines(path, new[]
                {
                    "midweek=" + (midHref ?? string.Empty),
                    "weekend=" + (wkHref ?? string.Empty)
                });
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile salvare i link cache in '{path}'.", ex);
                return false;
            }
        }

        public string? TryReadLink(string path, string key)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                foreach (string line in File.ReadAllLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    int eq = line.IndexOf('=');
                    if (eq <= 0)
                    {
                        continue;
                    }

                    string currentKey = line.Substring(0, eq).Trim().ToLowerInvariant();
                    string value = line.Substring(eq + 1).Trim();

                    if (currentKey == key && !string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere i link cache da '{path}'.", ex);
            }

            return null;
        }

        public bool TryLoadLatestArticleHtml(WebMeetingKind kind, out string html)
        {
            html = string.Empty;

            try
            {
                string prefix = kind == WebMeetingKind.Weekend ? "weekend-" : "midweek-";

                if (!Directory.Exists(CacheDirectory))
                {
                    return false;
                }

                FileInfo? candidate = new DirectoryInfo(CacheDirectory)
                    .GetFiles($"{prefix}*.html")
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .FirstOrDefault();

                if (candidate == null)
                {
                    return false;
                }

                html = File.ReadAllText(candidate.FullName);
                return !string.IsNullOrWhiteSpace(html);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile caricare l'ultimo articolo cache per '{kind}'.", ex);
                html = string.Empty;
                return false;
            }
        }

        public bool TryReadFreshSnapshot(WebMeetingKind kind, DateTime now, out IReadOnlyList<ParteSnapshotData> snapshot)
        {
            (int year, int week) = GetIsoWeek(now);
            return TryReadSnapshot(GetSnapshotCachePath(kind, year, week), requireFresh: true, now, out snapshot);
        }

        public bool TryReadLatestSnapshot(WebMeetingKind kind, out IReadOnlyList<ParteSnapshotData> snapshot)
        {
            return TryReadSnapshot(GetLatestSnapshotCachePath(kind), requireFresh: false, DateTime.MinValue, out snapshot);
        }

        public bool TryWriteSnapshot(WebMeetingKind kind, int year, int week, IReadOnlyList<ParteSnapshotData> snapshot)
        {
            if (snapshot.Count == 0)
            {
                return false;
            }

            bool wroteCurrent = TryWriteSnapshotFile(GetSnapshotCachePath(kind, year, week), snapshot);
            bool wroteLatest = TryWriteLatestSnapshot(kind, snapshot);
            return wroteCurrent || wroteLatest;
        }

        public bool TryWriteLatestSnapshot(WebMeetingKind kind, IReadOnlyList<ParteSnapshotData> snapshot)
        {
            if (snapshot.Count == 0)
            {
                return false;
            }

            return TryWriteSnapshotFile(GetLatestSnapshotCachePath(kind), snapshot);
        }

        public void EnsureWeeklyCachePurge()
        {
            try
            {
                lock (_cachePurgeLock)
                {
                    Directory.CreateDirectory(CacheDirectory);

                    (int year, int week) = GetIsoWeek(DateTime.Now);
                    string token = $"{year}-{week:D2}";
                    string last = string.Empty;

                    if (File.Exists(CachePurgeMarkerPath))
                    {
                        try
                        {
                            last = (File.ReadAllText(CachePurgeMarkerPath) ?? string.Empty).Trim();
                        }
                        catch (Exception ex)
                        {
                            AppLogger.LogWarning($"Impossibile leggere il marker cache '{CachePurgeMarkerPath}'.", ex);
                        }
                    }

                    if (string.Equals(last, token, StringComparison.Ordinal))
                    {
                        return;
                    }

                    DeleteStaleCacheFiles(year, week);

                    try
                    {
                        File.WriteAllText(CachePurgeMarkerPath, token);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogWarning($"Impossibile aggiornare il marker cache '{CachePurgeMarkerPath}'.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("Errore durante la pulizia settimanale della cache.", ex);
            }
        }

        private void TryMigrateLegacyCacheDirectory()
        {
            try
            {
                if (!Directory.Exists(LegacyCacheDirectory) || string.Equals(LegacyCacheDirectory, CacheDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                Directory.CreateDirectory(CacheDirectory);

                bool copiedAnyFile = false;
                foreach (string legacyFilePath in Directory.GetFiles(LegacyCacheDirectory))
                {
                    string fileName = Path.GetFileName(legacyFilePath);
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        continue;
                    }

                    string targetPath = Path.Combine(CacheDirectory, fileName);
                    if (File.Exists(targetPath))
                    {
                        continue;
                    }

                    File.Copy(legacyFilePath, targetPath, overwrite: false);
                    copiedAnyFile = true;
                }

                if (copiedAnyFile)
                {
                    AppLogger.LogInfo($"Cache legacy migrata da '{LegacyCacheDirectory}' a '{CacheDirectory}'.");
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile migrare la cache legacy da '{LegacyCacheDirectory}' a '{CacheDirectory}'.", ex);
            }
        }

        private void DeleteStaleCacheFiles(int currentYear, int currentWeek)
        {
            try
            {
                if (!Directory.Exists(CacheDirectory))
                {
                    return;
                }

                HashSet<string> filesToKeep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    Path.GetFileName(CachePurgeMarkerPath),
                    $"links-{currentYear}-{currentWeek:D2}.txt",
                    $"midweek-{currentYear}-{currentWeek:D2}.html",
                    $"midweek-parts-{currentYear}-{currentWeek:D2}.json",
                    "midweek-latest.json",
                    $"weekend-{currentYear}-{currentWeek:D2}.html",
                    $"weekend-parts-{currentYear}-{currentWeek:D2}.json",
                    "weekend-latest.json",
                    $"weekend-wtstudy-{currentYear}-{currentWeek:D2}.html",
                    $"memorial-date-{currentYear}.txt"
                };

                foreach (string filePath in Directory.GetFiles(CacheDirectory))
                {
                    string fileName = Path.GetFileName(filePath);
                    if (filesToKeep.Contains(fileName))
                    {
                        continue;
                    }

                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogWarning($"Impossibile eliminare il file cache obsoleto '{filePath}'.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile pulire selettivamente la cache '{CacheDirectory}'.", ex);
            }
        }

        internal static (int year, int week) GetIsoWeek(DateTime dt)
        {
            Calendar cal = CultureInfo.InvariantCulture.Calendar;
            int week = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int year = dt.Year;

            if (dt.Month == 1 && week >= 52)
            {
                year -= 1;
            }

            if (dt.Month == 12 && week == 1)
            {
                year += 1;
            }

            return (year, week);
        }

        private static string BuildCacheDir()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = Path.GetTempPath();
            }

            return Path.Combine(basePath, "InTempo", "cache");
        }

        private static string BuildLegacyCacheDir()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = Path.GetTempPath();
            }

            return Path.Combine(basePath, "InTime", "cache");
        }

        private bool TryReadSnapshot(string path, bool requireFresh, DateTime now, out IReadOnlyList<ParteSnapshotData> snapshot)
        {
            snapshot = Array.Empty<ParteSnapshotData>();

            try
            {
                string raw;
                bool loaded = requireFresh
                    ? TryReadFreshText(path, now, out raw)
                    : TryReadText(path, out raw);

                if (!loaded || string.IsNullOrWhiteSpace(raw))
                {
                    return false;
                }

                ParteSnapshotData[]? parsed = JsonSerializer.Deserialize<ParteSnapshotData[]>(raw);
                if (parsed == null || parsed.Length == 0)
                {
                    return false;
                }

                snapshot = parsed;
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere la snapshot cache '{path}'.", ex);
                snapshot = Array.Empty<ParteSnapshotData>();
                return false;
            }
        }

        private bool TryWriteSnapshotFile(string path, IReadOnlyList<ParteSnapshotData> snapshot)
        {
            try
            {
                string json = JsonSerializer.Serialize(snapshot);
                return TryWriteText(path, json);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile scrivere la snapshot cache '{path}'.", ex);
                return false;
            }
        }
    }
}
