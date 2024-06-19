using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Witlesss.Generation;
using WordChart = System.Collections.Generic.Dictionary<string, float>;

namespace Witlesss
{
    public class Copypaster2
    {
        public  const string   START = "_start",      END = "_end";
        private const string    LINK = "[ссылка удалена]", LINK_eng = "[deleted]", LINK_ua = "[видалено]";
        private const string      LF = "_LF", LF_Spaced = $" {LF} ";

        private static readonly Regex _urls = new(@"\S+(:[\/\\])\S+");
        private static readonly Regex _unacceptable = new(@"^(\/|\.)|^(\S+(:[\/\\])\S+)$");

        public GenerationPack DB { get; set; } = new();


        // CONSUMING TEXT

        public bool Eat(string text, out string? eaten)
        {
            eaten = null!;

            if (string.IsNullOrWhiteSpace(text)) return false;
            if (TextIsUnacceptable(text)) return false;

            var words = Tokenize(ReplaceLinks(text));
            int count = TokenCount();
            if (count < 14)
            {
                float weight = MathF.Round(1.4F - 0.1F * count, 1); // 1 => 1.3  |  5 => 0.9  |  13 => 0.1
                EatSimple(words, weight);
                eaten = string.Join(' ', words);
            }
            if (count > 1)
            {
                eaten = EatAdvanced(words);
            }
            return true;

            bool TextIsUnacceptable(string s) => _unacceptable.IsMatch(s);
            string ReplaceLinks(string s) => _urls.Replace(s, LINK);
            int TokenCount() => words.Length - words.Count(x => x == LF);
        }

        private string EatAdvanced(string[] words)
        {
            words = Advance(words);
            EatSimple(words);
            return string.Join(' ', words.Select(x => x.Replace(' ', '_')));
        }

        private void EatSimple(string[] words, float weight = 1F)
        {
            // words -> ids
            var ids = new LinkedList<int>();
            foreach (var word in words)
            {
                if (word != LF) ids.AddLast(DB.GetWordID(word));
            }

            ids.AddFirst(GenerationPack.START);
            ids.AddLast(GenerationPack.END);

            // add transitions data
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

            if (tokens.Contains(LF))
            {
                var indexes = tokens.Select((t, i) => new {t, i}).Where(x => x.t == LF).Select(x => x.i).ToArray();
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
        private static string[] Tokenize(string s) => s.ToLower().Replace("\n", LF_Spaced).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);


        // GENERATION

        public string GenerateByWord(string word)
        {
            var match = FindMatch(word, START, out bool separated);
            var result = Generate(match);
            if (separated) result = word.Split()[0] + " " + result;
            return result;
        }

        public string GenerateByLast(string word)
        {
            var match = FindMatch(word, END, out bool separated);
            var result = GenerateBackwards(match);
            if (separated) result = result + " " + word.Split()[1];
            return result;
        }

        // returns word id
        private int FindMatch(string word, string alt, out bool separated)
        {
            throw new NotImplementedException();
            /*if (DB.Vocabulary.Count == 0) throw new Exception("бро так не генерят");

            separated = false;

            if (DB.HasWord(word)) return word;

            if (word.Contains(' '))
            {
                word = alt == END ? word.Split()[0] : word.Split()[1];
                separated = true;
                if (DB.HasWord(word)) return word;
            }

            var w = word;
            var words = DB.Vocabulary.Where(KeyHasWord).ToList(); // E lisba -> megalisba S lisba -> lisball
            if (words.Count > 0) return RandomWord();

            if (word.Length > 2 && alt == END ? word.StartsWith("..") : word.EndsWith(".."))
            {
                w = word.Trim('.');
                words = DB.Vocabulary.Where(KeyHasWord).ToList(); // E ...ba -> booBA S lisb... -> LISBowski
                if (words.Count > 0) return RandomWord();
            }
            if (word.Length > 3)
            {
                w = alt == END ? word[^3..] : word.Remove(3);
                words = DB.Vocabulary.Where(KeyHasWord).ToList(); // E lisba -> so_SBA S lisba -> LISik
                if (words.Count > 0) return RandomWord();
            }
            if (word.Length > 1)
            {
                w = word;
                words = DB.Vocabulary.Where(WordHasKey).ToList(); // E lisba -> a S lisba -> lisb
                if (words.Count > 0)
                {
                    return words.First(x => x.Length == words.Max(s => s.Length));
                }
            }
            return alt;

            string RandomWord() => words[Random.Shared.Next(words.Count)];

            bool WordHasKey(string x) => alt == END ? w.EndsWith(x, StringComparison.Ordinal) : w.StartsWith(x, StringComparison.Ordinal);
            bool KeyHasWord(string x) => alt == END ? x.EndsWith(w, StringComparison.Ordinal) : x.StartsWith(w, StringComparison.Ordinal);*/
        }

        public string Generate(int wordID = GenerationPack.START)
        {
            // generate int sequence

            var ids = new LinkedList<int>();

            ids.AddLast(wordID);

            while (ids.Last!.Value != GenerationPack.END)
            {
                ids.AddLast(PickWord(DB.GetTableByID(ids.Last.Value)));
            }

            if (ids.First is { Value: GenerationPack.START }) ids.RemoveFirst();

            // get words

            var words = new LinkedList<string>();

            foreach (var id in ids)
            {
                var word = DB.GetWordByID(id);
                if (word is not null) words.AddLast(word);
            }

            return RenderText(words);
        }
        private string GenerateBackwards(int word)
        {
            throw new NotImplementedException();/*
            var tokens = new LinkedList<string>();

            string current = word;
            while (current != START)
            {
                tokens.AddFirst(current);
                current = PickWord(GetWordsBefore(current));
            }

            if (tokens.Last is { Value: END }) tokens.RemoveLast();
            
            return CleanMess(tokens);*/
        }

        private string RenderText(LinkedList<string> words)
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

        private string CleanMess(LinkedList<string> tokens)
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
        }

        private WordChart GetWordsBefore(string word)
        {
            throw new NotImplementedException();
            /*var words = new WordChart();
            foreach (var bunch in DB)
            {
                if (bunch.Value.TryGetValue(word, out float x))
                {
                    if (!words.TryAdd(bunch.Key, x)) words[bunch.Key] += x;
                }
            }
            return words;*/
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

        /*public void FixWitlessDB()
        {
            foreach (var word in DB)
            foreach (string next in word.Value.Keys)
            {
                if (!DB.ContainsKey(next))
                {
                    DB.Add(next, new WordChart());
                    DB[next].Add(END, 1F);
                }
            }
        }*/
    }
}