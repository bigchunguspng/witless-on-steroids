namespace PF_Tools.Copypaster.Extensions;

public static class Consuming_Advanced
{
    private const string LINE_BREAK = "[N]", LINE_BREAK_Spaced = $" {LINE_BREAK} ";
    private const string LINK_Spaced = $" {GenerationPack.S_REMOVED} ";

    private static readonly Regex _urls = new(            @"(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)",  RegexOptions.Compiled);
    private static readonly Regex _skip = new(@"^(?:\/|\.)|^(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)$", RegexOptions.Compiled);

    // LOGIC

    public static string[]? Eat_Advanced(this GenerationPack pack, string text, float chance = 0)
    {
        if (_skip.IsMatch(text)) return null;

        var lines = TokenizeMultiline(text);

        var result = new string[lines.Length];

        for (var i = 0; i < lines.Length; i++)
        {
            var tokens = new LinkedList<string>(lines[i]);
            if (chance == 0)
                chance = Math.Max(0.3F, MathF.Round(2 - MathF.Log10(tokens.Count), 1));

            tokens.CombineSomeTokens();
            tokens.RemoveLineBreaks();
            pack.Eat(tokens, chance);

            result[i] = string.Join(' ', tokens.Select(word => word.Replace(' ', '_')));
        }

        return result; // todo replace string[] accol with smth else - return ids for example and use console render
    }

    private static string[][] TokenizeMultiline(string text)
    {
        text = _urls.Replace(text.ToLower(), LINK_Spaced);
        return text.Contains("\n\n")
            ? text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries).Select(TokenizeLine).ToArray()
            : [TokenizeLine(text)];
    }

    private static string[] TokenizeLine(string text)
        => text
            .Trim([' ', '\n']).Replace("\n", LINE_BREAK_Spaced)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static void CombineSomeTokens(this LinkedList<string> tokens)
    {
        tokens.CombineByLength(1);
        tokens.CombineByLength(2);
        tokens.CombineByLength(3);
    }

    private static readonly Regex _regexA = new(@"[ \]]|[.!?]$", RegexOptions.Compiled);
    private static readonly Regex _regexB = new(@"[ \]]",        RegexOptions.Compiled);

    private static void CombineByLength(this LinkedList<string> tokens, int length)
    {
        var token = tokens.First!;
        while (token.Next is { } next)
        {
            var a = token.Value;
            var b =  next.Value;

            if (a.Length == length && !_regexA.IsMatch(a) && !_regexB.IsMatch(b))
            {
                token.Value = $"{a} {b}";
                tokens.Remove(next);
            }

            if (token.Next != null)
                token = token.Next;
            else
                break;
        }
    }

    private static void RemoveLineBreaks(this LinkedList<string> tokens)
    {
        var token = tokens.First!;
        while (true)
        {
            if (token.Value == LINE_BREAK)
                tokens.Remove(token);

            if (token.Next != null)
                token = token.Next;
            else
                break;
        }
    }
}