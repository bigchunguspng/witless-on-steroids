﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Witlesss.Generation;

namespace Witlesss
{
    public class Copypaster2
    {
        public  const string LINK = "[R]";
        private const string LINK_Spaced = $" {LINK} ";
        private const string START = "[S]", END = "[E]";
        private const string LINE_BREAK = "[N]", LINE_BREAK_Spaced = $" {LINE_BREAK} ";
        private const string LINK_en = "[deleted]", LINK_ua = "[засекречено]", LINK_ru = "[ссылка удалена]";

        private static readonly Regex _urls = new(@"(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)");
        private static readonly Regex _unacceptable = new(@"^(?:\/|\.)|^(?:(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>))$");

        public GenerationPack DB { get; set; } = new();


        // CONSUMING TEXT

        public bool Eat(string text, out string? eaten)
        {
            eaten = null!;

            if (string.IsNullOrWhiteSpace(text)) return false; // todo remove since it impossible on telegram
            if (_unacceptable.IsMatch(text)) return false;

            var lines = Tokenize(text);
            foreach (var line in lines)
            {
                EatInternal(Advance(line));
            }

            eaten = ""; // string.Join(' ', lines.Select(x => x.Replace(' ', '_')));
            return true;
        }

        private static string[][] Tokenize(string text)
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

        private static LinkedList<string> Advance(string[] words)
        {
            var tokens = new LinkedList<string>(words);
            CombineTokensByLength(tokens, 1);
            CombineTokensByLength(tokens, 2);
            CombineTokensByLength(tokens, 3);

            return tokens;
        }

        private static readonly Regex _regexA = new(@"[\s\[\]]|[.'?]$");
        private static readonly Regex _regexB = new(@"[\s\[\]]");

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

        private void EatInternal(LinkedList<string> words)
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
                    tokens.AddLast(new CombinedToken(id1, id2, idC));
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
                node.Value.RememberTransition(DB, next.Value);
                node = next;
            }
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
                var table = DB.GetTableByID(ids.Last.Value);
                ids.AddLast(PickWordID(table));
            }

            return RenderText(ids);
        }

        private string GenerateBackwards(int wordID)
        {
            var ids = new LinkedList<int>();

            ids.AddFirst(wordID);

            while (ids.First!.Value != GenerationPack.START)
            {
                var table = GetWordsBefore(ids.First.Value);
                ids.AddFirst(PickWordID(table));
            }

            return RenderText(ids);
        }

        public static int PickWordID(TransitionTable words)
        {
            var r = Random.Shared.NextSingle() * words.TotalChance;

            foreach (var transition in words)
            {
                if (transition.Chance > r) return transition.WordID;
                r -= transition.Chance;
            }

            LogError("GenerationPack.PickWordID >> UNEXPECTED EXECUTION PATH");

            return GenerationPack.END;
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
            var hasURLs = false;
            var sb = new StringBuilder();
            var word = words.First!;
            while (word is not null)
            {
                sb.Append(word.Value).Append(' ');

                hasURLs |= word.Value.Equals(LINK);

                word = word.Next;
            }

            var text = sb.ToString();
            if (hasURLs)
            {
                var replacement = IsMostlyCyrillic(text) ? LooksLikeUkrainian(text) ? LINK_ua : LINK_ru : LINK_en;
                text = text.Replace(LINK, replacement);
            }
            return text.ToRandomLetterCase();
        }

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

            // E lisba -> megalisba S lisba -> lisball
            if (HasWordsSimilarTo(word, backwards, out var words)) return RandomWordID(words);

            if (word.Length > 2 && backwards ? word.StartsWith("..") : word.EndsWith(".."))
            {
                // E ...ba -> booBA S lisb... -> LISBowski
                if (HasWordsSimilarTo(word.Trim('.'), backwards, out words)) return RandomWordID(words); 
            }
            if (word.Length > 3)
            {
                // E lisba -> so_SBA S lisba -> LISik
                var part = backwards ? word[^3..] : word.Remove(3);
                if (HasWordsSimilarTo(part, backwards, out words)) return RandomWordID(words); 
            }
            if (word.Length > 1)
            {
                // E lisba -> a S lisba -> lisb
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