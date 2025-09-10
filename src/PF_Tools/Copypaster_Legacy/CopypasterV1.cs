using System.Diagnostics.CodeAnalysis;
using System.Text;
using PF_Tools.Backrooms.Helpers;
using PF_Tools.Copypaster_Legacy.Pack;
using PF_Tools.Copypaster_Legacy.Tokens;

namespace PF_Tools.Copypaster_Legacy
{
    public class CopypasterV1
    {
        private const string START = "[S]", END = "[E]";
        private const string LINE_BREAK = "[N]", LINE_BREAK_Spaced = $" {LINE_BREAK} ";
        private const string ADD = "[+]", ADD_Spaced = "[+] ";
        private const string LINK = GenerationPack.S_REMOVED, LINK_Spaced = $" {LINK} ";
        private const string LINK_en = "[deleted]", LINK_ua = "[засекречено]", LINK_ru = "[ссылка удалена]";

        private static readonly Regex _urls = new(            @"(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)",  RegexOptions.Compiled);
        private static readonly Regex _skip = new(@"^(?:\/|\.)|^(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)$", RegexOptions.Compiled);
        private static readonly Regex _random = new(@"\[\*(\d{1,9})..(\d{1,9})\]", RegexOptions.Compiled);

        public GenerationPack DB { get; set; } = new();


        // CONSUMING TEXT

        public bool Eat(string text, [NotNullWhen(true)] out string[]? eaten, float chance = 0)
        {
            eaten = null;

            if (_skip.IsMatch(text)) return false;

            var lines = TokenizeMultiline(text);

            eaten = new string[lines.Length];

            for (var i = 0; i < lines.Length; i++)
            {
                var tokens = new LinkedList<string>(lines[i]);
                if (chance == 0)
                    chance = Math.Max(0.3F, MathF.Round(2 - MathF.Log10(tokens.Count), 1));

                CombineSomeTokens(tokens);
                EatInternal(tokens, chance);

                eaten[i] = string.Join(' ', tokens.Select(word => word.Replace(' ', '_')));
            }

            return true;
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

        private static void CombineSomeTokens(LinkedList<string> tokens)
        {
            CombineTokensByLength(tokens, 1);
            CombineTokensByLength(tokens, 2);
            CombineTokensByLength(tokens, 3);
        }

        private static readonly Regex _regexA = new(@"[ \]]|[.!?]$", RegexOptions.Compiled);
        private static readonly Regex _regexB = new(@"[ \]]",        RegexOptions.Compiled);

        private static void CombineTokensByLength(LinkedList<string> tokens, int length)
        {
            var token = tokens.First!;
            while (token.Next is { } next)
            {
                var a = token.Value;
                var b = token.Next.Value;

                if (!_regexA.IsMatch(a) && !_regexB.IsMatch(b) && a.Length == length)
                {
                    token.Value = $"{token.Value} {next.Value}";
                    tokens.Remove(next);
                }
                if (token.Next is null) break;
                token = token.Next;
            }
        }

        private void EatInternal(LinkedList<string> words, float chance)
        {
            // update vocabulary
            var tokens = new LinkedList<IConsumableToken>();
            foreach (var word in words)
            {
                // words: A, B, [R], [N], C
                if (word == LINE_BREAK) continue;
                if (word.Contains(' '))
                {
                    var index = word.IndexOf(' ');
                    var id1 = DB.GetOrAddWord_ReturnID(word.Remove(index));
                    var id2 = DB.GetOrAddWord_ReturnID(word.Substring(index + 1));
                    var idC = DB.GetOrAddWord_ReturnID(word);
                    tokens.AddLast(new DoubleToken(id1, id2, idC));
                }
                else
                    tokens.AddLast(new SingleToken(DB.GetOrAddWord_ReturnID(word)));
            }

            // update transitions
            tokens.AddFirst(new SingleToken(GenerationPack.START));
            tokens.AddLast (new SingleToken(GenerationPack.END));

            var node = tokens.First!;
            while (node.Next is { } next)
            {
                // ids: -5, A, B, -8, C, -3
                node.Value.RememberTransition(DB, next.Value, chance);
                node = next;
            }
        }

        // FUSE

        public void Fuse(GenerationPack other)
        {
            // update vocabulary
            var ids = other.Vocabulary.Select(word => DB.GetOrAddWord_ReturnID(word)).ToList();

            // update transitions
            foreach (var otherTable in other.Transitions)
            {
                var tableID = GetNewID(otherTable.Key);
                foreach (var transition in otherTable.Value.AsIEnumerable())
                {
                    var wordID = GetNewID(transition.WordID);
                    DB.GetOrAddTable(tableID).PutMax(wordID, transition.Chance);
                }
            }

            int GetNewID(int id) => id < 0 ? id : ids[id];
        }


        // GENERATION

        // Vocabulary: 0+

        // A: table [B: Chance]
        // A = [S], [R], 0+
        // B = [E], [R], 0+

        /// <param name="word">A user provided text, <b>\S+(\s\S+)?</b>.</param>
        public string GenerateByWord(string word)
        {
            var id = FindExistingOrSimilarWordID(word, START, out var separated);
            var result = Generate(id);
            if (separated) result = word.Split()[0] + " " + result;
            return result;
        }

        /// <param name="word">A user provided text, <b>\S+(\s\S+)?</b>.</param>
        public string GenerateByLast(string word)
        {
            var id = FindExistingOrSimilarWordID(word, END, out var separated);
            var result = GenerateBackwards(id);
            if (separated) result = result + " " + word.Split()[1];
            return result;
        }

        public string Generate(int wordID = GenerationPack.START)
        {
            var ids = new LinkedList<int>();

            ids.AddLast(wordID);

            while (ids.Last!.Value != GenerationPack.END)
            {
                var table = DB.Transitions[ids.Last.Value];
                ids.AddLast(PickWordID(table));
            }

            return RenderText(ids);
        }

        public string GenerateBackwards(int wordID = GenerationPack.END)
        {
            if (wordID == GenerationPack.END)
                wordID = GetRandomLastWordID();

            var ids = new LinkedList<int>();

            ids.AddFirst(wordID);

            while (ids.First!.Value != GenerationPack.START)
            {
                var table = GetWordsBefore(ids.First.Value);
                ids.AddFirst(PickWordID(table, fallback: GenerationPack.START));
            }

            return RenderText(ids);
        }

        private int GetRandomLastWordID()
        {
            for (var i = 0; i < 12; i++)
            {
                var table = DB.Transitions.ElementAt(Random.Shared.Next(DB.Transitions.Count));
                if (table.Value.AsIEnumerable().Any(x => x.WordID == GenerationPack.END)) return table.Key;
            }

            return -1;
        }

        private static int PickWordID(TransitionTable table, int fallback = GenerationPack.END)
        {
            var r = Random.Shared.NextSingle() * table.TotalChance;

            if (r > 0F)
            {
                foreach (var transition in table.AsIEnumerable())
                {
                    if (transition.Chance > r) return transition.WordID;
                    r -= transition.Chance;
                }

                throw new Exception("UNEXPECTED EXECUTION PATH");
            }

            return fallback;
        }

        private string RenderText(LinkedList<int> ids)
        {
            var words = new LinkedList<string>();

            foreach (var id in ids)
            {
                var word = DB.GetWordByID(id);
                if (word is not null)
                {
                    var match = _random.Match(word); // EXAMPLE: [*0..15928]
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

        private string BuildText(LinkedList<string> words)
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

        /// <summary>
        /// Finds all word ids, that has provided word id in their transitions
        /// </summary>
        private TransitionTable GetWordsBefore(int wordID)
        {
            var table = new TransitionTableLarge();
            foreach (var pair in DB.Transitions)
            {
                var index = pair.Value.FindIndexByID(wordID);
                if (index > 0)
                {
                    table.Put(pair.Key, pair.Value[index].Chance);
                }
            }
            return table;
        }

        private int FindExistingOrSimilarWordID(string word, string alt, out bool separated)
        {
            if (DB.Vocabulary.Count == 0) throw new Exception("бро так не генерят");

            var backwards = alt == END;
            separated = false;

            var id = DB.GetID_ByWord(word);
            if (id != GenerationPack.NO_WORD)
            {
                return id;
            }

            // word is not a part of the vocabulary

            if (word.Contains(' '))
            {
                word = word.Split()[backwards ? 0 : 1];
                separated = true;

                id = DB.GetID_ByWord(word);
                if (id != GenerationPack.NO_WORD) return id;

                // word is not a part of the vocabulary + is separated by space;
            }

            // S lisba ->     [lisba]ngelion
            // E lisba -> mega[lisba]
            if (HasWordsSimilarTo(word, backwards, out var words))
            {
                return RandomWordID(words);
            }

            if (word.Length > 2 && backwards ? word.StartsWith("..") : word.EndsWith(".."))
            {
                // S lisb.. -> [lisb]owski
                // E ...ba  -> boo[ba]
                if (HasWordsSimilarTo(word.Trim('.'), backwards, out words))
                {
                    return RandomWordID(words);
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
                    if (HasWordsSimilarTo(part, backwards, out words))
                    {
                        return RandomWordID(words);
                    }
                }
            }

            if (word.Length > 1)
            {
                // S lisba -> lisb
                // E lisba -> a
                if (HasWordsSimilarTo(word, backwards, out words, normalWay: false)) 
                {
                    return DB.GetID_ByWord(words.First(x => x.Length == words.Max(s => s.Length)));
                }
            }

            return backwards ? GenerationPack.END : GenerationPack.START;
        }

        private bool HasWordsSimilarTo(string word, bool backwards, out List<string> words, bool normalWay = true)
        {
            words = DB.Vocabulary.Where(normalWay ? WordIsPartOfDBWord : DBWordIsPartOfWord).ToList();
            return words.Count > 0;
            
            bool DBWordIsPartOfWord(string x) => backwards 
                ? word.  EndsWith(x, StringComparison.Ordinal) 
                : word.StartsWith(x, StringComparison.Ordinal);

            bool WordIsPartOfDBWord(string x) => backwards 
                ? x.  EndsWith(word, StringComparison.Ordinal) 
                : x.StartsWith(word, StringComparison.Ordinal);
        }

        private int RandomWordID(List<string> words) => DB.GetID_ByWord(words[Random.Shared.Next(words.Count)]);
    }
}