using System.Collections.Generic;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Tasks;

namespace Witlesss.Also
{
    public class FfTaskWebpToJpg : FfMpegTaskBase<int>
    {
        protected readonly string InputFilePath;
        protected readonly string OutputFilePath;

        public FfTaskWebpToJpg(string inputFilePath, out string outputFilePath, string extension)
        {
            InputFilePath = inputFilePath;
            OutputFilePath = inputFilePath.Remove(inputFilePath.LastIndexOf('.')) + extension;
            outputFilePath = OutputFilePath;
        }
        
        public override IList<string> CreateArguments() => new[]
        {
            "-i",
            InputFilePath ?? "",
            OutputFilePath ?? ""
        };

        public override async Task<int> ExecuteCommandAsync(IFfProcess ffProcess)
        {
            await ffProcess.Task;
            return 0;
        }
    }
}