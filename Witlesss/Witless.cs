using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Logger;
using static Witlesss.Also.Strings;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private const string Start = "_start", End = "_end", Dot = "_dot", Link = "[ссылка удалена]";

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
            WaitOnStartup();
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
            sentence = string.Join(' ', wordlist.GetRange(1, wordlist.Count - 2)).Replace($" {Dot} {Start} ", ". ");
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
        private string[] WordsOf(string sentence) => sentence.ToLower().Replace(". ", $" {Dot} {Start} ").Replace($". {Dot} {Start} ", ".. ").Trim().Split(new[] {' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries);
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
        public string TryToGenerateFromWord(string word = Start)
        {
            if (!Words.ContainsKey(word))
            {
                var words = new List<string>();
                foreach (string key in Words.Keys)
                    if (key.StartsWith(word))
                        words.Add(key);
                if (words.Count > 0)
                    return TryToGenerate(words[_random.Next(words.Count)]);

                foreach (string key in Words.Keys)
                    if (word.StartsWith(key, StringComparison.Ordinal))
                        words.Add(key);
                if (words.Count > 0)
                {
                    words.Sort(Comparison);
                    word = words[0];
                }
            }
            return TryToGenerate(string.IsNullOrEmpty(word) ? Start : word);

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

            result = result.Replace(Start, "").Replace($" {Dot} ", ".").TrimStart();
            
            return TextInRandomLetterCase(result);
        }
        private string PickWord(ConcurrentDictionary<string, int> dictionary)
        {
            int totalProbability = 0;
            foreach (KeyValuePair<string, int> chance in dictionary)
            {
                totalProbability += chance.Value;
            }
            
            int r = _random.Next(totalProbability);
            string result = End;

            foreach (KeyValuePair<string,int> chance in dictionary)
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
        
        private async void WaitOnStartup()
        {
            await Task.Run(() =>
            {
                _generation.Stop();
                Thread.Sleep(28000);
                
                Save();
                _generation.Resume();
            });
        }
        
        public void Count() => _generation.Count();
        public bool ReadyToGen() => _generation.Ready();

        public void Save()
        {
            if (HasUnsavedStuff)
            {
                _fileIO.SaveData(Words);
                HasUnsavedStuff = false;
                Log($"DIC  SAVED << {Chat}", ConsoleColor.Green);
            }
        }

        public void Load()
        {
            Words = _fileIO.LoadData();
            HasUnsavedStuff = false;
            Log($"LOADED DIC << {Chat}");
        }

        public void Backup()
        {
            Save();
            var file = new FileInfo(Path);
            Directory.CreateDirectory($@"{CurrentDirectory}\{BACKUP_FOLDER}");
            file.CopyTo($@"{CurrentDirectory}\{BACKUP_FOLDER}\{DB_FILE_PREFIX}-{Chat}-{DateTime.Now:dd.MM.yyyy_(HH-mm-ss.ffff)}.json");
        }
    }
}