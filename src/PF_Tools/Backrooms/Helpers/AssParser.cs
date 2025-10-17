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

public static class AssParser
{
    public static IEnumerable<string> ExtractTexts
        (string[] lines)
    {
        var index = lines.Length;
        for (var i = 0; i < index; i++)
        {
            if (lines[i] == "[Events]") index = i; // auto-break (i > index)
        }

        var index_Events = index;            // [Events]
        var index_Format = index_Events + 1; // Format: Layer, Start, …, Text
        var index_Dialog = index_Events + 2; // Dialogue: 0,0:00:00.00,…,TEXT

        var  line_Format = lines[index_Format];
        var commas_count = line_Format.Count(c => c is ',');

        foreach (var line in lines.Skip(index_Dialog))
        {
            if (line.StartsWith("Dialogue:").Janai()) break;

            var text_start = line.IndexOfNth(',', commas_count) + 1;
            var text = line.Substring(text_start);

            yield return _r_garbage.Replace(text, " ").Trim();
        }
    }

    private static readonly Regex
        _r_garbage = new(@"(?:{.*?}|\\N)+", RegexOptions.Compiled); // {…} & \N
}