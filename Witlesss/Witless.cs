﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Witlesss.Logger;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private const string Start = "_start", End = "_end", Dot = "_dot", Link = "[ссылка удалена]";
        
        private readonly Random _random = new Random();
        private readonly FileIO<WitlessDB> _fileIO;
        private Counter _generation;
        
        public bool HasUnsavedStuff;
        
        public Witless(long chat, int interval = 7)
        {
            Chat = chat;
            _generation = new Counter(interval);
            _fileIO = new FileIO<WitlessDB>($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-{Chat}.json");
            Load();
            WaitOnStartup();
        }

        [JsonProperty] private long Chat { get; set; }
        [JsonProperty] public int Interval
        {
            get => _generation.Interval;
            set => _generation.Interval = value;
        }
        
        public WitlessDB Words { get; set; }
        
        public bool ReceiveSentence(ref string sentence)
        {
            if (!SentenceIsAcceptable(sentence)) return false;
            
            List<string> wordlist = new List<string> {Start};
            wordlist.AddRange(
                sentence.ToLower().Replace(". ", $" {Dot} {Start} ").Replace($". {Dot} {Start} ", ".. ")
                .Trim().Split(new[]{ ' ', '\t', '\n'}, StringSplitOptions.RemoveEmptyEntries).ToList());
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
            if (sentence.StartsWith("http") && !sentence.Contains(" "))
                return false;
            return true;
        }
        private bool WordIsLink(string word) => (word.Contains(".com") || word.Contains(".ru")) && word.Length > 20 || word.StartsWith("http") && word.Length > 7;

        public string TryToGenerate()
        {
            try
            {
                return Generate();
            }
            catch (Exception e)
            {
                Log(e.Message, ConsoleColor.Red);
                return "";
            }
        }
        private string Generate()
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
                Log($"Словарь для чата {Chat} сохранён!", ConsoleColor.Green);
            }
        }

        public void Load()
        {
            Words = _fileIO.LoadData();
            HasUnsavedStuff = false;
            Log($"Словарь для чата {Chat} загружен!");
        }

        public void Backup()
        {
            Save();
            Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Backup");
            FileInfo file = new FileInfo($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-{Chat}.json");
            file.CopyTo($@"{Environment.CurrentDirectory}\Backup\Telegram-WitlessDB-{Chat}-{DateTime.Now:dd.MM.yyyy_(HH-mm-ss.ffff)}.json");
        }
    }
}