using System.Collections.Generic;
using System.IO;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webp" output.jpg
    public class F_WebpToJpg : F_Base
    {
        protected readonly string Input;
        protected readonly string Output;

        public F_WebpToJpg(string input, out string output, string extension)
        {
            Input = input;
            Output = Path.ChangeExtension(input, extension);
            output = Output;
        }

        public override IList<string> CreateArguments() => new[] {"-i", Input, Output};
    }
}