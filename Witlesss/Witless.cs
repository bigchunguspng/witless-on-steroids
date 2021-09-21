using System;
using System.Collections.Generic;
using System.IO;
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
        
        private readonly Random _random = new Random();
        private readonly FileIO<Dictionary<string, Dictionary<string, int>>> _fileIO;
        private Counter _saving, _generation;
        
        public Dictionary<string, Dictionary<string, int>> Words { get; set; }

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
            Load();
            
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
                if (!Words.ContainsKey(word))
                {
                    Words.Add(word, new Dictionary<string, int>());
                }

                string nextWord = wordlist[i + 1];
                if (Words[word].ContainsKey(nextWord))
                {
                    Words[word][nextWord]++;
                }
                else
                {
                    Words[word].Add(nextWord, 1);
                }
            }
            
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
                currentWord = PickWord(Words[currentWord]);
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

        public void TryToSave()
        {
            _saving.Count();
            if (_saving.Ready())
            {
                Save();
            }
        }
        public void Save()
        {
            _fileIO.SaveData(Words);
            Log($"Словарь для чата {Chat} сохранён!", ConsoleColor.Green);
        }

        public void Load()
        {
            Words = _fileIO.LoadData();
            Log($"Словарь для чата {Chat} загружен!");
        }

        public void Backup()
        {
            Save();
            Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Backup");
            FileInfo file = new FileInfo($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-{Chat}.json");
            file.CopyTo($@"{Environment.CurrentDirectory}\Backup\Telegram-WitlessDB-{Chat}-{DateTime.Now:dd.MM.yyyy_(HH-mm-ss)}.json");
        }
    }
}