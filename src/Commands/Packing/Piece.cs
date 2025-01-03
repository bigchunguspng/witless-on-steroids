﻿using Witlesss.Generation;

namespace Witlesss.Commands.Packing
{
    /// <summary>
    /// Post Id Easy Channel Exporter™? Maybe.
    /// </summary>
    public class Piece : SyncCommand
    {
        private static readonly Regex _args = new(@"t.me\/[a-z0-9_]{5,32}\/(\d+)\s(\S+)");
        private static readonly Regex _urls = new(@"t.me\/[a-z0-9_]{5,32}\/");

        protected override void Run()
        {
            if (Args == null || !_args.IsMatch(Args))
            {
                Bot.SendMessage(Origin, PIECE_MANUAL);
            }
            else
            {
                var url  = _urls.Match(Args).Value;
                var args = _args.Match(Args);
                var name = args.Groups[2].Value.Replace(' ', '_');
                var post = args.Groups[1].Value;

                var sandwich = $"{url}[+] [*1..{post}]";
                var chance = Convert.ToInt32(post);

                var cp = new Copypaster();
                cp.Eat(sandwich, out _, chance);

                var path = Move.GetUniqueExtraPackPath(name);
                JsonIO.SaveData(cp.DB, path);

                Bot.SendMessage(Origin, string.Format(PIECE_RESPONSE, Path.GetFileNameWithoutExtension(path)));
                Log($"{Title} >> THE ONE PIECE IS REAL!!!");
            }
        }
    }
}