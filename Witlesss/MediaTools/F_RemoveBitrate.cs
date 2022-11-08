using System.Collections.Generic;
using System.Drawing;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -f mp3          -b:a 1k        output.mp4
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k        output.mp4  <-- not using
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k -s WxH output.mp4
    public class F_RemoveBitrate : F_SimpleTask
    {
        private readonly int _bitrate;
        private readonly Size _size;

        public F_RemoveBitrate(string input) : base(input, OutputName(input)) { }
        public F_RemoveBitrate(string input, int bitrate, Size size) : this(input)
        {
            _bitrate = bitrate;
            _size = size;
        }
        
        public override IList<string> CreateArguments()
        {
            if (_bitrate > 0) return new List<string> 
            {
                "-i", Input, "-f", "mp4",
                "-b:v", $"{_bitrate}k", "-b:a", "1k",
                "-s", $"{_size.Width}x{_size.Height}", Output
            };
            return new List<string> { "-i", Input, "-f", "mp3", "-b:a", "1k", Output };
        }

        private static string OutputName(string input) => SetOutName(input, "-L").Replace(".webm", ".mp4");
    }
}