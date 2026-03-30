#nullable enable

namespace CueTime.Classes.Utilities
{
    internal enum WebMeetingKind
    {
        Midweek,
        Weekend
    }

    internal enum ParteVisualCategory
    {
        Sezione1,
        Sezione2,
        Sezione3,
        Cantico,
        Commenti,
        Consigli
    }

    internal sealed record MeetingLinks(string MidweekHref, string WeekendStudyHref, string WeekendTalkHref);

    internal sealed record WeekendSongSelection(int? Song2, int? Song3);

    internal sealed record ParsedParteData(
        string Title,
        int Minutes,
        string Type,
        ParteVisualCategory VisualCategory,
        int? Number);

    internal sealed record ParteSnapshotData(
        string Title,
        long TempoParteTicks,
        string Type,
        string ColorHex,
        long TempoScorrevoleTicks,
        int? Number);
}

