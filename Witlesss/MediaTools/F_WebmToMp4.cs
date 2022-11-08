using System.Collections.Generic;
using System.Drawing;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webm" -s WxH output.mp4
    public class F_WebmToMp4 : F_WebpToJpg
    {
        private readonly Size _size;
        
        public F_WebmToMp4(string input, Size size) : base(input, ".mp4") => _size = size;

        public override IList<string> CreateArguments() => new[]
        {
            "-i", Input, "-s", $"{_size.Width}x{_size.Height}", Output
        };
    }
}