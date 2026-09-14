using System.Text;

namespace PF_Bot.Features_Aux.Listing;

public class FFMpegDocsPage
{
    public required int    Number  { get; init; }
    public required string Anchor  { get; init; }
    public required string Title   { get; init; }
    public required string Content { get; init; }
}

/// Parses HTML docs, stores parsed pages.
public class FFMpegDocumentation
{
    // DATA

    public readonly List<FFMpegDocsPage>
        PagesAF = [], // 122
        PagesVF = []; // 296

    // PARSING

    public const string URL = "https://ffmpeg.org/ffmpeg-filters.html";

    private const RegexOptions
        ROps = RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;

    private static readonly Regex
        _r_h2    = new("""<h2\sclass="chapter">    ( [0-9]+)\s([^<]+)<.+?>               <\/h2>""", ROps),
        _r_h3_af = new("""<h3\sclass="section"> 8\.([.0-9]+)\s([^<]+)<.+?href="(.+?)".+?><\/h3>""", ROps),
        _r_h3_vf = new("""<h3\sclass="section">11\.([.0-9]+)\s([^<]+)<.+?href="(.+?)".+?><\/h3>""", ROps);

    public FFMpegDocumentation() => Parse().Wait();

    private async Task Parse()
    {
        var html = File_FFMpegDocsPage.FileExists
            ? await File.ReadAllTextAsync(File_FFMpegDocsPage)
            : await GetOnlineWebPage();

        var matches_h2   = _r_h2   .Matches(html);
        var matches_h3_a = _r_h3_af.Matches(html);
        var matches_h3_v = _r_h3_vf.Matches(html);
        PopulateFilters(html, matches_h3_a, matches_h2[ 8], PagesAF);
        PopulateFilters(html, matches_h3_v, matches_h2[11], PagesVF);

        return;

        async Task<string> GetOnlineWebPage()
        {
            using var client = HttpClientFactory.CreateClient();
            return await client.GetStringAsync(URL);
        }
    }

    private static void PopulateFilters
        (string html, MatchCollection filters, Match nextChapter, List<FFMpegDocsPage> pages)
    {
        for (var i = 0; i < filters.Count; i++)
        {
            var match  = filters[i];
            var number = match.Groups[1].Value;
            var title  = match.Groups[2].Value;
            var anchor = match.Groups[3].Value;

            var content_start = match.Index + match.Length;
            var content_end   = i + 1 == filters.Count
                ? nextChapter   .Index
                : filters[i + 1].Index;
            var content = html.Substring(content_start, content_end - content_start);

            pages.Add(new FFMpegDocsPage
            {
                Number   = int.Parse(number),
                Anchor   = anchor,
                Title    = title,
                Content  = ParseContent(content),
            });
        }
    }

    // PARSING CONTENT

    private static readonly Regex
        _r_content_p  = new("""<p>\s?((?:\s|\S)+?)\s?<\/p>""", ROps),
        _r_content_EX = new("""<h4\sclass="subsection">(?:\d+\.){2}\d+\sExamples<.+?><\/h4>""", ROps),
        _r_content_CO = new("""<h4\sclass="subsection">(?:\d+\.){2}\d+\sCommands<.+?><\/h4>""", ROps),
        _r_content_li = new("""<li>((?:\s|\S)+?)<""", ROps),
        _r_content_ex = new("""<pre\sclass="example-preformatted">((?:\s|\S)+?)<\/pre>""", ROps),
        _r_content_dl = new("""<dl\sclass="table">\s?((?:\s|\S)+?)\s?<\/dl>(?!\s+?<\/dd>)(?=\s+?<a)""", ROps);

    private static string ParseContent(string content)
    {
        var groups = _r_content_p .Matches(content).Select(x => new Group(x.Groups[1], TagType.Paragraph))
            .Concat (_r_content_EX.Matches(content).Select(x => new Group(x.Groups[0], TagType.Examples)))
            .Concat (_r_content_CO.Matches(content).Select(x => new Group(x.Groups[0], TagType.Commands)))
            .Concat (_r_content_li.Matches(content).Select(x => new Group(x.Groups[1], TagType.Li)))
            .Concat (_r_content_ex.Matches(content).Select(x => new Group(x.Groups[1], TagType.Example)))
            .Concat (_r_content_dl.Matches(content).Select(x => new Group(x.Groups[1], TagType.Table)))
            .OrderBy(x => x.Index)
            .ToList();

        var tables = groups
            .Where(x => x.Type == TagType.Table)
            .ToList();

        var sb = new StringBuilder();
        var example_section = false;
        foreach (var group in groups)
        {
            switch (group.Type)
            {
                case TagType.Paragraph:
                    if (tables.Any(x => group.Index > x.Index && group.End < x.End)) continue;
                    sb.Append(example_section ? "\n• " : "\n\n");
                    sb.Append(Sanitize(group.Value));
                    break;
                case TagType.Examples:
                    example_section = true;
                    sb.Append("\n\n").Append("<blockquote expandable><b>Examples</b>:");
                    break;
                case TagType.Commands:
                    if (example_section) sb.Append("</blockquote>");
                    example_section = false; // commands go after examples
                    sb.Append("\n\n").Append("<u>Commands</u>:");
                    break;
                case TagType.Li:
                    sb.Append(example_section ? "\n• " : "\n\n");
                    sb.Append(Sanitize(group.Value));
                    break;
                case TagType.Example:
                    sb.Append('\n').Append($"<code>{Sanitize(group.Value)}</code>");
                    break;
                case TagType.Table:
                    AppendTable(sb, group);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (example_section) sb.Append("</blockquote>");

        return sb.ToString();
    }

    private static readonly Regex
        _r_content_dt_dd = new("""(?:<dt>(?:&lsquo;)?<.+>(.+?)<\/.+>(?:&rsquo;)?\s?<\/dt>)\s?(?:<dd>\s?<.+?>((?:\s|\S)+?)<\/.+>\s?(?:<dl\sclass="table">\s?((?:\s|\S)+)\s?<\/dl>\s*?(?:\s?<p>((?:\s|\S)+?)\s?<\/p>)?)?\s*?<\/dd>)?""", ROps);
    // todo make it work with nested^2 tables

    private static void AppendTable(StringBuilder sb, Group group)
    {
        sb.Append('\n');
        var html = group.Value;
        var rows = _r_content_dt_dd.Matches(html);
        foreach (Match row in rows)
        {
            AppendTableRow(sb, row);
            var t = row.Groups[3].Value; // nested table (optional)
            if (t.IsNotNull_NorEmpty())
            {
                var rows2 = _r_content_dt_dd.Matches(t);
                sb.Append("\n<blockquote><b>Values</b>:");
                foreach (Match row2 in rows2)
                {
                    AppendTableRow(sb, row2);
                    var t2 = row2.Groups[3].Value; // nested^2 table (example: 8.37 anequalizer > params > t)
                    if (t2.IsNotNull_NorEmpty())
                    {
                        var rows3 = _r_content_dt_dd.Matches(t2);
                        foreach (Match row3 in rows3)
                        {
                            AppendTableRow(sb, row3, prefix: "    • ");
                        }
                    }
                }
                sb.Append("</blockquote>");
            }

            var p = row.Groups[4].Value;
            if (p.IsNotNull_NorEmpty())
            {
                sb.Append('\n').Append(Sanitize(p));
            }
        }
    }

    private static void AppendTableRow
        (StringBuilder sb, Match row, string prefix = "• ")
    {
        var k = row.Groups[1].Value; // option
        var v = row.Groups[2].Value; // description
        _ = v.IsNull_OrEmpty()
            ? sb.Append($"\n{prefix}<code>{k}</code>")
            : sb.Append($"\n{prefix}<code>{k}</code> - {Sanitize(v)}");
    }

    private static string Sanitize(string s)
    {
        s = s.Replace("\n", " ");
        if (s.Contains("&"))
            s = s // <> - don't replace!, &" - replaced by telegram anyway, rest - should be replaced here!
                .Replace("&amp;", "&")
                .Replace("&nbsp;", "\u00a0")
                .Replace("&ndash;", "--")
                .Replace("&lsquo;", "'")
                .Replace("&rsquo;", "'")
                .Replace("&ldquo;", "\"")
                .Replace("&rdquo;", "\"")
                .Replace("&quot;", "\"");
        if (s.Contains("<p"))
            s = s.Replace("</p> <p>", "");
        if (s.Contains("<var"))
            s = s.Replace("<var class=\"var\">", "<code>").Replace("</var>", "</code>");
        if (s.Contains("<samp"))
            s = s.Replace("<samp class=\"samp\">", "<code>").Replace("</samp>", "</code>");
        return s;
    }

    private readonly struct Group(Capture group, TagType type)
    {
        public string Value { get; } = group.Value;
        public int    Index { get; } = group.Index;
        public int   Length { get; } = group.Length;
        public TagType Type { get; } = type;

        public int End => Index + Length;
    }

    private enum TagType
    {
        Paragraph, Examples, Commands, Li, Example, Table
    }

    // DEBUG

    /// Init a git repo in <c>Static</c> folder.
    /// Create file <c>ffmpeg-parsed.html</c> from <c>ffmpeg.html</c>
    /// leaving only 8 and 11 sections (AF and VF).
    /// Replace all <c>\n\n</c> with <c>\n</c> and commit this file.
    /// Run this method to update the file and compare the changes with IDE diff.
    public void SaveParsedAsFile()
    {
        var sb = new StringBuilder();
        sb.Append("<a name=\"Audio-Filters\"></a>\n");
        sb.Append("<h2>8 Audio Filters</h2>\n");
        foreach (var page in PagesAF)
        {
            sb
                .Append($"<a name=\"{page.Anchor[1..]}\"></a>\n")
                .Append("<h3>8.")
                .Append(page.Number)
                .Append(' ')
                .Append(page.Title)
                .Append("</h3>\n<div>")
                .Append(page.Content)
                .Append("</div>\n");
        }
        sb.Append("<a name=\"Video-Filters\"></a>\n");
        sb.Append("<h2>11 Video Filters</h2>\n");
        foreach (var page in PagesVF)
        {
            sb
                .Append($"<a name=\"{page.Anchor[1..]}\"></a>\n")
                .Append("<h3>11.")
                .Append(page.Number)
                .Append(' ')
                .Append(page.Title)
                .Append("</h3>\n<div>")
                .Append(page.Content)
                .Append("</div>\n");
        }
        File.WriteAllText(Dir_Static.Combine("ffmpeg-parsed.html"), sb.ToString());
    }
}