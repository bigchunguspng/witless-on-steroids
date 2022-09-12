using System.Collections.Generic;
using System.Drawing;
using static Witlesss.Extension;

namespace Witlesss.MediaTools
{
    // ffmpeg -framerate 30 -i "F-%04d-D.jpg" -s WxH output.mp4
    public class F_RenderAnimation : F_Base
    {
        private readonly double _framerate;
        private readonly Size _size;
        private readonly string _inputFilesPath;
        private readonly string _outputFilePath;

        public F_RenderAnimation(double framerate, Size size, string inputFilesPath, string outputFilePath)
        {
            _framerate = framerate;
            _size = size;
            _inputFilesPath = inputFilesPath;
            _outputFilePath = outputFilePath;
        }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-framerate", FormatDouble(_framerate),
            "-i", _inputFilesPath,
            "-s", $"{_size.Width}x{_size.Height}", _outputFilePath
        };
    }
}