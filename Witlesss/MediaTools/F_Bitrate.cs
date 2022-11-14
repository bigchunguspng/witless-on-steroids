using static Witlesss.MediaType;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -f mp3 -vn      -b:a 1k output.mp3
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k output.mp4
    public class F_Bitrate : F_Base
    {
        public F_Bitrate(string input, int bitrate = 0, MediaType type = Movie) : base(SetOutName(input, "-L"))
        {
            var v = bitrate > 0;

            AddInput(input);
            AddOptions("-f", v ? "mp4" : "mp3");
            AddWhen(v, "-b:v", $"{bitrate}k");
            AddOptions("-b:a", "1k");
            AddSizeFix(type, input);
            AddSongFix(type);
        }
    }
}