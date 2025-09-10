using System.Diagnostics.CodeAnalysis;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_String
{
    // CHANGE CASE

    public static string EnsureIsNotUppercase(this string s)
    {
        return s.Length > 1 && s.Skip(1).Any(char.IsUpper) ? s[0] + s[1..].ToLower() : s;
    }

    // OTHER

    public static string? MakeNull_IfEmpty
        (this string? text) => string.IsNullOrEmpty(text) ? null : text;

    public static bool IsNullOrEmpty
        (this string? text) => string.IsNullOrEmpty(text);

    public static string Quote
        (this string text) => $"\"{text}\"";

    public static string Truncate
        (this string text, int length) => text.Length > length ? text[..(length - 1)] + "…" : text;

    public static int GetLineCount
        (this string text) => 1 + text.Count(x => x == '\n');

    public static ReadOnlySpan<char> SubstringTill
        (this string text, char c)
    {
        var index = text.IndexOf(c);
        return index >= 0 
            ? text.AsSpan(0, index)
            : text.AsSpan();
    }

    // PATH
    
    public static string RemoveExtension
        (this string path) => path.Remove(path.LastIndexOf('.'));

    public static string GetExtension_Or
        (this string? path, string fallback) => path != null ? Path.GetExtension(path) : fallback;

    public static void CreateDirectory
        (this string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory) == false)
            Directory.CreateDirectory(directory);
    }

    public static string ValidFileName(this string text, char x = '_')
    {
        var chars = Path.GetInvalidFileNameChars();
        return chars.Aggregate(text, (current, c) => current.Replace(c, x));
    }

    public static bool FileNameIsInvalid(this string text)
    {
        return Path.GetInvalidFileNameChars().Any(text.Contains);
    }

    // LANGUAGE DETECTION

    private static readonly Regex _lat = new("[A-Za-z]"), _cyr = new("[Ѐ-ӿ]");

    public static bool IsMostlyCyrillic(this string text)
    {
        return _cyr.Count(text) > _lat.Count(text);
    }

    private static readonly Regex _ukrD = new("[ієїґ]"), _rusD = new("[ыэъё]");
    private static readonly Regex _ukrM = new("[авдж]"), _rusM = new("[еоть]");

    public static bool LooksLikeUkrainian(this string text)
    {
        var u = _ukrD.Count(text);
        var r = _rusD.Count(text);

        return u > 0 || r > 0
            ? u > r
            : _ukrM.Count(text) >= _rusM.Count(text);
    }

    // CASE DETECTION

    private static readonly Regex LOWERCASE = new("[acegijm-su-zав-ух-џ]");

    public static float GetLowercaseRatio(this string text)
    {
        return Math.Clamp(LOWERCASE.Count(text) / (float)text.Length, 0F, 1F);
    }

    //
    
    private static readonly Regex _spaces = new(@"\s+"), _brackets = new(@"(\s?\(+([^\(]+?)\)+)|(\s?\[+([^\[]+?)\]+)");

    public static string RemoveTextInBrackets(this string text)
    {
        while (_brackets.IsMatch(text))
        {
            text = _brackets.Replace(text, match => match.Value.StartsWith(' ') ? "" : " ");
        }

        return _spaces.Replace(text, " ").Trim();
    }

    // REGEX

    public static Match? MatchOrNull(this Regex regex, string? text) => text is null ? null : regex.Match(text);

    public static string? GroupOrNull
        (this Match match, int group) => match.Groups[group].Success ? match.Groups[group].Value : null;

    [return: NotNullIfNotNull(nameof(fallback))]
    public static T? ExtractGroup<T>
        (this Regex regex, int group, string input, Func<string, T> convert, T? fallback = default)
    {
        var match = regex.Match(input);
        return match.Success ? convert(match.Groups[group].Value) : fallback;
    }

    [return: NotNullIfNotNull(nameof(fallback))]
    public static T? ExtractGroup<T>
        (this Match match, int group, Func<string, T> convert, T? fallback = default)
    {
        var g = match.Groups[group];
        return g.Success ? convert(g.Value) : fallback;
    }
}