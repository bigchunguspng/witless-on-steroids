using System.Collections.Generic;
using System.Globalization;

namespace Witlesss.Also
{
    // ffmpeg -i "input.mp4" -filter:v "setpts=0.25*PTS" out.mp4
    public class FfTaskSpeedV : FfTaskSpeedAV
    {
        public FfTaskSpeedV(string inputFilePath, string outputFilePath, double speed) : base(inputFilePath, outputFilePath, speed) { }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-i",
            Input,
            "-filter:v",
            $"setpts={(1 / Speed).ToString(CultureInfo.InvariantCulture)}*PTS",
            Output
        };
    }
}