using System.Collections.Generic;
using System.Globalization;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -filter_complex "[0:v]setpts=0.5*PTS[v];[0:a]atempo=2.0[a]" -map "[v]" -map "[a]" output.mp4
    public class F_SpeedAV : F_Base
    {
        protected readonly string Input;
        protected readonly string Output;
        protected readonly double Speed;
        
        public F_SpeedAV(string inputFilePath, string outputFilePath, double speed)
        {
            Input = inputFilePath;
            Output = outputFilePath;
            Speed = speed;
        }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-i",
            Input,
            "-filter_complex",
            $"[0:v]setpts={(1 / Speed).ToString(CultureInfo.InvariantCulture)}*PTS[v];[0:a]atempo={Speed.ToString(CultureInfo.InvariantCulture)}[a]",
            "-map", "[v]",
            "-map", "[a]",
            Output
        };
    }
}