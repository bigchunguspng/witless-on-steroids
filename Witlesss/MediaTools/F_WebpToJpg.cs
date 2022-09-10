using System.Collections.Generic;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webp" "output.jpg"
    public class F_WebpToJpg : F_Base
    {
        protected readonly string InputFilePath;
        protected readonly string OutputFilePath;

        public F_WebpToJpg(string inputFilePath, out string outputFilePath, string extension)
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