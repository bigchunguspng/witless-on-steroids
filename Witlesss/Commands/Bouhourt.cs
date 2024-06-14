﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Witlesss.Copypaster;

namespace Witlesss.Commands
{
    public class Bouhourt : WitlessSyncCommand
    {
        private readonly Regex _args = new(@"(\d)?(?:\s)?(.+)?");
        private readonly WitlessDB _baguette = new FileIO<WitlessDB>("BT.json").LoadData();

        protected override void Run()
        {
            var length = 3;
            string? start = null;
            if (Args is not null)
            {
                var match = _args.Match(Args);
                if (match.Success)
                {
                    var g1 = match.Groups[1];
                    var g2 = match.Groups[2];
                    if (g1.Success && int.TryParse(g1.Value, out var x)) length = x;
                    if (g2.Success)
                    {
                        start = g2.Value.ToUpper();
                        length--;
                    }
                }
            }

            var lines = new List<string>(length);
            var words = _baguette[START];
            var word = PickWord(words);

            if (start != null) lines.Add(start);

            AddTextLine();

            words = _baguette["_mid"];
            for (int i = 1; i < length; i++)
            {
                word = PickWord(words);
                if (word == END) break;
                AddTextLine();
            }

            string result = string.Join("\n@\n", lines.Where(x => x != "")).Replace(" @ ", "\n@\n").ToUpper();
            Bot.SendMessage(Chat, result);
            Log($"{Title} >> BUGURT #@#");

            void AddTextLine() => lines.Add(Baka.GenerateByWord(PullWord(word)).Trim('@').TrimStart());
        }

        private string PullWord(string word)
        {
            string[] xs;

            if      (word.StartsWith("..")) xs = Baka.Words.Keys.Where(x => x.EndsWith(word[2..] )).ToArray();
            else if (word.EndsWith  ("..")) xs = Baka.Words.Keys.Where(x => x.EndsWith(word[..^2])).ToArray();
            else if (word.Contains  (' ') ) return word.Split()[0] + ' ' + Baka.GenerateByWord(PullWord(word.Split()[1]));
            else
                return Baka.Words.ContainsKey(word) ? word : START;
            return xs.Length > 0 ? xs.ElementAt(Random.Shared.Next(xs.Length)) : START;
        }
    }
}