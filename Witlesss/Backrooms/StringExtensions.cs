using System;
using System.Text.RegularExpressions;

namespace Witlesss.Backrooms;

public static class StringExtensions
{
    public static string ToRandomLetterCase(this string text) => ToLetterCase(text, RandomLetterCase());

    public static string ToLetterCase(this string text, LetterCaseMode mode) => mode switch
    {
        LetterCaseMode.Lower    => text.ToLower(),
        LetterCaseMode.Upper    => text.ToUpper(),
        LetterCaseMode.Sentence => char.ToUpper(text[0]) + text[1..].ToLower(),
        _ => text
    };

    private static LetterCaseMode RandomLetterCase() => Random.Shared.Next(8) switch
    {
        < 5 => LetterCaseMode.Lower,
        < 7 => LetterCaseMode.Sentence,
        _   => LetterCaseMode.Upper
    };

    public static string Quote(this string s) => $"\"{s}\"";
    
    public static string Truncate(this string s, int length) => s.Length > length ? s[..(length - 1)] + "…" : s;

    public static bool IsNullOrEmpty(this string? text) => string.IsNullOrEmpty(text);

    public static string ReplaceExtension(this string path, string newExtension)
    {
        return Regex.Replace(path, @"\.\S+$", newExtension);
    }

    // REGEX

    public static Match? MatchOrNull(this Regex regex, string? text) => text is null ? null : regex.Match(text);
}