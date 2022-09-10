using System.Collections.Generic;
using System.Globalization;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -filter:v "setpts=0.25*PTS" out.mp4
    public class F_SpeedV : F_SpeedAV
    {
        public F_SpeedV(string inputFilePath, string outputFilePath, double speed) : base(inputFilePath, outputFilePath, speed) { }
        
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