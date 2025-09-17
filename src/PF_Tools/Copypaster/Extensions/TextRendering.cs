using System.Text;

namespace PF_Tools.Copypaster.Extensions;

public static class TextRendering
{
    private const string LINK = GenerationPack.S_REMOVED;
    private const string LINK_en = "[deleted]", LINK_ua = "[засекречено]", LINK_ru = "[ссылка удалена]";

    private const string ADD   = "[+]", ADD_Spaced = "[+] ";
    private static readonly Regex
        _r_random = new(@"\[\*(\d{1,9})..(\d{1,9})\]", RegexOptions.Compiled);

    public static string RenderText(this GenerationPack DB, LinkedList<int> ids)
    {
        var words = new LinkedList<string>();

        foreach (var id in ids)
        {
            var word = DB.GetWord(id);
            if (word is not null)
            {
                var match = _r_random.Match(word); // EXAMPLE: [*0..15928]
                if (match.Success)
                {
                    var a = Convert.ToInt32(match.Groups[1].Value);
                    var b = Convert.ToInt32(match.Groups[2].Value);
                    word = Fortune.RandomInt(a, b).ToString();
                }

                words.AddLast(word);
            }
        }

        return words.Count > 0 ? BuildText(words) : throw new Exception("Text wasn't generated");
    }

    private static string BuildText(LinkedList<string> words)
    {
        var text = new StringBuilder().AppendJoin(' ', words).ToString();

        if (words.Any(w => w.Equals(LINK)))
        {
            var replacement = text.IsMostlyCyrillic() ? text.LooksLikeUkrainian() ? LINK_ua : LINK_ru : LINK_en;
            text = text.Replace(LINK, replacement);
        }

        if (words.Any(w => w.EndsWith(ADD))) // "ab[+] cd" -> "abcd"
        {
            text = text.Replace(ADD_Spaced, "");
        }

        return text.ToRandomLetterCase();
    }
}