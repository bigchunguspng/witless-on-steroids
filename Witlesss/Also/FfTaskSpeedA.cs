using System.Collections.Generic;
using System.Globalization;

namespace Witlesss.Also
{
    // ffmpeg -i "input.mp3" -filter:a "atempo=2.0" -vn output.mp3
    public class FfTaskSpeedA : FfTaskSpeedAV
    {
        public FfTaskSpeedA(string inputFilePath, string outputFilePath, double speed) : base(inputFilePath, outputFilePath, speed) { }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-i",
            Input,
            "-filter:a", $"atempo={Speed.ToString(CultureInfo.InvariantCulture)}",
            "-vn",
            Output
        };
    }
}