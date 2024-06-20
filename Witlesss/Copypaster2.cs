using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Witlesss.Generation;

namespace Witlesss
{
    public class Copypaster2
    {
        private const string START = "[S]", END = "[E]";
        private const string LINK = "[R]";
        private const string LINK_en = "[deleted]", LINK_ua = "[засекречено]", LINK_ru = "[ссылка удалена]";
        private const string LINE_BREAK = "[N]", LINE_BREAK_Spaced = $" {LINE_BREAK} ";

        private static readonly Regex _urls = new(@"\S+(:[\/\\])\S+");
        private static readonly Regex _unacceptable = new(@"^(\/|\.)|^(\S+(:[\/\\])\S+)$");

        public GenerationPack DB { get; set; } = new();


        // CONSUMING TEXT

        public bool Eat(string text, out string? eaten)
        {
            eaten = null!;

            if (string.IsNullOrWhiteSpace(text)) return false; // todo remove since it impossible on telegram
            if (_unacceptable.IsMatch(text)) return false;

            var words = Tokenize(text);
            var tokenCount = words.Length - words.Count(x => x == LINE_BREAK);
            if (tokenCount < 14) EatInternal(words, GetChance(tokenCount));
            if (tokenCount > 01) EatInternal(Advance(words));

            eaten = string.Join(' ', words.Select(x => x.Replace(' ', '_')));
            return true;
        }

        private static string[] Tokenize(string text)
            => _urls.Replace(text, LINK)
                .ToLower().Replace("\n", LINE_BREAK_Spaced).Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // tokenCount:  1 => 1.3  |  5 => 0.9  |  13 => 0.1
        private float GetChance(int tokenCount) => MathF.Round(1.4F - 0.1F * tokenCount, 1);

        private void EatInternal(string[] words, float weight = 1F)
        {
            // update vocabulary
            var ids = new LinkedList<int>();
            foreach (var word in words)
            {
                if (word != LINE_BREAK) ids.AddLast(DB.GetWordID_AddNew(word));
            }

            // update transitions
            ids.AddFirst(GenerationPack.START);
            ids.AddLast(GenerationPack.END);

            var id = ids.First!;
            while (id.Next is { } next)
            {
                DB.GetTableByID(id.Value).Put(next.Value, weight);
                id = next;
            }
        }

        private static string[] Advance(string[] words)
        {
            var tokens = new LinkedList<string>(words);

            if (tokens.Contains(LINE_BREAK))
            {
                var indexes = tokens.Select((t, i) => new {t, i}).Where(x => x.t == LINE_BREAK).Select(x => x.i).ToArray();
                var list = new List<string[]>(indexes.Length + 1);
                var toks = tokens.ToArray();
                var a = 0;
                foreach (int index in indexes)
                {
                    if (a == index)
                    {
                        a++;
                        continue;
                    }
                    list.Add(toks[a..index]);
                    a = index + 1;
                }
                list.Add(toks[a..tokens.Count]);
                tokens.Clear();
                for (var i = 0; i < list.Count; i++)
                {
                    list[i] = Advance(list[i]);
                    foreach (string token in list[i])
                    {
                        tokens.AddLast(token);
                    }
                }
                return tokens.ToArray();
            }
            
            UniteTokensToRight(1, 3, 20);
            UniteTokensToRight(2, 2, 20);
            UniteTokensToLeft (2, 2, 5);
            UniteTokensToRight(3, 3, 4);
            UniteTokensToRight(4, 2, 5);

            return tokens.ToArray();

            IEnumerable<string> SmallWordsSkipLast (int length) => tokens.SkipLast(1).Where(x => UnitableToken(x, length)).Reverse();
            IEnumerable<string> SmallWordsSkipFirst(int length) => tokens.Skip    (1).Where(x => UnitableToken(x, length)).Reverse();
            
            bool UnitableToken(string x, int length) => x.Length == length;
            bool IsSimpleToken(string x) => !x.Contains(' ');

            void UniteTokensToRight(int length, int min, int max)
            {
                var small = SmallWordsSkipLast(length).ToArray();
                if (small.Length == 0) return;
                    
                foreach (string word in small)
                {
                    var x = tokens.Last;
                    tokens.RemoveLast();
                    var a = tokens.FindLast(word);
                    tokens.AddLast(x!);
                    var n = a?.Next;
                    var l = n?.Value.Length;
                    if (l >= min && l <= max && IsSimpleToken(n.Value))
                    {
                        a!.Value = a.Value + " " + n.Value;
                        tokens.Remove(n);
                    }
                }
            }
            void UniteTokensToLeft (int length, int min, int max)
            {
                var small = SmallWordsSkipFirst(length).ToArray();
                if (small.Length == 0) return;

                foreach (string word in small)
                {
                    var x = tokens.First;
                    tokens.RemoveFirst();
                    var a = tokens.FindLast(word);
                    tokens.AddFirst(x!);
                    var p = a?.Previous;
                    var l = p?.Value.Length;
                    if (l >= min && l <= max && IsSimpleToken(p.Value))
                    {
                        a!.Value = p.Value + " " + a.Value;
                        tokens.Remove(p);
                    }
                }
            }
        }


        // GENERATION

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

        private int FindExistingOrSimilarWordID(string word, string alt, out bool separated)
        {
            if (DB.Vocabulary.Count == 0) throw new Exception("бро так не генерят");

            var backwards = alt == END;
            separated = false;

            var id = DB.GetID_ByWord(word);
            if (id != GenerationPack.NO_WORD) return id;

            // word is not a part of the vocabulary

            if (word.Contains(' '))
            {
                word = word.Split()[backwards ? 0 : 1];
                separated = true;

                id = DB.GetID_ByWord(word);
                if (id != GenerationPack.NO_WORD) return id;

                // word is not a part of the vocabulary + is separated by space;
            }

            var w = word;
            List<string> words;
            if (GotEnoughWords()) return RandomWordID(); // E lisba -> megalisba S lisba -> lisball

            if (word.Length > 2 && backwards ? word.StartsWith("..") : word.EndsWith(".."))
            {
                w = word.Trim('.');
                if (GotEnoughWords()) return RandomWordID(); // E ...ba -> booBA S lisb... -> LISBowski
            }
            if (word.Length > 3)
            {
                w = backwards ? word[^3..] : word.Remove(3);
                if (GotEnoughWords()) return RandomWordID(); // E lisba -> so_SBA S lisba -> LISik
            }
            if (word.Length > 1)
            {
                w = word;
                if (GotEnoughWords(normalWay: false)) // E lisba -> a S lisba -> lisb
                {
                    return DB.GetID_ByWord(words.First(x => x.Length == words.Max(s => s.Length)));
                }
            }

            return backwards ? GenerationPack.END : GenerationPack.START;

            // ==

            int RandomWordID() => DB.GetID_ByWord(words[Random.Shared.Next(words.Count)]);

            bool GotEnoughWords(bool normalWay = true)
            {
                words = DB.Vocabulary.Where(normalWay ? WordIsPartOfDBWord : DBWordIsPartOfWord).ToList();
                return words.Count > 0;
            }

            bool DBWordIsPartOfWord(string x) => backwards 
                ? w.  EndsWith(x, StringComparison.Ordinal) 
                : w.StartsWith(x, StringComparison.Ordinal);

            bool WordIsPartOfDBWord(string x) => backwards 
                ? x.  EndsWith(w, StringComparison.Ordinal) 
                : x.StartsWith(w, StringComparison.Ordinal);
        }

        public string Generate(int wordID = GenerationPack.START)
        {
            var ids = new LinkedList<int>();

            ids.AddLast(wordID);

            while (ids.Last!.Value != GenerationPack.END)
            {
                ids.AddLast(PickWord(DB.GetTableByID(ids.Last.Value)));
            }

            if (ids.First is { Value: GenerationPack.START }) ids.RemoveFirst();

            return RenderText(ids);
        }

        private string GenerateBackwards(int wordID)
        {
            var ids = new LinkedList<int>();

            ids.AddFirst(wordID);

            while (ids.First!.Value != GenerationPack.START)
            {
                ids.AddFirst(PickWord(GetWordsBefore(ids.First.Value)));
            }

            if (ids.Last is { Value: GenerationPack.END }) ids.RemoveLast();

            return RenderText(ids);
        }

        private string RenderText(LinkedList<int> ids)
        {
            var words = new LinkedList<string>();

            foreach (var id in ids)
            {
                var word = DB.GetWordByID(id);
                if (word is not null) words.AddLast(word);
            }

            return BuildText(words);
        }

        private string BuildText(LinkedList<string> words)
        {
            var sb = new StringBuilder();
            var word = words.First!;
            while (true)
            {
                sb.Append(word.Value.Equals("[R]") ? LINK : word.Value);
                if (word.Next is null) return sb.ToString().ToRandomLetterCase();
                else sb.Append(' ');

                word = word.Next;
            }
        }

        /*private string CleanMess(LinkedList<string> tokens)
        {
            return LocalizeLinkRemovals(string.Join(' ', tokens)).ToRandomLetterCase();
        }
        private string LocalizeLinkRemovals(string text)
        {
            if (!text.Contains(LINK)) return text;

            var temp = text.Replace(LINK, "_+-+_");

            var cyr = IsMostlyCyrillic(temp);
            var ukr = cyr && LooksLikeUkrainian(temp, out var sure) && sure;

            return cyr && !ukr ? text : text.Replace(LINK, ukr ? LINK_ua : LINK_eng);
        }*/

        /// <summary>
        /// Finds all word ids, that has provided word id in their transitions
        /// </summary>
        private TransitionTable GetWordsBefore(int wordID)
        {
            var table = new TransitionTable();
            foreach (var pair in DB.Transitions)
            {
                var index = pair.Value.FindIndex(x => x.WordID == wordID);
                if (index > 0)
                {
                    table.Put(pair.Key, pair.Value[index].Chance);
                }
            }
            return table;
        }

        public static int PickWord(TransitionTable words)
        {
            var r = Random.Shared.NextSingle() * words.TotalChance;

            foreach (var transition in words)
            {
                // bug: [transition.Chance > r] is often not reached
                // r -= may be not accurate
                if (transition.Chance > r) return transition.WordID;
                r -= transition.Chance;
            }

            return GenerationPack.END;
        }
    }
}