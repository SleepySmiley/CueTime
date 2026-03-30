#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using CueTime.Classes.NonAbstract;

namespace CueTime.Classes.Utilities
{
    internal sealed class ParteFactory
    {
        private const int SongMinutes = 5;
        private const int MemorialTalkMinutes = 45;
        private const int WeekendTalkMinutes = 30;
        private const int WeekendWatchtowerMinutes = 60;

        public const string TypeCantico = "Cantico";
        public const string TypeDiscorso = "Discorso";
        public const string TypeLetturaBiblica = "Lettura biblica";
        public const string TypeCommenti = "Commenti";
        public const string TypeConsigli = "Consigli";
        public const string TypeGemme = "Gemme spirituali";
        public const string TypeStudio = "Studio";
        public const string TypeMinistero = "Ministero";
        public const string TypeTesori = "Tesori";
        public const string TypeVitaCristiana = "Vita cristiana";
        public const string TypeParte = "Parte";

        private static readonly Brush ColorSezione1 = CreateFrozenBrush(0x32, 0xC4, 0xE3);
        private static readonly Brush ColorSezione2 = CreateFrozenBrush(0xE3, 0xA2, 0x1D);
        private static readonly Brush ColorSezione3 = CreateFrozenBrush(0xE3, 0x4A, 0x2B);
        private static readonly Brush ColorCantico = CreateFrozenBrush(0x76, 0x76, 0x76);
        private static readonly Brush ColorCommenti = CreateFrozenBrush(0x9A, 0x9A, 0x9A);
        private static readonly Brush ColorConsigli = CreateFrozenBrush(0x58, 0x58, 0x58);

        public ObservableCollection<Parte> CreateParti(IEnumerable<ParsedParteData> parsedParti)
        {
            List<Parte> parti = new List<Parte>();

            foreach (ParsedParteData parsedParte in parsedParti)
            {
                TimeSpan durata = TimeSpan.FromMinutes(parsedParte.Minutes);
                parti.Add(new Parte(
                    parsedParte.Title,
                    durata,
                    parsedParte.Type,
                    GetBrush(parsedParte.VisualCategory),
                    durata,
                    parsedParte.Number));
            }

            return new ObservableCollection<Parte>(parti);
        }

        public ObservableCollection<Parte> CreatePartiFromSnapshot(IEnumerable<ParteSnapshotData> snapshotParti)
        {
            List<Parte> parti = new List<Parte>();

            foreach (ParteSnapshotData snapshotParte in snapshotParti)
            {
                TimeSpan tempoParte = TimeSpan.FromTicks(snapshotParte.TempoParteTicks);
                TimeSpan tempoScorrevole = TimeSpan.FromTicks(snapshotParte.TempoScorrevoleTicks);
                parti.Add(new Parte(
                    snapshotParte.Title,
                    tempoParte,
                    snapshotParte.Type,
                    CreateBrushFromHex(snapshotParte.ColorHex),
                    tempoScorrevole,
                    snapshotParte.Number));
            }

            return new ObservableCollection<Parte>(parti);
        }

        public IReadOnlyList<ParteSnapshotData> CreateSnapshot(IEnumerable<Parte> parti)
        {
            List<ParteSnapshotData> snapshot = new List<ParteSnapshotData>();

            foreach (Parte parte in parti)
            {
                snapshot.Add(new ParteSnapshotData(
                    parte.NomeParte,
                    parte.TempoParte.Ticks,
                    parte.TipoParte,
                    parte.ColoreSalvato,
                    parte.TempoScorrevole.Ticks,
                    parte.NumeroParte));
            }

            return snapshot;
        }

        public ObservableCollection<Parte> BuildMidweekStock()
        {
            return new ObservableCollection<Parte>
            {
                new Parte("Cantico iniziale", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 1),
                new Parte("Tesori della Parola di Dio", TimeSpan.FromMinutes(10), TypeTesori, ColorSezione1, TimeSpan.FromMinutes(10), 2),
                new Parte("Gemme spirituali", TimeSpan.FromMinutes(10), TypeGemme, ColorSezione1, TimeSpan.FromMinutes(10), 3),
                new Parte("Lettura biblica", TimeSpan.FromMinutes(4), TypeLetturaBiblica, ColorSezione1, TimeSpan.FromMinutes(4), 4),
                new Parte("Efficaci nel ministero", TimeSpan.FromMinutes(12), TypeMinistero, ColorSezione2, TimeSpan.FromMinutes(12), 5),
                new Parte("Vita cristiana", TimeSpan.FromMinutes(15), TypeVitaCristiana, ColorSezione3, TimeSpan.FromMinutes(15), 6),
                new Parte("Studio biblico di congregazione", TimeSpan.FromMinutes(30), TypeStudio, ColorSezione3, TimeSpan.FromMinutes(30), 7),
                new Parte("Commenti conclusivi", TimeSpan.FromMinutes(3), TypeCommenti, ColorCommenti, TimeSpan.FromMinutes(3), 8),
                new Parte("Cantico finale", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 9)
            };
        }

        public ObservableCollection<Parte> BuildWeekendStock()
        {
            return new ObservableCollection<Parte>
            {
                new Parte("Cantico (iniziale)", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 1),
                new Parte("Discorso pubblico", TimeSpan.FromMinutes(WeekendTalkMinutes), TypeDiscorso, ColorSezione2, TimeSpan.FromMinutes(WeekendTalkMinutes), 2),
                new Parte("Cantico (intermezzo)", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 3),
                new Parte("Studio Torre di Guardia", TimeSpan.FromMinutes(WeekendWatchtowerMinutes), TypeStudio, ColorSezione3, TimeSpan.FromMinutes(WeekendWatchtowerMinutes), 4),
                new Parte("Cantico (finale)", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 5)
            };
        }

        public ObservableCollection<Parte> BuildMemorialSchema()
        {
            return new ObservableCollection<Parte>
            {
                new Parte("Cantico 25", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 1),
                new Parte("Discorso commemorazione", TimeSpan.FromMinutes(MemorialTalkMinutes), TypeDiscorso, ColorSezione3, TimeSpan.FromMinutes(MemorialTalkMinutes), 3),
                new Parte("Cantico 18", TimeSpan.FromMinutes(SongMinutes), TypeCantico, ColorCantico, TimeSpan.FromMinutes(SongMinutes), 4)
            };
        }

        public void ApplyWeekendSongs(ObservableCollection<Parte> stock, WeekendSongSelection songs)
        {
            if (songs.Song2.HasValue)
            {
                stock[2] = new Parte(
                    $"Cantico {songs.Song2.Value}",
                    TimeSpan.FromMinutes(SongMinutes),
                    TypeCantico,
                    ColorCantico,
                    TimeSpan.FromMinutes(SongMinutes),
                    stock[2].NumeroParte);
            }

            if (songs.Song3.HasValue)
            {
                stock[4] = new Parte(
                    $"Cantico {songs.Song3.Value}",
                    TimeSpan.FromMinutes(SongMinutes),
                    TypeCantico,
                    ColorCantico,
                    TimeSpan.FromMinutes(SongMinutes),
                    stock[4].NumeroParte);
            }
        }

        private static Brush GetBrush(ParteVisualCategory category)
        {
            return category switch
            {
                ParteVisualCategory.Sezione1 => ColorSezione1,
                ParteVisualCategory.Sezione2 => ColorSezione2,
                ParteVisualCategory.Sezione3 => ColorSezione3,
                ParteVisualCategory.Cantico => ColorCantico,
                ParteVisualCategory.Commenti => ColorCommenti,
                ParteVisualCategory.Consigli => ColorConsigli,
                _ => ColorCommenti
            };
        }

        private static Brush CreateFrozenBrush(byte r, byte g, byte b)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private static Brush CreateBrushFromHex(string colorHex)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(colorHex)
                    && new BrushConverter().ConvertFromString(colorHex) is Brush brush)
                {
                    return brush;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogWarning($"Impossibile ricreare il colore snapshot '{colorHex}'.", ex);
            }

            return Brushes.Black;
        }
    }
}

