﻿using static Witlesss.X.MediaType;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -f mp3 -vn              -b:a 1k output.mp3
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k         -b:a 1k output.mp4  <-- obsolete
    // ffmpeg -i "input.mp4" -vcodec libx264 -crf 45 -b:a 1k output.mp4
    public class F_Bitrate : F_Base
    {
        public F_Bitrate(string input, int bitrate = 0, MediaType type = Movie) : base(SetOutName(input, "-L"))
        {
            var a = type == Audio;
            var v = type != Audio;

            AddInput(input);
            AddWhen(a, "-f", "mp3");
            AddWhen(v, "-vcodec", "libx264", "-crf", bitrate.ToString());
            AddOptions("-b:a", "1k");
            AddSizeFix(type, input);
            AddSongFix(type);
        }
    }
}