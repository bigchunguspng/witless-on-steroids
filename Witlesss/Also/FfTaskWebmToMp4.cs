using System.Collections.Generic;
using System.Drawing;

namespace Witlesss.Also
{
    // ffmpeg -i "input.webm" -s WxH "output.mp4"
    public class FfTaskWebmToMp4 : FfTaskWebpToJpg
    {
        private readonly Size _size;
        
        public FfTaskWebmToMp4(string inputFilePath, out string outputFilePath, string extension, Size size) : base(inputFilePath, out outputFilePath, extension)
        {
            _size = size;
        }

        public override IList<string> CreateArguments() => new[]
        {
            "-i",
            InputFilePath ?? "",
            "-s", $"{_size.Width}x{_size.Height}",
            OutputFilePath ?? ""
        };
    }
}