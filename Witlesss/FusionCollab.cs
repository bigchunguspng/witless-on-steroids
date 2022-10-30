using System;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, float>>;

namespace Witlesss
{
    public class FusionCollab
    {
        private readonly WitlessDB _dictionary1, _dictionary2;

        public FusionCollab(Witless witless, WitlessDB dictionary)
        {
            witless.Backup();

            _dictionary1 = witless.Words;
            _dictionary2 = dictionary;
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
        }
    }
}