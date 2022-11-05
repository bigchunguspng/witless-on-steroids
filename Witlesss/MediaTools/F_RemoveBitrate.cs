using System.Collections.Generic;
using System.Drawing;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -f mp3          -b:a 1k        output.mp4
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k        output.mp4
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k -s WxH output.mp4
    public class F_RemoveBitrate : F_Base
    {
        private readonly string _input, _output;
        private readonly bool _video, _resize;
        private readonly int _bitrate;
        private readonly Size _size;

        public F_RemoveBitrate(string input, out string output, int bitrate, Size size = default)
        {
            output = SetOutName(input, "-L", out _video);
            
            _input = input;
            _output = output;
            _bitrate = bitrate;
            if (size != default)
            {
                _size = size;
                _resize = true;
            }
        }
        
        public override IList<string> CreateArguments()
        {
            var result = new List<string> {"-i", _input, "-f", "mp3", "", "", "-b:a", "1k", "", "", _output};
            if (_video)
            {
                result[3] = "mp4";
                result[4] = "-b:v";
                result[5] = $"{_bitrate}k";
                if (_resize)
                {
                    result[8] = "-s";
                    result[9] = $"{_size.Width}x{_size.Height}";
                }
            }
            return RemoveEmpties(result);
        }
    }
}