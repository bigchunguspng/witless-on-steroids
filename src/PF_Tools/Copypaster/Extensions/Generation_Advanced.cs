namespace PF_Tools.Copypaster.Extensions;

public static class Generation_Advanced
{
    private const string START = "[S]", END = "[E]";

    // GENERATION

    // Vocabulary: 0+

    // A: table [B: Chance]
    // A = [S], [R], 0+
    // B = [E], [R], 0+

    public static string GenerateByWord(this GenerationPack pack, string word)
    {
        var id = pack.FindExistingOrSimilarWord(word, START, out var separated);
        var ids = pack.Generate(id);
        var result = pack.RenderText(ids);
        if (separated) result = $"{word.SliceByFirst(' ')} {result}";
        return result;
    }

    public static string GenerateByLast(this GenerationPack pack, string word)
    {
        var id = pack.FindExistingOrSimilarWord(word, END, out var separated);
        var ids = pack.GenerateBackwards(id);
        var result = pack.RenderText(ids);
        if (separated) result = result + " " + word.Split()[1];
        return result;
    }

    private static int FindExistingOrSimilarWord(this GenerationPack pack, string word, string alt, out bool separated)
    {
        if (pack.VocabularyCount == 0) throw new Exception("бро так не генерят");

        var backwards = alt == END;
        separated = false;

        var id = pack.GetWordId(word);
        if (id != GenerationPack.NO_WORD)
        {
            return id;
        }

        // word is not a part of the vocabulary

        if (word.Contains(' '))
        {
            word = word.Split()[backwards ? 0 : 1];
            separated = true;

            id = pack.GetWordId(word);
            if (id != GenerationPack.NO_WORD) return id;

            // word is not a part of the vocabulary + is separated by space;
        }

        // S lisba ->     [lisba]ngelion
        // E lisba -> mega[lisba]
        if (pack.HasWordsSimilarTo(word, backwards, out var words))
        {
            return pack.GetWordId(words.PickAny());
        }

        if (word.Length > 2 && backwards ? word.StartsWith("..") : word.EndsWith(".."))
        {
            // S lisb.. -> [lisb]owski
            // E ...ba  -> boo[ba]
            if (pack.HasWordsSimilarTo(word.Trim('.'), backwards, out words))
            {
                return pack.GetWordId(words.PickAny());
            }
        }

        if (word.Length > 1)
        {
            // S lisba -> [lis]ik     [li]thium    [l]mao
            // E lisba -> so_[sba]    yo[ba]       sigm[a]
            var start = Math.Min(word.Length - 1, 3);
            for (var i = start; i >= 1; i--)
            {
                var part = backwards ? word[^i..] : word.Remove(i);
                if (pack.HasWordsSimilarTo(part, backwards, out words))
                {
                    return pack.GetWordId(words.PickAny());
                }
            }
        }

        if (word.Length > 1)
        {
            // S lisba -> lisb
            // E lisba -> a
            if (pack.HasWordsSimilarTo(word, backwards, out words, normalWay: false)) 
            {
                return pack.GetWordId(words.First(x => x.Length == words.Max(s => s.Length)));
            }
        }

        return backwards ? GenerationPack.END : GenerationPack.START;
    }

    private static bool HasWordsSimilarTo(this GenerationPack pack, string word, bool backwards, out List<string> words, bool normalWay = true)
    {
        words = pack.Vocabulary.Where(normalWay ? WordIsPartOfDBWord : DBWordIsPartOfWord).ToList();
        return words.Count > 0;
            
        bool DBWordIsPartOfWord(string x) => backwards 
            ? word.  EndsWith(x, StringComparison.Ordinal) 
            : word.StartsWith(x, StringComparison.Ordinal);

        bool WordIsPartOfDBWord(string x) => backwards 
            ? x.  EndsWith(word, StringComparison.Ordinal) 
            : x.StartsWith(word, StringComparison.Ordinal);
    }
}