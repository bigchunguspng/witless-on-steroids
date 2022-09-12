using System.Collections.Generic;
using static Witlesss.Extension;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -vf reverse -af areverse output.mp4
    // ffmpeg -i "input.mp3"             -af areverse output.mp3
    public class F_Reverse : F_Base
    {
        private readonly string _input, _output;
        private readonly bool _video;
        
        public F_Reverse(string input, out string output)
        {
            SetOutName(input, out output, "-R", out _video);
            
            _input = input;
            _output = output;
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