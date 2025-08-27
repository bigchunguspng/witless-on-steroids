using PF_Bot.Generation;
using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Extensions;

namespace PF_Bot.Commands.Packing
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

                var pack = new GenerationPack();
                pack.Eat_Advanced(sandwich, chance);

                var path = Move.GetUniqueExtraPackPath(name);
                GenerationPackIO.Save(pack, path);

                Bot.SendMessage(Origin, string.Format(PIECE_RESPONSE, Path.GetFileNameWithoutExtension(path)));
                Log($"{Title} >> THE ONE PIECE IS REAL!!!");
            }
        }
    }
}