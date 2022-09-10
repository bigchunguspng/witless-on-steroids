using System.Collections.Generic;
using static Witlesss.Extension;

namespace Witlesss.Also
{
    // ffmpeg -i "input.mp4" -vf reverse -af areverse output.mp4
    // ffmpeg -i "input.mp3"             -af areverse output.mp3
    public class FfTaskReverse : FfTask
    {
        private readonly string _input;
        private readonly string _output;
        private readonly bool _video;
        
        public FfTaskReverse(string inputFilePath, out string outputFilePath)
        {
            _input = inputFilePath;
            string extension = GetFileExtension(inputFilePath);
            _video = extension == ".mp4";
            if (!_video) extension = ".mp3";
            _output = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + "-R" + extension;
            outputFilePath = _output;
        }

        public override IList<string> CreateArguments()
        {
            var result = new List<string> {"-i", _input, "", "", "-af", "areverse", _output};
            if (_video)
            {
                result[2] = "-vf";
                result[3] = "reverse";
            }
            return RemoveEmpties(result);
        }
    }
}