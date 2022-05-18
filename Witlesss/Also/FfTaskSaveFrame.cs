using System;
using System.Collections.Generic;
using System.Globalization;

namespace Witlesss.Also
{
    // ffmpeg -nostdin -y -loglevel info -ss 0.066 -i "input.mp4" -vframes 1 "F-0002.jpg"
    public class FfTaskSaveFrame : FfTask
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;
        private readonly TimeSpan _seekSpan;

        public FfTaskSaveFrame(string inputFilePath, string outputFilePath, TimeSpan seekSpan)
        {
            _inputFilePath = inputFilePath;
            _outputFilePath = outputFilePath;
            _seekSpan = seekSpan;
        }

        public override IList<string> CreateArguments() => new[]
        {
            "-nostdin",
            "-y",
            "-loglevel",
            "info",
            "-ss",
            _seekSpan.TotalSeconds.ToString(CultureInfo.InvariantCulture),
            "-i",
            _inputFilePath ?? "",
            "-vframes",
            "1",
            _outputFilePath ?? ""
        };
    }
}