using System.Collections.Generic;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.Also
{
    public class FfTaskWebpToJpg : FfMpegTaskBase<int>
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;

        public FfTaskWebpToJpg(string inputFilePath, out string outputFilePath, string extension)
        {
            _inputFilePath = inputFilePath;
            _outputFilePath = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + extension;
            outputFilePath = _outputFilePath;
        }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-i",
            _inputFilePath ?? "",
            _outputFilePath ?? ""
        };

        public override async Task<int> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return 0;
        }
    }
}