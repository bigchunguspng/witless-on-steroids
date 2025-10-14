using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Extensions;
using PF_Tools.Copypaster.Helpers;

namespace PF_Bot.Features_Aux.Packs.Commands
{
    /// Post Id Easy Channel Exporter™? Maybe.
    public class Piece : CommandHandlerBlocking
    {
        private static readonly Regex
            _rgx_args = new(@"t.me\/[a-z0-9_]{5,32}\/(\d+)\s(\S+)", RegexOptions.Compiled),
            _rgx_urls = new(@"t.me\/[a-z0-9_]{5,32}\/",             RegexOptions.Compiled);

        protected override void Run()
        {
            if (Args != null && _rgx_args.IsMatch(Args))
            {
                var url  = _rgx_urls.Match(Args).Value;
                var args = _rgx_args.Match(Args);
                var name = args.Groups[2].Value;
                var post = args.Groups[1].Value;

                var sandwich = $"{url}[+] [*1..{post}]";
                var chance = Convert.ToInt32(post);

                var pack = new GenerationPack();
                pack.Eat_Advanced(sandwich, chance);

                var path = PackManager.GetExtraPackPath(name);
                GenerationPackIO.Save(pack, path);

                Bot.SendMessage(Origin, PIECE_RESPONSE.Format(Path.GetFileNameWithoutExtension(path)));
                Log($"{Title} >> THE ONE PIECE IS REAL!!!");
            }
            else
                SendManual(PIECE_MANUAL);
        }
    }
}