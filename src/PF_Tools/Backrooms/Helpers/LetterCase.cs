namespace PF_Tools.Backrooms.Helpers;

// TODO cleanup + BTFO
// destination: bot/core/text
// reason: rigged random, used only in text & memes

public enum LetterCase
{
    Lower, Upper, Sentence,
}

public static class LetterCaseHelpers
{
    public static string ToRandomLetterCase(this string text) => InLetterCase(text, GetRandomLetterCase());

    public static string InLetterCase(this string text, LetterCase mode) => mode switch
    {
        LetterCase.Lower    => text.ToLower(),
        LetterCase.Upper    => text.ToUpper(),
        LetterCase.Sentence => char.ToUpper(text[0]) + text[1..].ToLower(),
        _ => text,
    };

    public static LetterCase GetRandomLetterCase() => Random.Shared.Next(8) switch
    {
        < 5 => LetterCase.Lower,
        < 7 => LetterCase.Sentence,
        _   => LetterCase.Upper,
    };

    public static LetterCase GetUpperOrLowerLetterCase() => Fortune.IsOneIn(2) ? LetterCase.Upper : LetterCase.Lower;

}