using System.Collections.Generic;
using System.Drawing;
using static Witlesss.Extension;

namespace Witlesss.Also
{
    // ffmpeg -i "input.mp3" -f mp3          -b:a 1k        output.mp4
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k        output.mp4
    // ffmpeg -i "input.mp4" -f mp4 -b:v 40k -b:a 1k -s WxH output.mp4
    public class FfTaskRemoveBitrate : FfTask
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;
        private readonly string _bitrate;
        private readonly bool _video, _otherSize;
        private readonly Size _size;

        public FfTaskRemoveBitrate(string inputFilePath, out string outputFilePath, int bitrate, Size size = default)
        {
            _inputFilePath = inputFilePath;
            string extension = GetFileExtension(inputFilePath);
            _video = extension == ".mp4";
            if (!_video) extension = ".mp3";
            _outputFilePath = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + "-L" + extension;
            _bitrate = bitrate.ToString();
            outputFilePath = _outputFilePath;
            if (size != default)
            {
                _size = size;
                _otherSize = true;
            }
        }
        
        public override IList<string> CreateArguments()
        {
            var result = new List<string> {"-i", _inputFilePath, "-f", "mp3", "", "", "", "", "-b:a", "1k", _outputFilePath};
            if (_video)
            {
                result[3] = "mp4";
                result[4] = "-b:v";
                result[5] = $"{_bitrate}k";
                if (_otherSize)
                {
                    result[6] = "-s";
                    result[7] = $"{_size.Width}x{_size.Height}";
                }
            }
            return RemoveEmpties(result);
        }
    }
}