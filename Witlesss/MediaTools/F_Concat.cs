using System.Collections.Generic;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input_a.mp4" -i "input_b.mp4" -filter_complex "[0:v][0:a][1:v][1:a]concat=n=2:v=1:a=1" output.mp4
    // ffmpeg -i "input_a.mp4" -i "input_b.mp4" -filter_complex "[0:0][1:0]concat=n=2:v=1:a=0"           output.mp4
    // ffmpeg -i "input_a.mp3" -i "input_b.mp3" -filter_complex "[0:0][1:0]concat=n=2:v=0:a=1"           output.mp3
    public class F_Concat : F_Base
    {
        private readonly string _input_a, _input_b, _output;
        private readonly MediaType _type;
        
        public F_Concat(string inputA, string inputB, out string output, MediaType type)
        {
            _input_a = inputA;
            _input_b = inputB;

            output = SetOutName(_input_a, "-A");

            _type = type;
            _output = output;
        }

        public override IList<string> CreateArguments() => new []
            {
                "-i", _input_a,
                "-i", _input_b,
                "-filter_complex", Filters[_type], _output
            };

        private static readonly Dictionary<MediaType, string> Filters = new()
        {
            {MediaType.Audio, "[0:0][1:0]concat=n=2:v=0:a=1"},
            {MediaType.Video, "[0:0][1:0]concat=n=2:v=1:a=0"},
            {MediaType.Movie, "[0:v][0:a][1:v][1:a]concat=n=2:v=1:a=1"}
        };
    }
}