using System;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -ss 00:00:05 -t 00:00:15     output.mp4
    // ffmpeg -i "input.mp3" -ss 00:00:05 -t 00:00:15 -vn output.mp3
    // ffmpeg -i "input.mp3" -ss 00:00:05             -vn output.mp3
    public class F_Cut : F_Base
    {
        public F_Cut(string input, CutSpan s, MediaType type) : base(SetOutName(input, "-X"))
        {
            var b = s.Length != TimeSpan.Zero;
            AddInput(input);
            AddOptions("-ss", $"{s.Start:c}");
            AddWhen(b, "-t", $"{s.Length:c}");
            AddSizeFix(type, input);
            AddSongFix(type);
        }
    }
}