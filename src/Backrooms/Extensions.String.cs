namespace Witlesss.Backrooms;

public static partial class Extensions
{
    // CHANGE CASE

    public enum LetterCase
    {
        Lower, Upper, Sentence
    }

    public static string ToRandomLetterCase(this string text) => InLetterCase(text, GetRandomLetterCase());

    public static string InLetterCase(this string text, LetterCase mode) => mode switch
    {
        LetterCase.Lower    => text.ToLower(),
        LetterCase.Upper    => text.ToUpper(),
        LetterCase.Sentence => char.ToUpper(text[0]) + text[1..].ToLower(),
        _ => text
    };

    public static LetterCase GetRandomLetterCase() => Random.Shared.Next(8) switch
    {
        < 5 => LetterCase.Lower,
        < 7 => LetterCase.Sentence,
        _   => LetterCase.Upper
    };

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

    // PATH

    public static string RemoveExtension
        (this string path) => path.Remove(path.LastIndexOf('.'));

    public static string ReplaceExtension
        (this string path, string newExtension) => Regex.Replace(path, @"\.\S+$", newExtension);

    public static string GetExtension
        (this string? path, string fallback) => path != null ? Path.GetExtension(path) : fallback;

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

    public static T? ExtractGroup<T>
        (this Regex regex, int group, string input, Func<string, T> convert, T? fallback = default)
    {
        var match = regex.Match(input);
        return match.Success ? convert(match.Groups[group].Value) : fallback;
    }

    public static T? ExtractGroup<T>
        (this Regex regex, int group, string input, Func<string, T> convert, T? fallback = default) where T : struct
    {
        var match = regex.Match(input);
        return match.Success ? convert(match.Groups[group].Value) : fallback;
    }
}