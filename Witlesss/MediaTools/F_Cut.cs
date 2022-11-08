using System;
using System.Collections.Generic;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -ss 00:00:05 -t 00:00:15 output.mp4
    // ffmpeg -i "input.mp4" -ss 00:00:05             output.mp4
    public class F_Cut : F_SimpleTask
    {
        private readonly TimeSpan _start, _length;

        public F_Cut(string input, TimeSpan start, TimeSpan length) : base(input, SetOutName(input, "-C"))
        {
            _start = start;
            _length = length;
        }

        public override IList<string> CreateArguments()
        {
            var result = new List<string>
            {
                "-i", Input,
                "-ss", $"{_start:c}",
                "-t", $"{_length:c}",
                Output
            };
            if (_length == TimeSpan.Zero)
            {
                result[4] = "";
                result[5] = "";
            }
            return RemoveEmpties(result);
        }
    }
}