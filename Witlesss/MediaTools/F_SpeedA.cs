using System.Collections.Generic;
using System.Globalization;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp3" -filter:a "atempo=2.0" -vn output.mp3
    public class F_SpeedA : F_SpeedAV
    {
        public F_SpeedA(string inputFilePath, string outputFilePath, double speed) : base(inputFilePath, outputFilePath, speed) { }
        
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