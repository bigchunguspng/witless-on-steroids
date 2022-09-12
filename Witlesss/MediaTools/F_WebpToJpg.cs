using System.Collections.Generic;
using static Witlesss.Extension;

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
            Output = GetFileName(input) + extension;
            output = Output;
        }

        public override IList<string> CreateArguments() => new[] {"-i", Input, Output};
    }
}