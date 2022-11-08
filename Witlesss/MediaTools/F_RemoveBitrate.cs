using System.Collections.Generic;
using System.Drawing;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -f mp3          -b:a 1k        output.mp4
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k        output.mp4  <-- not using
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k -s WxH output.mp4
    public class F_RemoveBitrate : F_Base
    {
        private readonly string _input, _output;
        private readonly bool _video;
        private readonly int _bitrate;
        private readonly Size _size;

        public F_RemoveBitrate(string input, string output, int bitrate, Size size = default)
        {
            _input = input;
            _output = output;
            _bitrate = bitrate;
            _video = size != default;
            if (_video) _size = size;
        }
        
        public override IList<string> CreateArguments()
        {
            if (_video) return new List<string> 
            {
                "-i", _input, "-f", "mp4",
                "-b:v", $"{_bitrate}k", "-b:a", "1k",
                "-s", $"{_size.Width}x{_size.Height}", _output
            };
            return new List<string> { "-i", _input, "-f", "mp3", "-b:a", "1k", _output };
        }
    }
}