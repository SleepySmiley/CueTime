#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace InTempo.Classes.Utilities
{
    internal sealed class HtmlParteParser
    {
        private static readonly string[] MwbSectionTreasures = { "TESORI DELLA PAROLA DI DIO" };
        private static readonly string[] MwbSectionMinistry = { "EFFICACI NEL MINISTERO" };
        private static readonly string[] MwbSectionLife = { "VITA CRISTIANA" };

        private static readonly string[] MwbMinistryAllowedParts =
        {
            "Iniziare una conversazione",
            "Coltivare l’interesse",
            "Fare discepoli",
            "Spiegare quello in cui si crede",
            "Discorso"
        };

        private const string TitleIntroComments = "Commenti introduttivi";
        private const string TitleBibleReading = "Lettura biblica";
        private const string TitleGems = "Gemme spirituali";
        private const string TitleCongBibleStudy = "Studio biblico di congregazione";
        private const string TitleFinalComments = "Commenti conclusivi";
        private const string TitleCounsel = "Consigli";
        private const int SongMinutes = 5;

        private static readonly Regex MinutesRegex = new Regex(
            @"(?<!\d)(\d{1,3})\s*(?:min\.?|minuti|minute)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MemorialDateLineRegex = new Regex(
            @"\b(?<year>\d{4})\s*:\s*(?:(?:luned[iì]|marted[iì]|mercoled[iì]|gioved[iì]|venerd[iì]|sabato|domenica)\s+)?(?<day>\d{1,2})\s+(?<month>[A-Za-zÀ-ÿ]+)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public bool TryGetArticleNode(string html, out HtmlNode? article)
        {
            HtmlDocument doc = LoadHtml(html);
            article = FindArticleNode(doc);
            return article != null && !string.IsNullOrWhiteSpace(article.InnerText);
        }

        public MeetingLinks ExtractMeetingLinks(string weekHtml)
        {
            HtmlDocument doc = LoadHtml(weekHtml);
            return new MeetingLinks(
                ExtractSectionFirstLink(doc, "Vita e ministero"),
                ExtractWeekendStudyLink(doc),
                ExtractWeekendTalkLink(doc));
        }

        public IReadOnlyList<ParsedParteData> ParseMidweekFromHtml(string html)
        {
            if (!TryGetArticleNode(html, out HtmlNode? article) || article == null)
            {
                return Array.Empty<ParsedParteData>();
            }

            List<ParsedParteData> result = new List<ParsedParteData>();
            string currentSection = "Apertura";
            bool counselMode = false;
            int partNo = 0;
            int? lastNumbered = null;

            HtmlNodeCollection? nodes = article.SelectNodes(".//*[self::h2 or self::h3 or self::p]");
            if (nodes == null)
            {
                return result;
            }

            foreach (HtmlNode node in nodes)
            {
                string raw = NormalizeSpaces(HtmlEntity.DeEntitize(node.InnerText ?? string.Empty));
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                if (node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
                {
                    if (MatchesAny(raw, MwbSectionTreasures))
                    {
                        currentSection = "Tesori della Parola di Dio";
                    }
                    else if (MatchesAny(raw, MwbSectionMinistry))
                    {
                        currentSection = "Efficaci nel ministero";
                    }
                    else if (MatchesAny(raw, MwbSectionLife))
                    {
                        currentSection = "Vita cristiana";
                    }

                    continue;
                }

                if (!node.Name.Equals("h3", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (raw.IndexOf("Commenti introduttivi", StringComparison.OrdinalIgnoreCase) >= 0
                    && raw.IndexOf("Cantico", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string song = ExtractCanticoTitle(raw);
                    if (!string.IsNullOrWhiteSpace(song))
                    {
                        result.Add(new ParsedParteData(song, SongMinutes, ParteFactory.TypeCantico, ParteVisualCategory.Cantico, null));
                    }

                    int introMinutes = FindMinutesForHeading(node);
                    result.Add(new ParsedParteData(TitleIntroComments, introMinutes, ParteFactory.TypeCommenti, ParteVisualCategory.Commenti, null));
                    continue;
                }

                if (raw.IndexOf("Commenti conclusivi", StringComparison.OrdinalIgnoreCase) >= 0
                    && raw.IndexOf("Cantico", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int endMinutes = FindMinutesForHeading(node);
                    result.Add(new ParsedParteData(TitleFinalComments, endMinutes, ParteFactory.TypeCommenti, ParteVisualCategory.Commenti, null));

                    string song = ExtractCanticoTitle(raw);
                    if (!string.IsNullOrWhiteSpace(song))
                    {
                        result.Add(new ParsedParteData(song, SongMinutes, ParteFactory.TypeCantico, ParteVisualCategory.Cantico, null));
                    }

                    continue;
                }

                if (raw.StartsWith("Cantico", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("Canto", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new ParsedParteData(ExtractCanticoTitle(raw), SongMinutes, ParteFactory.TypeCantico, ParteVisualCategory.Cantico, null));
                    continue;
                }

                string title = RemoveLeadingNumber(raw);

                if (title.Equals(TitleGems, StringComparison.OrdinalIgnoreCase))
                {
                    int minutes = FindMinutesForHeading(node);
                    partNo++;
                    lastNumbered = partNo;
                    result.Add(new ParsedParteData(TitleGems, minutes, ParteFactory.TypeGemme, ParteVisualCategory.Sezione1, partNo));
                    continue;
                }

                if (title.StartsWith(TitleBibleReading, StringComparison.OrdinalIgnoreCase))
                {
                    int minutes = FindMinutesForHeading(node);
                    partNo++;
                    lastNumbered = partNo;
                    result.Add(new ParsedParteData(TitleBibleReading, minutes, ParteFactory.TypeLetturaBiblica, ParteVisualCategory.Sezione1, partNo));
                    counselMode = true;
                    result.Add(new ParsedParteData(TitleCounsel, 1, ParteFactory.TypeConsigli, ParteVisualCategory.Consigli, lastNumbered));
                    continue;
                }

                if (title.Equals(TitleCongBibleStudy, StringComparison.OrdinalIgnoreCase))
                {
                    int minutes = FindMinutesForHeading(node);
                    if (minutes <= 0)
                    {
                        minutes = 30;
                    }

                    partNo++;
                    lastNumbered = partNo;
                    result.Add(new ParsedParteData(TitleCongBibleStudy, minutes, ParteFactory.TypeStudio, ParteVisualCategory.Sezione3, partNo));
                    continue;
                }

                if (currentSection.Equals("Efficaci nel ministero", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsAllowedMinistryPart(title))
                    {
                        int minutes = FindMinutesForHeading(node);
                        partNo++;
                        lastNumbered = partNo;
                        result.Add(new ParsedParteData(
                            title,
                            minutes,
                            ResolveTipoFromTitleAndSection(title, currentSection),
                            ParteVisualCategory.Sezione2,
                            partNo));

                        if (counselMode)
                        {
                            result.Add(new ParsedParteData(TitleCounsel, 1, ParteFactory.TypeConsigli, ParteVisualCategory.Consigli, lastNumbered));
                        }
                    }

                    continue;
                }

                if (currentSection.Equals("Vita cristiana", StringComparison.OrdinalIgnoreCase))
                {
                    int minutes = FindMinutesForHeading(node);
                    partNo++;
                    result.Add(new ParsedParteData(
                        title,
                        minutes,
                        ResolveTipoFromTitleAndSection(title, currentSection),
                        ParteVisualCategory.Sezione3,
                        partNo));
                    continue;
                }

                if (currentSection.Equals("Tesori della Parola di Dio", StringComparison.OrdinalIgnoreCase))
                {
                    int minutes = FindMinutesForHeading(node);
                    partNo++;
                    result.Add(new ParsedParteData(
                        title,
                        minutes,
                        ResolveTipoFromTitleAndSection(title, currentSection),
                        ParteVisualCategory.Sezione1,
                        partNo));
                }
            }

            return result;
        }

        public WeekendSongSelection ExtractWeekendSongsFromWtStudyHtml(string wtStudyHtml)
        {
            if (string.IsNullOrWhiteSpace(wtStudyHtml))
            {
                return new WeekendSongSelection(null, null);
            }

            HtmlDocument doc = LoadHtml(wtStudyHtml);
            HtmlNode article = doc.DocumentNode.SelectSingleNode("//article[@id='article']") ?? doc.DocumentNode;
            List<int> found = ExtractDistinctCanticoNumbers(article);

            if (found.Count == 0)
            {
                HtmlNodeCollection? pubRefNodes = article.SelectNodes(".//p[contains(concat(' ', normalize-space(@class), ' '), ' pubRefs ')]");
                if (pubRefNodes != null)
                {
                    foreach (HtmlNode pubRefNode in pubRefNodes)
                    {
                        found.AddRange(ExtractDistinctCanticoNumbers(pubRefNode, found));
                    }
                }
            }

            int? song2 = found.Count >= 1 ? found[0] : null;
            int? song3 = found.Count >= 2 ? found[^1] : null;
            return new WeekendSongSelection(song2, song3);
        }

        public DateTime? TryExtractOfficialMemorialDate(string html, int year)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            HtmlDocument doc = LoadHtml(html);
            string text = HtmlEntity.DeEntitize(doc.DocumentNode?.InnerText ?? string.Empty);
            text = NormalizeSpaces(text);

            foreach (Match match in MemorialDateLineRegex.Matches(text))
            {
                if (!match.Success)
                {
                    continue;
                }

                if (!int.TryParse(match.Groups["year"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedYear) || parsedYear != year)
                {
                    continue;
                }

                if (!int.TryParse(match.Groups["day"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int day))
                {
                    continue;
                }

                int month = ParseItalianMonthName(match.Groups["month"].Value);
                if (month <= 0)
                {
                    continue;
                }

                try
                {
                    return new DateTime(parsedYear, month, day);
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning("Data commemorazione non valida trovata nella pagina ufficiale.", ex);
                    return null;
                }
            }

            return null;
        }

        private static string ExtractSectionFirstLink(HtmlDocument doc, string sectionHeadingContains)
        {
            HtmlNodeCollection? h2Nodes = doc.DocumentNode.SelectNodes("//div[@id='materialNav']//h2");
            if (h2Nodes == null || h2Nodes.Count == 0)
            {
                return string.Empty;
            }

            foreach (HtmlNode h2 in h2Nodes)
            {
                string txt = (h2.InnerText ?? string.Empty).Trim();
                if (txt.IndexOf(sectionHeadingContains, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    HtmlNode? ul = h2.SelectSingleNode("./following-sibling::ul[1]");
                    HtmlNode? a = ul?.SelectSingleNode(".//a[1]");
                    if (a != null)
                    {
                        return a.GetAttributeValue("href", string.Empty);
                    }
                }
            }

            return string.Empty;
        }

        private static string ExtractWeekendStudyLink(HtmlDocument doc)
        {
            HtmlNode? container = doc.DocumentNode.SelectSingleNode("//div[@id='materialNav']");
            if (container == null)
            {
                return string.Empty;
            }

            HtmlNodeCollection? links = container.SelectNodes(".//a");
            if (links == null)
            {
                return string.Empty;
            }

            foreach (HtmlNode link in links)
            {
                string text = (link.InnerText ?? string.Empty).Trim();
                if (text.IndexOf("Studio", StringComparison.OrdinalIgnoreCase) >= 0
                    && text.IndexOf("Torre di Guardia", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return link.GetAttributeValue("href", string.Empty);
                }
            }

            return string.Empty;
        }

        private static string ExtractWeekendTalkLink(HtmlDocument doc)
        {
            HtmlNode? container = doc.DocumentNode.SelectSingleNode("//div[@id='materialNav']");
            if (container == null)
            {
                return string.Empty;
            }

            HtmlNodeCollection? links = container.SelectNodes(".//a");
            if (links == null)
            {
                return string.Empty;
            }

            foreach (HtmlNode link in links)
            {
                string text = (link.InnerText ?? string.Empty).Trim();
                if (text.IndexOf("Adunanza pubblica", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return link.GetAttributeValue("href", string.Empty);
                }
            }

            return string.Empty;
        }

        private static HtmlDocument LoadHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html ?? string.Empty);
            return doc;
        }

        private static HtmlNode? FindArticleNode(HtmlDocument? doc)
        {
            return doc?.DocumentNode?.SelectSingleNode("//article")
                ?? doc?.DocumentNode?.SelectSingleNode("//div[@id='content']")
                ?? doc?.DocumentNode?.SelectSingleNode("//body");
        }

        private static bool IsAllowedMinistryPart(string title)
        {
            foreach (string allowed in MwbMinistryAllowedParts)
            {
                if (title.Equals(allowed, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveTipoFromTitleAndSection(string title, string currentSection)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return ParteFactory.TypeParte;
            }

            string normalizedTitle = title.Trim();

            if (normalizedTitle.StartsWith("Cantico", StringComparison.OrdinalIgnoreCase)
                || normalizedTitle.StartsWith("Canto", StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeCantico;
            }

            if (normalizedTitle.StartsWith(TitleBibleReading, StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeLetturaBiblica;
            }

            if (normalizedTitle.IndexOf("Discorso", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ParteFactory.TypeDiscorso;
            }

            if (normalizedTitle.Equals(TitleIntroComments, StringComparison.OrdinalIgnoreCase)
                || normalizedTitle.Equals(TitleFinalComments, StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeCommenti;
            }

            if (normalizedTitle.Equals(TitleCounsel, StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeConsigli;
            }

            if (normalizedTitle.Equals(TitleGems, StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeGemme;
            }

            if (normalizedTitle.Equals(TitleCongBibleStudy, StringComparison.OrdinalIgnoreCase)
                || normalizedTitle.StartsWith("Studio", StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeStudio;
            }

            if (currentSection.Equals("Efficaci nel ministero", StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeMinistero;
            }

            if (currentSection.Equals("Tesori della Parola di Dio", StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeTesori;
            }

            if (currentSection.Equals("Vita cristiana", StringComparison.OrdinalIgnoreCase))
            {
                return ParteFactory.TypeVitaCristiana;
            }

            return ParteFactory.TypeParte;
        }

        private static int FindMinutesForHeading(HtmlNode heading)
        {
            string raw = NormalizeSpaces(HtmlEntity.DeEntitize(heading.InnerText ?? string.Empty));
            int minutes = ExtractMinutes(raw);
            if (minutes > 0)
            {
                return minutes;
            }

            HtmlNode? paragraph = heading.SelectSingleNode("following::p[1]");
            if (paragraph == null)
            {
                return 0;
            }

            string paragraphText = NormalizeSpaces(HtmlEntity.DeEntitize(paragraph.InnerText ?? string.Empty));
            return ExtractMinutes(paragraphText);
        }

        private static string ExtractCanticoTitle(string text)
        {
            Match match = Regex.Match(text ?? string.Empty, @"\b(?:Cantico|Canto)\s+(\d+)\b", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
            {
                return $"Cantico {number}";
            }

            return (text ?? string.Empty).Trim();
        }

        private static string RemoveLeadingNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            Match match = Regex.Match(value, @"^\s*\d+\.\s*(.+)$");
            return match.Success ? match.Groups[1].Value.Trim() : value.Trim();
        }

        private static bool MatchesAny(string text, IEnumerable<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int ExtractMinutes(string text)
        {
            Match match = MinutesRegex.Match(text ?? string.Empty);
            return match.Success && int.TryParse(match.Groups[1].Value, out int min) ? min : 0;
        }

        private static List<int> ExtractDistinctCanticoNumbers(HtmlNode rootNode, List<int>? seed = null)
        {
            List<int> found = seed ?? new List<int>();
            string text = NormalizeSpaces(HtmlEntity.DeEntitize(rootNode.InnerText ?? string.Empty));

            foreach (Match match in Regex.Matches(text, @"\b(?:CANTICO|CANTO)\s*(\d+)\b", RegexOptions.IgnoreCase))
            {
                if (!match.Success)
                {
                    continue;
                }

                if (int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)
                    && !found.Contains(number))
                {
                    found.Add(number);
                }
            }

            return found;
        }

        private static string NormalizeSpaces(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = value.Replace("\u00A0", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ");
            return normalized.Trim();
        }

        private static int ParseItalianMonthName(string monthName)
        {
            if (string.IsNullOrWhiteSpace(monthName))
            {
                return 0;
            }

            string normalized = monthName.Trim().ToLowerInvariant()
                .Replace("à", "a")
                .Replace("è", "e")
                .Replace("é", "e")
                .Replace("ì", "i")
                .Replace("ò", "o")
                .Replace("ù", "u");

            return normalized switch
            {
                "gennaio" => 1,
                "febbraio" => 2,
                "marzo" => 3,
                "aprile" => 4,
                "maggio" => 5,
                "giugno" => 6,
                "luglio" => 7,
                "agosto" => 8,
                "settembre" => 9,
                "ottobre" => 10,
                "novembre" => 11,
                "dicembre" => 12,
                _ => 0
            };
        }
    }
}
