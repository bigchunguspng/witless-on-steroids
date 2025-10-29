using System.Text;

namespace PF_Tools.Copypaster.Extensions;

public static class TextRendering
{
    private const string LINK = GenerationPack.S_REMOVED;
    private const string LINK_en = "[deleted]", LINK_ua = "[засекречено]", LINK_ru = "[ссылка удалена]";

    public static string RenderText(this GenerationPack DB, LinkedList<int> ids)
    {
        var words = new LinkedList<string>();

        foreach (var id in ids)
        {
            var word = DB.GetWord(id);
            if (word is not null)
            {
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

        return text.ToRandomLetterCase();
    }
}