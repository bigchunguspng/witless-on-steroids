using System;
using static System.TimeSpan;

namespace Witlesss.Commands.Editing
{
    public class Cut : FileEditingCommand
    {
        public override void Run()
        {
            if (NothingToProcess()) return;

            var x = GetArgs();
            if (x.failed)
            {
                Bot.SendMessage(Chat, CUT_MANUAL);
                return;
            }
            
            Bot.Download(FileID, Chat, out var path, out var type);
            
            var result = Memes.Cut(path, new CutSpan(x.start, x.length));
            SendResult(result, type);
            Log($"{Title} >> CUT [8K-]");
        }

        protected static (bool failed, TimeSpan start, TimeSpan length) GetArgs()
        {
            var s = Text.Split();
            var len = s.Length;
            if     (len == 2 && s[1].IsTimeSpan(out var length)) return (false, Zero,  length);      // [++]----]
            if     (len >= 3 && s[1].IsTimeSpan(out var  start))
            {
                if (len == 4 && s[3].IsTimeSpan(out var    end)) return (false, start, end - start); // [-[++]--]
                if             (s[2].IsTimeSpan(out     length)) return (false, start, length);      // [-[++]--]
                else                                             return (false, start, Zero);        // [-[+++++]
            }
            else                                                 return (true,  Zero,  Zero);        // [-------]
        }
        
        protected override string AudioFileName => SongNameOr($"((({Sender}))).mp3");
        protected override string VideoFileName { get; } = "cut_fap_club.mp4";
    }
}