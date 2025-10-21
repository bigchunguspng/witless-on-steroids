namespace PF_Tools.Backrooms.Helpers;

/*
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠿⠛⠛⠛⠛⠿⠿⠿⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠉⠁⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠈⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠻⠃⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠈⢿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠋⠄⠄⠄⠄⠄⠄⠄⠄⢸⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄⠄⠄⠄⠄⠄⠄⠄⣿⡆⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠄⠄⠄⠄⠄⠄⠄⠄⢸⣿⣷⠄⠸⡄⠄⢇⠄⠄⠄⠄⠄⠄⠄⠻⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡐⠄⠄⠄⠄⠄⠄⠄⠘⣿⣿⣷⣤⠄⠄⠐⢰⠄⠄⠄⢸⠄⠄⠄⠈⢿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠄⠄⠄⠄⠄⢴⣦⣤⣾⣿⣿⣿⣿⣷⣬⣴⡍⠄⠄⠄⠄⠄⠄⢠⠄⣸⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣦⡆⠄⠄⠄⠈⢿⣿⣿⣿⠿⢿⣿⣿⣿⣟⠔⠄⠄⢀⣔⣴⣴⣿⣾⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠄⠄⠄⣄⢀⠨⣝⠻⢿⡿⠟⣛⣿⡇⠄⠄⢢⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣰⣠⣶⠿⠿⠛⠁⠄⠄⣠⣶⠿⠛⠁⠄⠄⠘⠿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⠋⠁⠄⠄⠄⠄⠄⠄⠺⣿⠇⠄⠄⠄⠄⠄⠄⠄⠄⠉⠛⠻⢿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⠏⠄⠄⠄⠄⠄⣀⠄⢀⡤⠆⣀⡴⠂⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠹⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⠏⠄⠄⠄⠄⣠⣴⣾⣿⣿⣾⣯⡥⠶⠋⠁⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢹⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⠋⠄⠄⠄⠄⢸⣿⣿⣿⣿⣿⠿⠿⠒⠒⠂⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠿⣿⣿

ASS subs, or Advanced Substation Alpha subtitles, is a
subtitle format that supports advanced features like
custom fonts, colors, and animations. This makes them
popular for creating complex subtitles, especially for
anime, but they may not be supported by all media pla-
yers. They are often converted to simpler formats like
SRT to ensure compatibility across different devices
and platforms.                                       */

// Specs: http://www.tcax.org/docs/ass-specs.htm

public record StyleInfo(string Name, int Count, IEnumerable<string> Examples);

public static class AssParser
{
    public static StyleInfo[] ListStyles
        (string[] lines, int examplesLimit)
    {
        var styles = GetStyles    (lines);
        var events = GetEventsMeta(lines);
        return GetTexts(lines, events, styles)
            .GroupBy(x => x.StyleIndex)
            .Select(g =>
            {
                var name  = styles[g.Key];
                var texts = g.Select(x => x.Text).ToArray();
                var count = texts.Length;

                string[] examples;
                var examples_count = Math.Min(count, examplesLimit);
                if (examples_count == count)
                    examples = texts;
                else
                {
                    examples = new string[examples_count];
                    Array.Copy(texts, examples, examples_count);
                }

                return new StyleInfo(name, count, examples);
            })
            .OrderByDescending(x => x.Count)
            .ToArray();
    }

    public static IEnumerable<string>
        ExtractTexts
        (string[] lines, string[]? styles = null)
    {
        var events = GetEventsMeta(lines);
        return GetTexts(lines, events, styles).Select(x => x.Text);
    }

    // GET SECTIONS

    private static string[] GetStyles(string[] lines)
    {
        var index_Styles = Locate_Styles(lines); // [V4+ Styles] \n Format: Name, …
        var index_Style  = index_Styles + 2;     // Style: Default,…

        // count styles
        var styles_Count = 0;
        foreach (var line in lines.Skip(index_Style))
        {
            if (line.StartsWith("Style:").Janai()) break;

            styles_Count++;
        }

        // gather style names (1st column)
        var styles = new string[styles_Count];
        for (var i = 0; i < styles_Count; i++)
        {
            var line = lines[index_Style + i];
            var index_name  = line.IndexOf(' ') + 1;
            var index_comma = line.IndexOf(',');
            var length_name = index_comma - index_name;
            var name = line.Substring(index_name, length_name);
            styles[i] = name;
        }

        return styles;
    }

    private record struct EventsSectionMeta
    (
        int Dialog_Start,
        int Dialog_Count,
        int Column_Style,
        int Column_Text
    );

    private static EventsSectionMeta GetEventsMeta(string[] lines)
    {
        var index_Events = Locate_Events(lines); // [Events]
        var index_Format = index_Events + 1;     // Format: Layer, Start, …, Text
        var index_Dialog = index_Events + 2;     // Dialogue: 0,0:00:00.00,…,TEXT
        var  last_Dialog = lines.FindLastIndex(x => x.StartsWith("Dialogue:"));
        var count_Dialog = last_Dialog - index_Dialog + 1;

        var columns = lines[index_Format].Split(',');
        var column_Style = columns.FindIndex(x => x.EndsWith("Style"));
        var column_Text  = columns.FindIndex(x => x.EndsWith("Text"));

        return new EventsSectionMeta(index_Dialog, count_Dialog, column_Style, column_Text);
    }

    // LOCATE

    private static int Locate_Styles(string[] lines)
    {
        var length =  lines.Length;
        for (var i = 0; i < length; i++)
        {
            if (lines[i].EndsWith(/* [V4+ */"Styles]"))
               return i;
        }

        return length;
    }

    private static int Locate_Events(string[] lines)
    {
        var length =  lines.Length;
        for (var i = 0; i < length; i++)
        {
            if (lines[i] == "[Events]")
               return i;
        }

        return length;
    }

    // EXTRACT

    private static IEnumerable<(int StyleIndex, string Text)> GetTexts
    (
        IEnumerable<string> lines,
        EventsSectionMeta events,
        string[]? styles = null
    )
    {
        var linesDialogue = lines
            .Skip(events.Dialog_Start)
            .Take(events.Dialog_Count);
        foreach (var line in linesDialogue)
        {
            if (line.StartsWith("Dialogue:").Janai())
                continue;

            var style_index = -1;
            if (styles != null)
            {
                var comma_beforeStyle = events.Column_Style;
                var comma_after_Style = events.Column_Style + 1;
                var style_start = line.IndexOfNth(',', comma_beforeStyle) + 1;
                var style_end   = line.IndexOfNth(',', comma_after_Style);
                var style = line.Substring(style_start, style_end - style_start);
                _ = style_index = Array.IndexOf(styles, style);
                if (style_index < 0)
                    continue; // skip this style
            }

            var comma_beforeText = events.Column_Text;
            var text_start = line.IndexOfNth(',', comma_beforeText) + 1;
            var text = line.Substring(text_start);

            var text_tidy = _r_garbage.Replace(text, " ").Trim();
            if (text_tidy.StartsWith("m ") && text.Contains(@"\p"))
                continue; // skip drawings    text: {…\p1\…}m 18 18 l 18 180 …

            yield return (style_index, text_tidy);
        }
    }

    private static readonly Regex
        _r_garbage = new(@"(?:{.*?}|\\N)+", RegexOptions.Compiled); // {…} & \N
}