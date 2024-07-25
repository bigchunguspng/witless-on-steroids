using System;
using System.Linq;
using System.Threading.Tasks;
using Witlesss.MediaTools;
using static System.TimeSpan;

#pragma warning disable CS4014

namespace Witlesss.Commands.Editing
{
    public class Cut : Slice
    {
        protected override async Task Execute()
        {
            var args = Args?.Split().SkipWhile(x => x.StartsWith('/') || x.StartsWith("http")).ToArray();
            // todo move this to another command - /video

            var x = ParseArgs(args);
            if (x.failed)
            {
                Bot.SendMessage(Chat, CUT_MANUAL);
                return;
            }

            var span = new CutSpan(x.start, x.length);

            var (path, type, waitMessage) = await DownloadFileSuperCool();

            var result = await FFMpegXD.Cut(path, span);

            Task.Run(() => Bot.DeleteMessage(Chat, waitMessage));

            SendResult(result, type);
            Log($"{Title} >> CUT [8K-]");
        }

        // todo move this and similar to arg parsing
        public static (bool failed, TimeSpan start, TimeSpan length) ParseArgs(string[]? s)
        {
            if (s is null) return (true, Zero, Zero);

            var len = s.Length;
            if     (len == 1 && s[0].IsTimeSpan(out var length)) return (false, Zero,  length);      // [++]----]
            if     (len >= 2 && s[0].IsTimeSpan(out var  start))
            {
                if (len == 3 && s[2].IsTimeSpan(out var    end)) return (false, start, end - start); // [-[++]--]
                if             (s[1].IsTimeSpan(out     length)) return (false, start, length);      // [-[++]--]
                else                                             return (false, start, Zero);        // [-[+++++]
            }
            else                                                 return (true,  Zero,  Zero);        // [-------]
        }

        protected override string VideoFileName => "cut_fap_club.mp4";
        protected override string AudioFileName => SongNameOr($"((({Sender}))).mp3");
    }
}