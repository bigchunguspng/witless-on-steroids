using System.Collections.Generic;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -vf reverse -af areverse output.mp4
    // ffmpeg -i "input.mp3"             -af areverse output.mp3
    public class F_Reverse : F_SimpleTask
    {
        private readonly bool _video;

        public F_Reverse(string input, MediaType type) : base(input, SetOutName(input, "-R"))
        {
            _video = type > MediaType.Audio;
        }

        public override IList<string> CreateArguments()
        {
            var result = new List<string> {"-i", Input, "", "", "-af", "areverse", Output};
            if (_video)
            {
                result[2] = "-vf";
                result[3] = "reverse";
            }
            return RemoveEmpties(result);
        }
    }
}