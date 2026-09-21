 using System.Text;
 using HtmlAgilityPack;

 namespace PF_Bot.Features_Aux.Listing;

public class FFMpegDocsPage
{
    public required int      Number  { get; init; }
    public required string   Anchor  { get; init; }
    public required string   Title   { get; init; }
    public required string[] Content { get; init; }
}

/// Parses HTML docs, stores parsed pages.
public class FFMpegDocumentation
{
    // DATA

    public readonly List<FFMpegDocsPage>
        PagesAF = [], // 122
        PagesVF = []; // 296

    // PARSING

    public const string
        HOST = "https://ffmpeg.org/",
        URL  = "https://ffmpeg.org/ffmpeg-filters.html";

    private const string
        _xp_afs = "//h3[starts-with(text(),  '8')]",
        _xp_vfs = "//h3[starts-with(text(), '11')]";

    private const RegexOptions RO_COMP_NOSPACE = RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;

    private static readonly Regex
        _r_title = new("""(?:\d+)\.(\d+) (.+)""", RegexOptions.Compiled),
        _r_a_ref = new("""<a\sclass="ref"       \shref="(\S+?)">""", RO_COMP_NOSPACE),
        _r_a_man = new("""<a\sdata-manual="\S+?"\shref="(\S+?)">""", RO_COMP_NOSPACE);

    public FFMpegDocumentation() => Parse();

    private void Parse()
    {
        var sw = Stopwatch.StartNew();

        var       file_exists = File_FFMpegDocsPage.FileExists; 
        var doc = file_exists
            ? new HtmlDocument().Fluent(x => x.Load(File_FFMpegDocsPage))
            : new HtmlWeb().Load(URL);

        sw.Log($"FFMPEG DOCS -> parse file ({(file_exists ? "local" : "online")})");

        if (file_exists.Janai())
        {
            File.WriteAllText(File_FFMpegDocsPage, doc.ParsedText);
            sw.Log("FFMPEG DOCS -> save file");
        }

        ParseFilters(PagesAF, doc.DocumentNode.SelectNodes(_xp_afs));
        ParseFilters(PagesVF, doc.DocumentNode.SelectNodes(_xp_vfs));
        sw.Log("FFMPEG DOCS -> parse filters");
    }

    private static void ParseFilters
        (List<FFMpegDocsPage> pages, HtmlNodeCollection nodes) // nodes - h3 of filters
    {
        pages.Capacity = nodes.Count;
        foreach (var node in nodes)
        {
            var match  = _r_title.Match(node.FirstChild.InnerText);
            var number = match.ExtractGroup(1, int.Parse);
            var title  = match.ExtractGroup(2, s => s, "");
            var number_length = 1 + (int)Math.Log10(number);
            var offset1 = number_length + 6 + title.Length; // ("🎬 " + " - ").len = 6
            var offsetN = offset1 + 6; // " 📃6/7".len = 6
            var content = ParseContent(node.NextElementSibling()!);
            var content_pages = content.SplitIntoPages(offset1, offsetN);
            pages.Add(new FFMpegDocsPage
            {
                Anchor   = node.PrevElementSibling()!.Attributes["name"].Value,
                Number   = number,
                Title    = title,
                Content  = content_pages,
            });
        }
    }

    // PARSING CONTENT

    // (ANTI)WARNING! HAP shit can be null even tho HAP doesn't expose nullability
    // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

    private const string BULLET = "•", TRIG = "▵";

    private static string ParseContent(HtmlNode node) // node - 1st node of filter section, usually p
    {
        // filter page sections: description [p…, dl], Examples [a, h4, ul], Commands [a, h4, p…, dl]
        // edge cases: examples a5 crossfade, commands a35 amix, table^3 a37 aneq
        var sb = new StringBuilder();
        while (true)
        {
            if      (node.Name == "a") { } // skip to headers
            else if (node.Name == "h3") break; // next filter
            else if (node.Name == "h4") // subsections
            {
                var text = node.FirstChild.InnerText.Split(' ', 2)[1];
                sb.Append("\n\n").Append("<u>").Append(text).Append("</u>:");
            }
            else if (node.Name == "p") // regular ass text
            {
                sb.Append("\n\n").Append(Sanitize(node.InnerHtml));
            }
            else if (node.Name == "div") // example w/o ul  |  8.26 aformat, 8.92 hdcd, 8.95 join, …
            {
                var pre = node.FirstElementChild()!;
                sb.Append('\n').Append("<blockquote>").Append("<code>");
                sb.Append(Sanitize(pre.InnerHtml, make_one_line: false));
                sb.Append("</code>").Append("</blockquote>");
            }
            else if (node.Name == "ul") ParseExamples_UL(sb, node.ElementChildren());
            else if (node.Name == "dl") ParseTable_DL   (sb, node.ElementChildren());

            node = node.NextElementSibling()!;
        }
        return sb.ToString();
    }

    private static void ParseExamples_UL
        (StringBuilder sb, IEnumerable<HtmlNode> nodes) // nodes - ul children (li)
    {
        sb.Append('\n').Append("<blockquote expandable>");
        var n = false;
        foreach (var li in nodes)
        {
            if (n) sb.Append('\n');
            n = true;
            var node = li.FirstChild;
            while (true)
            {
                if (node == null) break;
                if (node.Name == "#text")
                {
                    sb.Append($"{BULLET} ").Append(Sanitize(node.InnerHtml));
                    // ^ text before example
                }
                else if (node.Name == "div")
                {
                    var pre = node.FirstElementChild()!;
                    var txt = Sanitize(pre.InnerHtml, make_one_line: false);
                    sb.Append('\n').Append("<code>").Append(txt).Append("</code>");
                    // ^ example code
                }
                else if (node.Name == "p")
                {
                    sb.Append('\n').Append($"{TRIG} ").Append(Sanitize(node.InnerHtml));
                    // ^ text after example
                }
                else if (node.Name == "a") // 11.64 derain
                {
                    sb.Append(' ').Append(node.OuterHtml);
                    // ^ it's not even in the examples section lol
                }
                else if (node.Name is "code" or "samp" or "var") // 8.25 afir
                {
                    sb.Append(' ').Append("<code>").Append(Sanitize(node.InnerHtml)).Append("</code>");
                    var text_node = node.NextSibling;
                    if (text_node.NodeType == HtmlNodeType.Text)
                        sb.Append(Sanitize(text_node.InnerHtml));
                    // ^ tags & text inside [text before example]
                }
                else if (node.Name == "dl") // 11.50 curves
                {
                    ParseTable_DL(sb, node.ElementChildren(), nesting: 1);
                    // ^ table
                }
                else
                    throw new UnexpectedException($"PARSING UL >> NODE {node.Name}? WTF");

                node = node.NextElementSibling();
            }
        }
        sb.Append("</blockquote>");
    }

    private static void ParseTable_DL
        (StringBuilder sb, IEnumerable<HtmlNode> nodes, int nesting = 0) // nodes - dl children
    {
        // table can be nested up 3 times
        if (nesting == 1) sb.Append('\n').Append("<blockquote>");
        var n = nesting != 1;
        foreach (var node in nodes)
        {
            if (node is { Name: "dt" } node_dt)
            {
                if (n) sb.Append('\n');
                n = true;
                var samp = node_dt.FirstElementChild()!;
                for (var i = 2; i <= nesting; i++) sb.Append("    ");
                sb.Append($"{BULLET} ").Append("<code>").Append(Sanitize(samp.InnerHtml)).Append("</code>");
                // ^ option/value name
            }
            else if (node is { Name: "dd" } node_dd)
            {
                ParseTable_DD(sb, node_dd.ElementChildren(), nesting);
                // ^ lore
            }
            else
                throw new UnexpectedException($"PARSING DL >> NODE {node.Name}? WTF");
        }
        if (nesting == 1) sb.Append("</blockquote>");
    }

    private static void ParseTable_DD
        (StringBuilder sb, IEnumerable<HtmlNode> nodes, int nesting = 0) // nodes - dd children
    {
        var first = true;
        foreach (var node in nodes)
        {
            if (node is { Name: "p" } node_p)
            {
                if (first)
                {
                    first = false;
                    sb.Append(" - ")     .Append(Sanitize(node_p.InnerHtml));
                    // ^ just text (very common)
                }
                else
                {
                    sb.Append('\n');
                    for (var i = 2; i <= nesting; i++) sb.Append("    ");
                    sb.Append($"{TRIG} ").Append(Sanitize(node_p.InnerHtml));
                    // ^ additional texts (quite often)
                }
            }
            else if (node is { Name: "dl" } node_dl)
            {
                ParseTable_DL(sb, node_dl.ElementChildren(), nesting + 1);
                // ^ nested table (sometimes)
            }
            else if (node is { Name: "div" } node_div) // 8.120 volume
            {
                var pre = node_div.FirstElementChild()!;
                var txt = Sanitize(pre.InnerHtml, make_one_line: false);
                sb.Append('\n').Append("<code>").Append(txt).Append("</code>");
                // ^ example (lol)
            }
            else if (node is { Name: "a" }) // 11.39 colorspace
            {
                // skip, it's there by mistake
            }
            else if (node is { Name: "ul" }) // 11.76 drawtext
            {
                ParseExamples_UL(sb, node.ElementChildren());
            }
            else
                throw new UnexpectedException($"PARSING DD >> NODE {node.Name}? WTF");
        }
    }

    private static string Sanitize(string s, bool make_one_line = true)
    {
        s = s.TrimEnd();
        if (make_one_line)
            s = s.Replace("\n", " ");
        if (s.Contains('&'))
            s = s // <> - don't replace!, &" - replaced by telegram anyway, rest - should be replaced here!
                .Replace("&amp;", "&")
                .Replace("&nbsp;", "\u00a0")
                .Replace("&ndash;", "--")
                .Replace("&lsquo;", "'")
                .Replace("&rsquo;", "'")
                .Replace("&ldquo;", "\"")
                .Replace("&rdquo;", "\"")
                .Replace("&quot;", "\"");
        if (s.Contains("<"))
        {
            s = s.Replace("<br>", $"\n{TRIG}"); // 11.270 v360
            if (s.Contains("<p"))
                s = s.Replace("</p> <p>", "");
            if (s.Contains("<var"))
                s = s
                    .Replace("<var class=\"var\">", "<code>")
                    .Replace("</var>", "</code>");
            if (s.Contains("<samp"))
                s = s
                    .Replace("<samp class=\"samp\">",   "<code>")
                    .Replace("<samp class=\"option\">", "<code>")
                    .Replace("<samp class=\"file\">",   "<code>")
                    .Replace("</samp>", "</code>");
            if (s.Contains("<a"))
            {
                s = _r_a_ref.Replace(s, m => $"<a href=\"{URL }{m.Groups[1].Value}\">");
                s = _r_a_man.Replace(s, m => $"<a href=\"{HOST}{m.Groups[1].Value}\">");
            }
        }
        return s;
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
        foreach (var page in PagesAF) AppendPage(page, 8);
        sb.Append("<a name=\"Video-Filters\"></a>\n");
        sb.Append("<h2>11 Video Filters</h2>\n");
        foreach (var page in PagesVF) AppendPage(page, 11);
        File.WriteAllText(Dir_Static.Combine("ffmpeg-parsed.html"), sb.ToString());

        void AppendPage(FFMpegDocsPage page, int kind) => sb
            .Append($"<a name=\"{page.Anchor}\"></a>\n")
            .Append("<h3>")
            .Append(kind)
            .Append('.')
            .Append(page.Number)
            .Append(' ')
            .Append(page.Title)
            .Append("</h3>\n<div>")
            .AppendJoin("\n\n<br>", page.Content)
            .Append("</div>\n");
    }
}

/* LONG ASS PAGES (for testing):
       LEN   AV# NAME
     11083 |  85 dynaudnorm
      5890 | 108 silenceremove
      6903 |  72 dnn_processing
     17992 |  76 drawtext
     10882 |  93 fieldmatch
      4443 | 145 lensfun
     26819 | 147 libplacebo
      5349 | 188 overlay
      8125 | 207 qrencode
     11835 | 221 scale
      5372 | 236 signalstats
      7162 | 258 tinterlace
      7849 | 270 v360
      4376 | 285 waveform
      6882 | 296 zscale
^ COUNT: 15
*/