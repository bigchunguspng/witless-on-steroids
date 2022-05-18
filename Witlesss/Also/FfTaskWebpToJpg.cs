using System.Collections.Generic;

namespace Witlesss.Also
{
    // ffmpeg -i "input.webp" "output.jpg"
    public class FfTaskWebpToJpg : FfTask
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
    }
}