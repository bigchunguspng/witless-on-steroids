using System;
using System.IO;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -ss 00:00:05 -t 00:00:15 output.mp3
    // ffmpeg -i "input.mp3" -ss 00:00:05             output.mp3
    public class F_Cut : F_Base
    {
        public F_Cut(string input, TimeSpan start, TimeSpan length) : base(SetOutName(input, "-X"))
        {
            var b = length != TimeSpan.Zero;
            AddInput(input);
            AddOptions("-ss", $"{start:c}");
            AddWhen(b, "-t", $"{length:c}");
            AddSizeFix(input, MediaTypeFromID(Path.GetFileName(input)));
        }
    }
}