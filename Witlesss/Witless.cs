using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Logger;
using static Witlesss.Strings;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        public const string Start = "_start", End = "_end", Link = "[ссылка удалена]";

        private readonly Random _random;
        private readonly FileIO<WitlessDB> _fileIO;
        private Counter _generation;
        private int _probability;
        
        public bool HasUnsavedStuff;
        
        public Witless(long chat, int interval = 7, int probability = 20)
        {
            Chat = chat;
            _random = new Random();
            _generation = new Counter(interval);
            _fileIO = new FileIO<WitlessDB>(Path);
            DgProbability = probability;
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
        
        public WitlessDB Words { get; set; }
        public string Path => $@"{CurrentDirectory}\{DBS_FOLDER}\{DB_FILE_PREFIX}-{Chat}.json";
        
        public bool ReceiveSentence(ref string sentence)
        {
            if (!SentenceIsAcceptable(sentence)) return false;
            
            var wordlist = new List<string> {Start};
            wordlist.AddRange(WordsOf(sentence));
            wordlist.Add(End);
            
            for (var i = 0; i < wordlist.Count; i++)
            {
                if (WordIsLink(wordlist[i]))
                    wordlist[i] = Link;
            }

            for (var i = 0; i < wordlist.Count - 1; i++)
            {
                string word = wordlist[i];
                if (!Words.ContainsKey(word))
                {
                    Words.TryAdd(word, new ConcurrentDictionary<string, int>());
                }

                string nextWord = wordlist[i + 1];
                if (Words[word].ContainsKey(nextWord))
                {
                    Words[word][nextWord]++;
                }
                else
                {
                    Words[word].TryAdd(nextWord, 1);
                }
            }

            HasUnsavedStuff = true;
            sentence = string.Join(' ', wordlist.GetRange(1, wordlist.Count - 2));
            return true;
        }
        private bool SentenceIsAcceptable(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return false;
            if (sentence.StartsWith('/'))
                return false;
            if (sentence.StartsWith('.'))
                return false;
            if (sentence.StartsWith("http") && !sentence.Contains(" "))
                return false;
            return true;
        }
        private string[] WordsOf(string sentence) => sentence.ToLower().Trim().Split(new[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries);
        private bool WordIsLink(string word) => (word.Contains(".com") || word.Contains(".ru")) && word.Length > 20 || word.StartsWith("http") && word.Length > 7;

        public string TryToGenerate(string word = Start)
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
        public string TryToGenerateBackwards(string word)
        {
            try
            {
                return GenerateBackwards(word);
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return "";
            }
        }

        public string GenerateByWord(string word)
        {
            word = FindMatch(word, Start);
            return TryToGenerate(word);
        }
        public string GenerateByWordBackwards(string word)
        {
            word = FindMatch(word, End);
            return TryToGenerateBackwards(word);
        }
        
        private string FindMatch(string word, string alt)
        {
            if (!Words.ContainsKey(word))
            {
                var words = new List<string>();
                foreach (string key in Words.Keys)
                {
                    if (key.StartsWith(word)) words.Add(key);
                }
                if (words.Count > 0)
                    return words[_random.Next(words.Count)];

                foreach (string key in Words.Keys)
                {
                    if (word.StartsWith(key, StringComparison.Ordinal)) words.Add(key);
                }
                if (words.Count > 0)
                {
                    words.Sort(Comparison);
                    return words[0];
                }
                return alt;
            }
            return word;

            int Comparison(string x, string y) => y.Length - x.Length;
        }
        
        private string Generate(string word)
        {
            string result = "";
            string currentWord = word == Start || Words.ContainsKey(word) ? word : Start;

            while (currentWord != End)
            {
                result = result + " " + currentWord;
                currentWord = PickWord(Words[currentWord]);
            }

            result = result.Replace(Start, "").TrimStart();
            
            return TextInRandomLetterCase(result);
        }
        private string GenerateBackwards(string word)
        {
            string result = "";
            string currentWord = word == End || Words.ContainsKey(word) ? word : End;
            
            while (currentWord != Start)
            {
                result = currentWord + " " + result;
                
                var words = new ConcurrentDictionary<string, int>();
                foreach (var bunch in Words)
                {
                    if (bunch.Value.ContainsKey(currentWord) && !words.TryAdd(bunch.Key, 1)) words[bunch.Key]++;
                }
                currentWord = PickWord(words);
            }
            
            result = result.Replace(End, "").TrimEnd();
            
            return TextInRandomLetterCase(result);
        }
        private string PickWord(ConcurrentDictionary<string, int> dictionary)
        {
            var chanceTotal = 0;
            foreach (var chance in dictionary)
            {
                chanceTotal += chance.Value;
            }
            
            int r = _random.Next(chanceTotal);
            string result = End;

            foreach (var chance in dictionary)
            {
                if (chance.Value > r)
                {
                    return chance.Key;
                }
                else
                {
                    r -= chance.Value;
                }
            }

            return result;
        }
        
        private async void PauseGeneration(int seconds)
        {
            _generation.Stop();
            await Task.Delay(1000 * seconds);
            Save();
            _generation.Resume();
        }
        
        public void Count() => _generation.Count();
        public bool ReadyToGen() => _generation.Ready();

        public void Save()
        {
            if (HasUnsavedStuff) SaveNoMatterWhat();
        }

        public void SaveNoMatterWhat()
        {
            _fileIO.SaveData(Words);
            HasUnsavedStuff = false;
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        public void Load()
        {
            Words = _fileIO.LoadData();
            HasUnsavedStuff = false;
            Log($"DIC LOADED << {Chat}");
        }

        public void Backup()
        {
            Save();
            var file = new FileInfo(Path);
            var path = $@"{CurrentDirectory}\{BACKUP_FOLDER}\{DateTime.Now:yyyy-MM-dd}";
            Directory.CreateDirectory(path);
            file.CopyTo(UniquePath($@"{path}\{DB_FILE_PREFIX}-{Chat}.json", ".json"));
        }
    }
}