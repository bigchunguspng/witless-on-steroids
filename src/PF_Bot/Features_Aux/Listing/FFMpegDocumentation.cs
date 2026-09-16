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

    private const int MAX_MESSAGE_LEN = 4096; // TEXT only, HTML tags are excluded.

    private const RegexOptions RO_COMP_NOSPACE = RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace;

    private static readonly Regex
        _r_tag   = new("""<.+?>""",               RegexOptions.Compiled),
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
            var offset = (int)Math.Log10(number) + 7 + title.Length;
            var content = SplitIntoPages(ParseContent(node.NextElementSibling()!), offset);
            pages.Add(new FFMpegDocsPage
            {
                Anchor   = node.PrevElementSibling()!.Attributes["name"].Value,
                Number   = number,
                Title    = title,
                Content  = content,
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

    // SPLIT CONTENT -> PAGES

    private const StringSplitOptions
        SPLIT_RM_EMPTY_TRIM = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    private static string[] SplitIntoPages(string content, int offset)
    {
        if (offset + content.Length <= MAX_MESSAGE_LEN)
            return [content]; // 92.3% exit here (386/418 files)

        if (offset + GetTextLength_HTML(content) <= MAX_MESSAGE_LEN)
            return [content]; //  4.1% here (17)

        // code for the rest 15  (3.6%) long ass files:
        // TL;DR:
        // Walk content line by line, keeping track of TEXT length and open tags.
        // When too long - split at line break.

        var sb = new StringBuilder();
        List <string> pages = []; // result
        Stack<string> tagsP = []; // breadcrumb of open tags, as for prev line
        Stack<string> tagsC = []; // breadcrumb of open tags, as for curr line
        var text_lengthP = 0; // of the current page, as for prev line
        var text_lengthC = 0; // of the current page, as for curr line
        var line_offset  = 0; // HTML, current line, chars
        var lines_paged  = 0; // first N lines assigned to pages
        var lines = content.Split('\n');
        var    curr_line = 0; // index
        while (curr_line < lines.Length)
        {
            // line can be: empty, [text & tag]*N & text?  | N>=0
            var line = lines[curr_line];
            if (line.Length - line_offset <= 0) // empty line / to text after tag
                goto LINE_END;

            // line not empty
            var i_tag = line.IndexOf('<', line_offset);
            if (i_tag < 0) // just text
            {
                text_lengthC += line.Length - line_offset;
                goto LINE_END;
            }

            // tag found
            var i_tag_end = line.IndexOf('>', i_tag + 1); // should be found!
            var tag_is_closing_part = line[i_tag     + 1] == '/';
            var tag_is_closing_full = line[i_tag_end - 1] == '/';
            var tag_is_opening_part = tag_is_closing_part.Janai() && tag_is_closing_full.Janai();

            // count length of text before tag
            text_lengthC += i_tag - line_offset;
            line_offset = i_tag_end + 1;

            // update tag breadcrumb
            if      (tag_is_closing_part) tagsC.Pop();
            else if (tag_is_opening_part)
            {
                var tag = line.Substring(i_tag + 1, i_tag_end - i_tag - 1);
                tagsC.Push(tag); // save THE WHOLE tag
            }

            continue;
            LINE_END:
            if (offset + text_lengthC > MAX_MESSAGE_LEN)
            {
                // write the page
                var lines_to_take = curr_line - lines_paged;
                var page_lines = lines.Skip(lines_paged).Take(lines_to_take);
                sb.AppendJoin('\n', page_lines);
                foreach (var tag in tagsP)
                {
                    // close tags
                    var name = tag.Split(' ', 2, SPLIT_RM_EMPTY_TRIM)[0];
                    sb.Append("</").Append(name).Append('>');
                }
                pages.Add(sb.ToString());

                lines_paged += lines_to_take;
                text_lengthC -= text_lengthP;

                // start the next page
                sb.Clear();
                sb.Append("\n\n");
                foreach (var tag in tagsP.Reverse())
                {
                    // open tags on the next page
                    sb.Append('<').Append(tag).Append('>');
                }
            }
            text_lengthC++; // \n
            text_lengthP = text_lengthC;
            curr_line++;
            line_offset = 0;
            tagsP = new Stack<string>(tagsC.Reverse());
        }

        // write the last page
        sb.AppendJoin('\n', lines.Skip(lines_paged));
        foreach (var tag in tagsP)
        {
            // close tags
            var name = tag.Split(' ', 2, SPLIT_RM_EMPTY_TRIM)[0];
            sb.Append("</").Append(name).Append('>');
        }
        pages.Add(sb.ToString());

        return pages.ToArray();
    }

    private static int GetTextLength_HTML(string content)
    {
        return content.Length - _r_tag.Matches(content).Sum(x => x.Length);
    }
}