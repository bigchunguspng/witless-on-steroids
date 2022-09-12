using System;
using System.Collections.Generic;
using static Witlesss.Extension;

namespace Witlesss.MediaTools
{
    // ffmpeg -ss 0.066 -i "input.mp4" -vframes 1 "F-0002.jpg"
    public class F_SaveFrame : F_Base
    {
        private readonly string _input, _output;
        private readonly TimeSpan _seekSpan;

        public F_SaveFrame(string input, string output, TimeSpan seekSpan)
        {
            _input = input;
            _output = output;
            _seekSpan = seekSpan;
        }

        public override IList<string> CreateArguments() => new[]
        {
            "-ss", FormatDouble(_seekSpan.TotalSeconds),
            "-i", _input, "-vframes", "1", _output
        };
    }
}