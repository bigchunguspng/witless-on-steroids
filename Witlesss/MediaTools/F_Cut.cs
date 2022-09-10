using System;
using System.Collections.Generic;
using static Witlesss.Extension;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.mp4" -ss 00:00:05 -t 00:00:15 output.mp4
    // ffmpeg -i "input.mp4" -ss 00:00:05             output.mp4
    public class F_Cut : F_Base
    {
        private readonly string _input;
        private readonly string _output;
        private readonly TimeSpan _start;
        private readonly TimeSpan _length;
        
        public F_Cut(string inputFilePath, out string outputFilePath, TimeSpan start, TimeSpan length)
        {
            _input = inputFilePath;
            string extension = GetFileExtension(inputFilePath);
            _output = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + "-C" + extension;
            outputFilePath = _output;
            
            _start = start;
            _length = length;
        }

        public override IList<string> CreateArguments()
        {
            var result = new List<string>
            {
                "-i", 
                _input,
                "-ss", $"{_start:c}",
                "-t", $"{_length:c}",
                _output
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