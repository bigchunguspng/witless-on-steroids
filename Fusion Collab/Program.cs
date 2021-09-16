using System;
using System.Collections.Generic;
using Witlesss;
using static Witlesss.Logger;

namespace Fusion_Collab
{
    class Program
    {
        private static Dictionary<string, Dictionary<string, int>> _dictionary1, _dictionary2;
        private static FileIO<Dictionary<string, Dictionary<string, int>>> _file1, _file2, _result;
        
        static void Main(string[] args)
        {
            Import();
            Fuse();
            Export();

            Console.ReadLine();
        }

        static void Import()
        {
            _file1 = new FileIO<Dictionary<string, Dictionary<string, int>>>($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-1.json");
            _file2 = new FileIO<Dictionary<string, Dictionary<string, int>>>($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-2.json");

            _dictionary1 = _file1.LoadData();
            _dictionary2 = _file2.LoadData();

            Log("Файлы импортировано");
        }

        static void Fuse()
        {
            foreach (var pair in _dictionary2) //pair = "word1: {[][][][][]}"
            {
                if (_dictionary1.ContainsKey(pair.Key))
                {
                    foreach (KeyValuePair<string, int> chance in pair.Value) //chance = "word2: x"}"
                    {
                        if (_dictionary1[pair.Key].ContainsKey(chance.Key))
                        {
                            // pick x1 or x2
                            _dictionary1[pair.Key][chance.Key] = ChooseBigger(_dictionary1[pair.Key][chance.Key], chance.Value);
                        }
                        else
                        {
                            // add "word2: x" pair
                            _dictionary1[pair.Key].Add(chance.Key, chance.Value);
                        }
                    }
                }
                else
                {
                    // add "word1: {[][][][][]}"
                    _dictionary1.Add(pair.Key, pair.Value);
                }
            }
            Log("Слияние выполнено");
        }

        static int ChooseBigger(int a, int b) => a > b ? a : b;

        static void Export()
        {
            _result = new FileIO<Dictionary<string, Dictionary<string, int>>>($@"{Environment.CurrentDirectory}\Telegram-WitlessDB-Fusion.json");
            _result.SaveData(_dictionary1);
            Log("Файл сохранено!");
        }
    }
}