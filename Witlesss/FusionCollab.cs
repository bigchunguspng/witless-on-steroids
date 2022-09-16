using System;
using static Witlesss.Logger;
using static Witlesss.Strings;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, float>>;

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
            foreach (var word in _dictionary2) // word = "word1: {[w1:x1][w2:x2][...]}"
            {
                if (_dictionary1.ContainsKey(word.Key))
                {
                    foreach (var next in word.Value) // next = "word2: x"}"
                    {
                        if (_dictionary1[word.Key].ContainsKey(next.Key))
                        {
                            // pick x1 or x2
                            _dictionary1[word.Key][next.Key] = Math.Max(_dictionary1[word.Key][next.Key], next.Value);
                        }
                        else
                        {
                            // add "word2: x" pair
                            _dictionary1[word.Key].TryAdd(next.Key, next.Value);
                        }
                    }
                }
                else
                {
                    // add "word1: {[][][][][]}"
                    _dictionary1.TryAdd(word.Key, word.Value);
                }
            }
            Log(LOG_FUSE_DONE, ConsoleColor.Magenta);
        }
    }
}