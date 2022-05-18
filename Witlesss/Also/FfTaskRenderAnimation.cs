using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace Witlesss.Also
{
    // ffmpeg -framerate 30 -i "F-%04d-D.jpg" -s WxH output.mp4
    public class FfTaskRenderAnimation : FfTask
    {
        private readonly double _framerate;
        private readonly Size _size;
        private readonly string _inputFilesPath;
        private readonly string _outputFilePath;

        public FfTaskRenderAnimation(double framerate, Size size, string inputFilesPath, string outputFilePath)
        {
            _inputFilesPath = inputFilesPath;
            _outputFilePath = outputFilePath;
            _framerate = framerate;
            _size = size;
        }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-framerate",
            _framerate.ToString(CultureInfo.InvariantCulture),
            "-i",
            _inputFilesPath ?? "",
            "-s",
            $"{_size.Width}x{_size.Height}",
            _outputFilePath ?? ""
        };
    }
}