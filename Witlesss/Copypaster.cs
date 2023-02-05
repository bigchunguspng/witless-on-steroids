﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WordChart = System.Collections.Concurrent.ConcurrentDictionary<string, float>;

namespace Witlesss
{
    public class Copypaster
    {
        public  const string   START = "_start",      END = "_end";
        private const string S_START = $"{START} ", S_END = $" {END}";
        private const string    LINK = "[ссылка удалена]", LF = "_LF", LF_Spaced = $" {LF} ";
        private readonly Regex _urls = new(@"\S+(:[\/\\])\S+");

        public Copypaster(Witless baka) => Baka = baka;

        private Witless Baka { get; }

        private WitlessDB Words => Baka.Words;
        
        
        public bool Eat(string text, out string eaten)
        {
            eaten = null;
            
            if (TextIsUnacceptable(text)) return false;
            
            var words = Tokenize(ReplaceLinks(text));
            int count = TokenCount();
            if (count < 14)
            {
                float weight = MathF.Round(1.4F - 0.1F * count, 1); // 1 => 1.3  |  5 => 0.9  |  13 => 0.1
                eaten = EatSimple(words, weight);
            }
            if (count > 1)
            {
                eaten = EatAdvanced(words);
            }
            
            Baka.HasUnsavedStuff = true;
            return true;

            bool TextIsUnacceptable(string s) => Regex.IsMatch(s, @"^(\/|\.)|^(\S+(:[\/\\])\S+)$");
            string ReplaceLinks(string s) => _urls.Replace(s, LINK);
            int TokenCount() => words.Length - words.Count(x => x == LF);
        }

        private string EatAdvanced(string[] words)
        {
            words = Advance(words);
            EatSimple(words);
            return string.Join(' ', words.Select(x => x.Replace(' ', '_')));
        }
        private string EatSimple(string[] words, float weight = 1F)
        {
            var list = new List<string>(words.Length + 2) { START };
            
            list.AddRange(words);
            list.Add(END);
            list.RemoveAll(x => x == LF);
            
            for (var i = 0; i < list.Count - 1; i++)
            {
                string word = list[i];
                if (!Words.ContainsKey(word)) Words.TryAdd(word, new WordChart());

                string next = list[i + 1];
                if (Words[word].ContainsKey(next))
                    Words[word][next] = MathF.Round(Words[word][next] + weight, 1);
                else
                    Words[word].TryAdd(next, weight);
            }
            return string.Join(' ', list.GetRange(1, list.Count - 2));
        }
        private string[] Advance(string[] words)
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
        private string[] Tokenize(string s) => s.ToLower().Replace("\n", LF_Spaced).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        public string GenerateByWord(string word)
        {
            string match = FindMatch(word, START, out bool separated);
            string result = Baka.Generate(match);
            if (separated) result = word.Split()[0] + " " + result;
            return result;
        }
        public string GenerateByLastWord(string word)
        {
            string match = FindMatch(word, END, out bool separated);
            string result = GenerateBackwards(match);
            if (separated) result = result + " " + word.Split()[1];
            return result;
        }

        private string FindMatch(string word, string alt, out bool separated)
        {
            separated = false;

            if (Words.ContainsKey(word)) return word;

            if (word.Contains(' '))
            {
                word = alt == END? word.Split()[0] : word.Split()[1];
                separated = true;
                if (Words.ContainsKey(word)) return word;
            }

            var words = Words.Keys.Where(KeyHasWord).ToList();
            if (words.Count > 0) return words[Extension.Random.Next(words.Count)];

            words     = Words.Keys.Where(WordHasKey).ToList();
            if (words.Count < 1) return alt;

            words.Sort(Comparison);
            return words[0];

            int  Comparison(string x, string y) => y.Length - x.Length;
            bool WordHasKey(string x) => alt == END ? word.EndsWith(x, StringComparison.Ordinal) : word.StartsWith(x, StringComparison.Ordinal);
            bool KeyHasWord(string x) => alt == END ? x.EndsWith(word, StringComparison.Ordinal) : x.StartsWith(word, StringComparison.Ordinal);
        }

        public  string Generate(string word)
        {
            string result = "";
            string current = word;

            while (current != END)
            {
                result = result + " " + current;
                current = PickWord(Words[current]);
            }

            return TextInRandomLetterCase(result.Replace(S_START, ""));
        }
        private string GenerateBackwards(string word)
        {
            string result = "";
            string current = word;
            
            while (current != START)
            {
                result = current + " " + result;
                current = PickWord(GetWordsBefore(current));
            }

            return TextInRandomLetterCase(result.Replace(S_END, ""));
        }

        private WordChart GetWordsBefore(string word)
        {
            var words = new WordChart();
            foreach (var bunch in Words)
            {
                if (bunch.Value.ContainsKey(word))
                {
                    float x = bunch.Value[word];
                    if (!words.TryAdd(bunch.Key, x)) words[bunch.Key] += x;
                }
            }
            return words;
        }

        public static string PickWord(WordChart words)
        {
            float r = (float) Extension.Random.NextDouble() * words.Sum(chance => chance.Value);

            foreach (var chance in words)
            {
                if  (chance.Value > r) return chance.Key;
                r -= chance.Value;
            }

            return END;
        }
        
        public void FixWitlessDB()
        {
            foreach (var word in Words)
            foreach (string next in word.Value.Keys)
            {
                if (!Words.ContainsKey(next))
                {
                    Words.TryAdd(next, new WordChart());
                    Words[next].TryAdd(END, 1F);
                }
            }
        }
    }
}