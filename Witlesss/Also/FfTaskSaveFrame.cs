using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.Also
{
    public class FfTaskSaveFrame : FfMpegTaskBase<int>
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

        public override async Task<int> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return 0;
        }
    }
}