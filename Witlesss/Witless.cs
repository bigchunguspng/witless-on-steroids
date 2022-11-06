using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        public const string START = "_start", END = "_end", LINK = "[ссылка удалена]", LF = "_LF", LF_Spaced = " " + LF + " ";

        private int _probability, _quality;
        private bool _hasUnsavedStuff, _admins;
        private readonly Random _random = new();
        private readonly Regex _urls = new(@"\S+(:[\/\\])\S+");
        private readonly FileIO<WitlessDB> _fileIO;
        private readonly Counter _generation = new();

        public Witless(long chat, int interval = 7, int probability = 20, int jpg = 75)
        {
            Chat = chat;
            Interval = interval;
            DgProbability = probability;
            JpgQuality = jpg;
            _fileIO = new FileIO<WitlessDB>(Path);
            Load();
            PauseGeneration(30);
        }

        [JsonProperty] public long Chat { get; set; }
        [JsonProperty] public int Interval
        {
            get => _generation.Interval;
            set => _generation.Interval = value;
        }
        [JsonProperty] public int DgProbability
        {
            get => _probability;
            set => _probability = Math.Clamp(value, 0, 100);
        }
        [JsonProperty] public bool DemotivateStickers { get; set; }
        [JsonProperty] public int JpgQuality
        {
            get => _quality;
            set => _quality = Math.Clamp(value, 0, 100);
        }

        [JsonProperty] public bool AdminsOnly
        {
            get => _admins;
            set
            {
                if (Chat < 0) _admins = value;
            }
        }

        public WitlessDB Words { get; set; }
        public string Path => $@"{DBS_FOLDER}\{DB_FILE_PREFIX}-{Chat}.json";

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
            if (count > 4)
            {
                eaten = EatAdvanced(words);
            }
            
            _hasUnsavedStuff = true;
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
                if (!Words.ContainsKey(word)) Words.TryAdd(word, new ConcurrentDictionary<string, float>());

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

        public string TryToGenerate(string word = START)
        {
            try
            {
                return Generate(word);
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return "";
            }
        }

        public string GenerateByWord(string word)
        {
            string match = FindMatch(word, START, out bool separated);
            string result = TryToGenerate(match);
            if (separated) result = word.Split()[0] + " " + result;
            return result;
        }
        public string GenerateByWordBackwards(string word)
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

            var words = Words.Keys.Where(key => key.StartsWith(word)).ToList();
            if (words.Count > 0) return words[_random.Next(words.Count)];

            words     = Words.Keys.Where(key => word.StartsWith(key, StringComparison.Ordinal)).ToList();
            if (words.Count < 1) return alt;

            words.Sort(Comparison);
            return words[0];

            int Comparison(string x, string y) => y.Length - x.Length;
        }
        
        private string Generate(string word)
        {
            string result = "";
            string current = word;

            while (current != END)
            {
                result = result + " " + current;
                current = PickWord(Words[current]);
            }

            result = result.Replace(START, "").TrimStart();
            
            return TextInRandomLetterCase(result);
        }
        private string GenerateBackwards(string word)
        {
            string result = "";
            string current = word;
            
            while (current != START)
            {
                result = current + " " + result;
                
                var words = new ConcurrentDictionary<string, float>();
                foreach (var bunch in Words)
                {
                    if (bunch.Value.ContainsKey(current))
                    {
                        float x = bunch.Value[current];
                        if (!words.TryAdd(bunch.Key, x)) words[bunch.Key] += x;
                    }
                }
                current = PickWord(words);
            }
            
            result = result.Replace(END, "").TrimEnd();
            
            return TextInRandomLetterCase(result);
        }
        private string PickWord(ConcurrentDictionary<string, float> dictionary)
        {
            var chanceTotal = dictionary.Sum(chance => chance.Value);

            float r = (float)_random.NextDouble() * chanceTotal;

            foreach (var chance in dictionary)
            {
                if  (chance.Value > r) return chance.Key;
                r -= chance.Value;
            }

            return END;
        }
        
        private async void PauseGeneration(int seconds)
        {
            _generation.Stop();
            await Task.Delay(1000 * seconds);
            Save();
            _generation.Resume();
        }
        
        public void Count() => _generation.Count();
        public bool Ready() => _generation.Ready();

        public void Save()
        {
            if (_hasUnsavedStuff) SaveNoMatterWhat();
        }

        public void SaveNoMatterWhat()
        {
            _fileIO.SaveData(Words);
            _hasUnsavedStuff = false;
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        public void Load()
        {
            Words = _fileIO.LoadData();
            _hasUnsavedStuff = false;
            Log($"DIC LOADED << {Chat}");
        }

        public void Backup()
        {
            Save();
            var path = $@"{BACKUP_FOLDER}\{DateTime.Now:yyyy-MM-dd}\{DB_FILE_PREFIX}-{Chat}.json";
            new FileInfo(Path).CopyTo(UniquePath(path));
        }

        public void Delete()
        {
            Backup();
            File.Delete(Path);
        }
    }
}