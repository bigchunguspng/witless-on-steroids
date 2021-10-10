using System;
using System.Collections.Generic;
using static Witlesss.Logger;
using static Witlesss.Strings;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss
{
    public class FusionCollab
    {
        private readonly WitlessDB _dictionary1, _dictionary2;

        public FusionCollab(WitlessDB dictionary1, WitlessDB dictionary2)
        {
            _dictionary1 = dictionary1;
            _dictionary2 = dictionary2;

            Log(LOG_FUSION_HAVE_DICS, ConsoleColor.Magenta);
        }
        
        public void Fuse()
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
                            _dictionary1[pair.Key].TryAdd(chance.Key, chance.Value);
                        }
                    }
                }
                else
                {
                    // add "word1: {[][][][][]}"
                    _dictionary1.TryAdd(pair.Key, pair.Value);
                }
            }
            Log(LOG_FUSE_DONE, ConsoleColor.Magenta);
            
            int ChooseBigger(int a, int b) => a > b ? a : b;
        }
    }
}