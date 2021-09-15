using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Witlesss.Logger;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private const string Start = "_start", End = "_end", Dot = "_dot";
        
        private readonly Dictionary<string, Dictionary<string, int>> _words;
        private readonly Random _random = new Random();
        private readonly FileIO<Dictionary<string, Dictionary<string, int>>> _fileIO;
        private Counter _saving, _generation;

        [JsonProperty] private long Chat { get; set; }
        [JsonProperty] public int Interval
        {
            get => _generation.Interval;
            set => _generation.Interval = value;
        }
        
        public Witless(long chat, int interval = 7)
        {
            Chat = chat;
            
            _saving = new Counter(10);
            _generation = new Counter(interval);
            
            _fileIO = new FileIO<Dictionary<string, Dictionary<string, int>>>($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-{Chat}.json");
            _words = _fileIO.LoadData();
            
            WaitOnStartup();
        }
        
        public bool ReceiveSentence(string sentence)
        {
            if (!SentenceIsAcceptable(sentence)) return false;
            
            List<string> wordlist = new List<string> {Start};
            wordlist.AddRange(sentence.ToLower().Replace(". ", $" {Dot} {Start} ")
                .Trim().Split(new[]{ ' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList());
            wordlist.Add(End);

            for (var i = 0; i < wordlist.Count - 1; i++)
            {
                string word = wordlist[i];
                if (!_words.ContainsKey(word))
                {
                    Dictionary<string, int> probOfNextWords = new Dictionary<string, int>();
                    _words.Add(word, probOfNextWords);
                }

                string nextWord = wordlist[i + 1];
                if (_words[word].ContainsKey(nextWord))
                {
                    _words[word][nextWord]++;
                }
                else
                {
                    _words[word].Add(nextWord, 1);
                }
            }

            TryToSave();
            return true;
        }
        private bool SentenceIsAcceptable(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return false;
            if (sentence.StartsWith('/'))
                return false;
            if (sentence.Contains("https://") || sentence.Contains("http://"))
                return false;
            /*if (sentence.Contains("@") && sentence.Contains(".") && !sentence.Contains(" "))
                return false;*/ // ок оставлю по приколу хд)00)
            
            return true;
        }
        
        public string Generate()
        {
            string result = "";
            string currentWord = Start;

            while (currentWord != End)
            {
                result = result + " " + currentWord;
                currentWord = PickWord(_words[currentWord]);
            }

            result = result.Replace(Start, "").Replace($" {Dot} ", ".").TrimStart();
            
            return result;
        }
        private string PickWord(Dictionary<string, int> dictionary)
        {
            int totalProbability = 0;
            foreach (KeyValuePair<string, int> chance in dictionary)
            {
                totalProbability += chance.Value;
            }
            
            int r = _random.Next(totalProbability);
            string result = End;

            foreach (KeyValuePair<string,int> pair in dictionary)
            {
                if (pair.Value > r)
                {
                    return pair.Key;
                }
                else
                {
                    r -= pair.Value;
                }
            }

            return result;
        }
        
        private async void WaitOnStartup()
        {
            await Task.Run(() =>
            {
                _saving.Stop();
                _generation.Stop();
                Thread.Sleep(6200);
                
                Save();
                _generation.Resume();
                _saving.Resume();
            });
        }
        
        public void Count() => _generation.Count();
        public bool ReadyToGen() => _generation.Ready();

        private void TryToSave()
        {
            _saving.Count();
            if (_saving.Ready())
            {
                Save();
            }
        }
        private void Save()
        {
            _fileIO.SaveData(_words);
            Log($"Словарь для чата {Chat} сохранён!");
        }
    }
}