#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using InTempo.Classes.NonAbstract;

namespace InTempo.Classes.Utilities
{
    internal enum MeetingSnapshotKind
    {
        Midweek,
        Weekend
    }

    internal sealed class MeetingPartsSnapshotStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        public MeetingPartsSnapshotStore(string? snapshotDirectory = null)
        {
            SnapshotDirectory = ResolveSnapshotDirectory(snapshotDirectory);

            try
            {
                Directory.CreateDirectory(SnapshotDirectory);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile creare la cartella snapshot '{SnapshotDirectory}'.", ex);
            }
        }

        public string SnapshotDirectory { get; }

        public bool TryLoadSnapshot(
            MeetingSnapshotKind kind,
            bool isVisitWeek,
            DateTime today,
            out ObservableCollection<Parte> parts,
            out bool isCurrentWeek)
        {
            parts = new ObservableCollection<Parte>();
            isCurrentWeek = false;

            (int year, int week) = WebPartsCache.GetIsoWeek(today);
            string currentPath = GetSnapshotPath(kind, isVisitWeek, year, week);

            if (TryReadSnapshot(currentPath, out parts))
            {
                isCurrentWeek = true;
                return true;
            }

            try
            {
                if (!Directory.Exists(SnapshotDirectory))
                {
                    return false;
                }

                string prefix = GetSnapshotFilePrefix(kind, isVisitWeek);
                FileInfo? latest = new DirectoryInfo(SnapshotDirectory)
                    .EnumerateFiles($"{prefix}-*.json")
                    .OrderByDescending(file => file.LastWriteTimeUtc)
                    .FirstOrDefault();

                return latest != null && TryReadSnapshot(latest.FullName, out parts);
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere le snapshot delle parti da '{SnapshotDirectory}'.", ex);
                parts = new ObservableCollection<Parte>();
                return false;
            }
        }

        public bool SaveSnapshot(
            MeetingSnapshotKind kind,
            bool isVisitWeek,
            DateTime today,
            IEnumerable<Parte> parts)
        {
            try
            {
                List<ParteSnapshotRecord> records = parts?
                    .Select(CreateSnapshotRecord)
                    .ToList()
                    ?? new List<ParteSnapshotRecord>();

                if (records.Count == 0)
                {
                    return false;
                }

                (int year, int week) = WebPartsCache.GetIsoWeek(today);
                string path = GetSnapshotPath(kind, isVisitWeek, year, week);
                string? directory = Path.GetDirectoryName(path);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(records, SerializerOptions);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning("Impossibile salvare la snapshot locale delle parti.", ex);
                return false;
            }
        }

        private bool TryReadSnapshot(string path, out ObservableCollection<Parte> parts)
        {
            parts = new ObservableCollection<Parte>();

            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                string json = File.ReadAllText(path);
                List<ParteSnapshotRecord>? records = JsonSerializer.Deserialize<List<ParteSnapshotRecord>>(json, SerializerOptions);

                if (records == null || records.Count == 0)
                {
                    return false;
                }

                parts = new ObservableCollection<Parte>(records.Select(CreateParte));
                return parts.Count > 0;
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile leggere la snapshot delle parti '{path}'.", ex);
                parts = new ObservableCollection<Parte>();
                return false;
            }
        }

        private string GetSnapshotPath(MeetingSnapshotKind kind, bool isVisitWeek, int year, int week)
        {
            string prefix = GetSnapshotFilePrefix(kind, isVisitWeek);
            return Path.Combine(SnapshotDirectory, $"{prefix}-{year}-{week:D2}.json");
        }

        private static string GetSnapshotFilePrefix(MeetingSnapshotKind kind, bool isVisitWeek)
        {
            string basePrefix = kind == MeetingSnapshotKind.Weekend ? "weekend" : "midweek";
            return isVisitWeek ? $"{basePrefix}-visit" : basePrefix;
        }

        private static ParteSnapshotRecord CreateSnapshotRecord(Parte parte)
        {
            return new ParteSnapshotRecord
            {
                NumeroParte = parte.NumeroParte,
                NomeParte = parte.NomeParte,
                TempoParteTicks = parte.TempoParte.Ticks,
                TipoParte = parte.TipoParte,
                ColoreSalvato = parte.ColoreSalvato,
                TempoScorrevoleTicks = parte.TempoScorrevole.Ticks
            };
        }

        private static Parte CreateParte(ParteSnapshotRecord record)
        {
            Parte parte = new Parte
            {
                NumeroParte = record.NumeroParte,
                NomeParte = record.NomeParte ?? string.Empty,
                TempoParte = TimeSpan.FromTicks(record.TempoParteTicks),
                TipoParte = record.TipoParte ?? string.Empty,
                ColoreSalvato = string.IsNullOrWhiteSpace(record.ColoreSalvato) ? "#FF000000" : record.ColoreSalvato,
                TempoScorrevole = TimeSpan.FromTicks(record.TempoScorrevoleTicks),
                IsCurrent = false
            };

            return parte;
        }

        private static string ResolveSnapshotDirectory(string? snapshotDirectory)
        {
            if (!string.IsNullOrWhiteSpace(snapshotDirectory))
            {
                return snapshotDirectory;
            }

            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = Path.GetTempPath();
            }

            return Path.Combine(basePath, "InTempo", "cache", "snapshots");
        }

        private sealed class ParteSnapshotRecord
        {
            public int? NumeroParte { get; set; }

            public string NomeParte { get; set; } = string.Empty;

            public long TempoParteTicks { get; set; }

            public string TipoParte { get; set; } = string.Empty;

            public string ColoreSalvato { get; set; } = "#FF000000";

            public long TempoScorrevoleTicks { get; set; }
        }
    }
}
