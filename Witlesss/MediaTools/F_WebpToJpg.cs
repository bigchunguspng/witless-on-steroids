using System.Collections.Generic;
using System.IO;

namespace Witlesss.MediaTools
{
    // ffmpeg -i "input.webp" output.jpg
    public class F_WebpToJpg : F_SimpleTask
    {
        public F_WebpToJpg(string input, string extension) : base(input, Path.ChangeExtension(input, extension)) { }

        public override IList<string> CreateArguments() => new[] { "-i", Input, Output };
    }
}